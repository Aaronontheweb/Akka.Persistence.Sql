// -----------------------------------------------------------------------
//  <copyright file="DdlGenerator.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2023 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using System.Text;
using Akka.Persistence.Sql.Config;
using Akka.Persistence.Sql.Db;
using Akka.Persistence.Sql.Journal.Dao;
using Akka.Persistence.Sql.Journal.Types;
using Akka.Persistence.Sql.Snapshot;
using Akka.Persistence.Sql.Tests.Common.Containers;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;

namespace Akka.Persistence.Sql.DdlGenerator
{
    /// <summary>
    /// Specifies the table naming convention for DDL generation.
    /// </summary>
    public enum TableMappingMode
    {
        /// <summary>
        /// Generate DDL for the default table mapping (for new deployments).
        /// Uses table names like 'journal', 'snapshot' with snake_case columns.
        /// </summary>
        Default,

        /// <summary>
        /// Generate DDL for legacy compatibility mappings (for migration from old plugins).
        /// Uses provider-specific table names like 'EventJournal' (SQL Server), 'event_journal' (PostgreSQL).
        /// </summary>
        Compat
    }

    public class DdlGenerator
    {
        private readonly string _outputPath;

        public DdlGenerator(string outputPath)
        {
            _outputPath = outputPath;
        }

        public async Task GenerateAll(TableMappingMode mappingMode = TableMappingMode.Compat)
        {
            await GenerateForProvider("SqlServer", mappingMode);
            await GenerateForProvider("PostgreSQL", mappingMode);
            await GenerateForProvider("MySQL", mappingMode);
            await GenerateForProvider("SQLite", mappingMode);
        }

        public async Task GenerateForProvider(string providerName, TableMappingMode mappingMode = TableMappingMode.Compat)
        {
            var mappingName = mappingMode == TableMappingMode.Default ? "default" : "compat";
            Console.WriteLine($"Generating DDL for {providerName} ({mappingName} mapping)...");

            ITestContainer? container = null;
            try
            {
                // Create appropriate container
                container = providerName.ToLowerInvariant() switch
                {
                    "sqlserver" => new SqlServerContainer(),
                    "postgresql" => new PostgreSqlContainer(),
                    "mysql" => new MySqlContainer(),
                    "sqlite" => new MsSqliteContainer(), // Use Microsoft.Data.Sqlite
                    _ => throw new ArgumentException($"Unknown provider: {providerName}")
                };

                await container.InitializeAsync();

                // Generate DDL files in appropriate subdirectory
                var providerDir = Path.Combine(_outputPath, mappingName, providerName.ToLowerInvariant());
                Directory.CreateDirectory(providerDir);

                // Generate Journal DDL
                var journalSql = await GenerateJournalDdl(container, mappingMode);
                await File.WriteAllTextAsync(
                    Path.Combine(providerDir, "journal.sql"),
                    journalSql);
                Console.WriteLine($"  ✓ journal.sql");

                // Generate Journal Tags DDL
                var tagsSql = await GenerateJournalTagsDdl(container, mappingMode);
                await File.WriteAllTextAsync(
                    Path.Combine(providerDir, "journal-tags.sql"),
                    tagsSql);
                Console.WriteLine($"  ✓ journal-tags.sql");

                // Generate Snapshot DDL
                var snapshotSql = await GenerateSnapshotDdl(container, mappingMode);
                await File.WriteAllTextAsync(
                    Path.Combine(providerDir, "snapshot.sql"),
                    snapshotSql);
                Console.WriteLine($"  ✓ snapshot.sql");

                // Generate Metadata DDL (for compatibility mode)
                var metadataSql = await GenerateMetadataDdl(container, mappingMode);
                await File.WriteAllTextAsync(
                    Path.Combine(providerDir, "metadata.sql"),
                    metadataSql);
                Console.WriteLine($"  ✓ metadata.sql");

                Console.WriteLine($"✓ Completed {providerName} ({mappingName})");
            }
            finally
            {
                if (container != null)
                {
                    await container.DisposeAsync();
                }
            }
        }

        private async Task<string> GenerateJournalDdl(ITestContainer container, TableMappingMode mappingMode)
        {
            var sql = new StringBuilder();
            var mappingName = mappingMode == TableMappingMode.Default ? "default" : "compat";
            sql.AppendLine("-- Journal Table DDL");
            sql.AppendLine($"-- Generated for {container.ProviderName}");
            sql.AppendLine($"-- Table mapping: {mappingName}");
            sql.AppendLine("-- This table stores all persisted events");
            sql.AppendLine();

            var config = await CreateJournalConfig(container, mappingMode: mappingMode);

            // Capture table creation first (without footer)
            var tableSql = await CaptureTableCreationSql(
                container,
                config,
                async (connection) =>
                {
                    await connection.CreateTableAsync<JournalRow>(
                        TableOptions.None,
                        null); // No footer yet
                },
                "journal",
                config.TableConfig.EventJournalTable.Name);

            sql.Append(tableSql);

            // Then add the footer SQL as a comment/separate section
            var footer = config.GenerateJournalFooter();
            if (!string.IsNullOrEmpty(footer))
            {
                sql.AppendLine();
                sql.AppendLine("-- Additional constraints and indexes:");
                sql.AppendLine(footer);
            }

            return sql.ToString();
        }

        private async Task<string> GenerateJournalTagsDdl(ITestContainer container, TableMappingMode mappingMode)
        {
            var sql = new StringBuilder();
            var mappingName = mappingMode == TableMappingMode.Default ? "default" : "compat";
            sql.AppendLine("-- Journal Tags Table DDL");
            sql.AppendLine($"-- Generated for {container.ProviderName}");
            sql.AppendLine($"-- Table mapping: {mappingName}");
            sql.AppendLine("-- This table stores tags in normalized form (TagMode.TagTable)");
            sql.AppendLine();

            var config = await CreateJournalConfig(container, mappingMode: mappingMode);

            // Capture table creation first (without footer)
            var tableSql = await CaptureTableCreationSql(
                container,
                config,
                async (connection) =>
                {
                    await connection.CreateTableAsync<JournalTagRow>(
                        TableOptions.None,
                        null); // No footer yet
                },
                "tags",
                config.TableConfig.TagTable.Name);

            sql.Append(tableSql);

            // Then add the footer SQL as a comment/separate section
            var footer = config.GenerateTagFooter();
            if (!string.IsNullOrEmpty(footer))
            {
                sql.AppendLine();
                sql.AppendLine("-- Additional constraints and indexes:");
                sql.AppendLine(footer);
            }

            return sql.ToString();
        }

        private async Task<string> GenerateSnapshotDdl(ITestContainer container, TableMappingMode mappingMode)
        {
            var sql = new StringBuilder();
            var mappingName = mappingMode == TableMappingMode.Default ? "default" : "compat";
            sql.AppendLine("-- Snapshot Table DDL");
            sql.AppendLine($"-- Generated for {container.ProviderName}");
            sql.AppendLine($"-- Table mapping: {mappingName}");
            sql.AppendLine("-- This table stores actor state snapshots");
            sql.AppendLine();

            // Determine table-mapping based on mode
            var tableMapping = mappingMode == TableMappingMode.Default
                ? "default"
                : container.ProviderName.ToLowerInvariant() switch
                {
                    var p when p.Contains("sqlserver") => "sql-server",
                    var p when p.Contains("postgre") => "postgresql",  // PostgreSQL* contains "postgre"
                    var p when p.Contains("mysql") => "mysql",
                    var p when p.Contains("sqlite") => "sqlite",
                    _ => "sql-server"
                };

            // Create snapshot config
            var hoconConfig = Akka.Configuration.ConfigurationFactory.ParseString($@"
                akka.persistence.snapshot-store.sql {{
                    connection-string = ""{container.ConnectionString}""
                    provider-name = ""{container.ProviderName}""
                    table-mapping = {tableMapping}
                }}
            ")
            .WithFallback(SqlPersistence.DefaultConfiguration)
            .GetConfig("akka.persistence.snapshot-store.sql");

            var snapshotConfig = new SnapshotConfig(hoconConfig);

            // Capture table creation first (without footer)
            var tableSql = await CaptureSnapshotTableCreationSql(container, snapshotConfig);
            sql.Append(tableSql);

            // Then add the footer SQL as a comment/separate section
            var footer = snapshotConfig.GenerateSnapshotFooter();
            if (!string.IsNullOrEmpty(footer))
            {
                sql.AppendLine();
                sql.AppendLine("-- Additional constraints and indexes:");
                sql.AppendLine(footer);
            }

            return sql.ToString();
        }

        private async Task<string> GenerateMetadataDdl(ITestContainer container, TableMappingMode mappingMode)
        {
            var sql = new StringBuilder();
            var mappingName = mappingMode == TableMappingMode.Default ? "default" : "compat";
            sql.AppendLine("-- Journal Metadata Table DDL");
            sql.AppendLine($"-- Generated for {container.ProviderName}");
            sql.AppendLine($"-- Table mapping: {mappingName}");
            sql.AppendLine("-- This table is used for delete-compatibility-mode");
            sql.AppendLine();

            // Enable compatibility mode to ensure metadata table mapping is applied
            var config = await CreateJournalConfig(container, enableCompatibilityMode: true, mappingMode: mappingMode);

            // Use TableOptions.None to force CREATE TABLE generation
            var capturedSql = await CaptureTableCreationSql(
                container,
                config,
                async (connection) =>
                {
                    await connection.CreateTableAsync<JournalMetaData>(
                        TableOptions.None,
                        null);
                },
                "metadata",
                config.TableConfig.MetadataTable.Name);

            sql.Append(capturedSql);
            return sql.ToString();
        }

        private Task<JournalConfig> CreateJournalConfig(
            ITestContainer container,
            bool enableCompatibilityMode = false,
            TableMappingMode mappingMode = TableMappingMode.Compat)
        {
            // Determine table-mapping based on mode
            var tableMapping = mappingMode == TableMappingMode.Default
                ? "default"
                : container.ProviderName.ToLowerInvariant() switch
                {
                    var p when p.Contains("sqlserver") => "sql-server",
                    var p when p.Contains("postgre") => "postgresql",  // PostgreSQL* contains "postgre"
                    var p when p.Contains("mysql") => "mysql",
                    var p when p.Contains("sqlite") => "sqlite",
                    _ => "sql-server"
                };

            var hoconConfig = Akka.Configuration.ConfigurationFactory.ParseString($@"
                akka.persistence.journal.sql {{
                    connection-string = ""{container.ConnectionString}""
                    provider-name = ""{container.ProviderName}""
                    table-mapping = {tableMapping}
                    delete-compatibility-mode = {enableCompatibilityMode.ToString().ToLowerInvariant()}
                }}
            ")
            .WithFallback(SqlPersistence.DefaultConfiguration)
            .GetConfig("akka.persistence.journal.sql");

            return Task.FromResult(new JournalConfig(hoconConfig));
        }

        private async Task<string> CaptureTableCreationSql(
            ITestContainer container,
            JournalConfig config,
            Func<AkkaDataConnection, Task> createAction,
            string tableType,
            string? expectedTableName = null)
        {
            var capturedSql = new StringBuilder();

            // Create a properly configured DataConnectionFactory with table/column mappings
            var factory = new AkkaPersistenceDataConnectionFactory(config);
            var connection = factory.GetConnection();

            // Execute the table creation to ensure it exists in the database
            // The DataConnection now has proper MappingSchema with table/column name mappings
            await createAction(connection);

            // Generate CREATE TABLE statement from schema introspection
            // The schema now has the correctly mapped table names from the MappingSchema
            Console.WriteLine($"[INFO] Generating CREATE TABLE DDL from mapped schema...");
            try
            {
                var schema = connection.GetSchema();

                // List all tables for debugging
                Console.WriteLine($"[DEBUG] Found {schema.Tables.Count} tables in schema:");
                foreach (var t in schema.Tables)
                {
                    Console.WriteLine($"[DEBUG]   - {t.SchemaName}.{t.TableName}");
                }

                // Find the table - it should now have the correct configured name
                var table = expectedTableName != null
                    ? schema.Tables.FirstOrDefault(t =>
                        t.TableName.Equals(expectedTableName, StringComparison.OrdinalIgnoreCase))
                    : schema.Tables.FirstOrDefault(t =>
                        t.TableName.ToLowerInvariant().Contains("event") ||
                        t.TableName.ToLowerInvariant().Contains("journal") ||
                        t.TableName.ToLowerInvariant().Contains("tags") ||
                        t.TableName.ToLowerInvariant().Contains("snapshot") ||
                        t.TableName.ToLowerInvariant().Contains("metadata"));

                if (table != null)
                {
                    Console.WriteLine($"[INFO] Found table: {table.SchemaName}.{table.TableName}");
                    // Generate CREATE TABLE statement - table and column names are already correct from mapping
                    var createTable = GenerateCreateTableFromSchema(table, container.ProviderName);
                    capturedSql.AppendLine(createTable);
                    capturedSql.AppendLine();
                }
                else
                {
                    Console.WriteLine($"[ERROR] Table for {tableType} not found in schema");
                    capturedSql.AppendLine($"-- ERROR: Table for {tableType} not found in database schema");
                    capturedSql.AppendLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Schema introspection failed: {ex.Message}");
                capturedSql.AppendLine("-- ERROR: Unable to retrieve schema information");
                capturedSql.AppendLine($"-- {ex.Message}");
                capturedSql.AppendLine();
            }
            finally
            {
                await connection.DisposeAsync();
            }

            return capturedSql.ToString();
        }

        private string GenerateCreateTableFromSchema(
            LinqToDB.SchemaProvider.TableSchema table,
            string providerName)
        {
            var sql = new StringBuilder();
            var isSqlServer = providerName.ToLowerInvariant().Contains("sqlserver");
            var isPostgreSql = providerName.ToLowerInvariant().Contains("postgre");
            var isMySql = providerName.ToLowerInvariant().Contains("mysql");
            var isSqlite = providerName.ToLowerInvariant().Contains("sqlite");

            // Table and column names are already mapped correctly via MappingSchema
            var fullTableName = string.IsNullOrEmpty(table.SchemaName)
                ? QuoteIdentifier(table.TableName, providerName)
                : $"{QuoteIdentifier(table.SchemaName, providerName)}.{QuoteIdentifier(table.TableName, providerName)}";

            // Add IF NOT EXISTS check based on provider
            if (isSqlServer)
            {
                sql.AppendLine($"IF NOT EXISTS (");
                sql.AppendLine($"    SELECT 1");
                sql.AppendLine($"    FROM sys.tables");
                sql.AppendLine($"    WHERE");
                if (!string.IsNullOrEmpty(table.SchemaName))
                {
                    sql.AppendLine($"        SCHEMA_NAME(schema_id) = '{table.SchemaName}' AND");
                }
                sql.AppendLine($"        name = '{table.TableName}'");
                sql.AppendLine($")");
                sql.AppendLine($"BEGIN");
                sql.AppendLine($"    CREATE TABLE {fullTableName} (");
            }
            else if (isPostgreSql)
            {
                sql.AppendLine($"CREATE TABLE IF NOT EXISTS {fullTableName} (");
            }
            else if (isMySql)
            {
                sql.AppendLine($"CREATE TABLE IF NOT EXISTS {fullTableName} (");
            }
            else if (isSqlite)
            {
                sql.AppendLine($"CREATE TABLE IF NOT EXISTS {fullTableName} (");
            }
            else
            {
                sql.AppendLine($"CREATE TABLE {fullTableName} (");
            }

            // Add columns - names are already correctly mapped
            var columnDefinitions = new List<string>();
            foreach (var column in table.Columns)
            {
                var columnDef = new StringBuilder();
                columnDef.Append($"    {QuoteIdentifier(column.ColumnName, providerName)} {column.ColumnType}");

                if (!column.IsNullable)
                {
                    columnDef.Append(" NOT NULL");
                }

                if (column.IsIdentity && isSqlServer)
                {
                    columnDef.Append(" IDENTITY");
                }

                columnDefinitions.Add(columnDef.ToString());
            }

            // Add primary key constraint
            var pkColumns = table.Columns.Where(c => c.IsPrimaryKey).ToList();
            if (pkColumns.Any())
            {
                var pkColumnNames = string.Join(", ", pkColumns.Select(c => QuoteIdentifier(c.ColumnName, providerName)));
                columnDefinitions.Add($"    CONSTRAINT {QuoteIdentifier($"PK_{table.TableName}", providerName)} PRIMARY KEY ({pkColumnNames})");
            }

            sql.AppendLine(string.Join($",{Environment.NewLine}", columnDefinitions));

            // Close the CREATE TABLE statement
            if (isSqlServer)
            {
                sql.AppendLine("    );");
                sql.AppendLine("END");
            }
            else
            {
                sql.AppendLine(");");
            }

            return sql.ToString();
        }

        private string QuoteIdentifier(string identifier, string providerName)
        {
            var isPostgreSql = providerName.ToLowerInvariant().Contains("postgre");
            var isSqlServer = providerName.ToLowerInvariant().Contains("sqlserver");
            var isMySql = providerName.ToLowerInvariant().Contains("mysql");

            if (isPostgreSql)
                return $"\"{identifier}\"";
            else if (isSqlServer)
                return $"[{identifier}]";
            else if (isMySql)
                return $"`{identifier}`";
            else
                return identifier; // SQLite doesn't require quotes
        }

        private async Task<string> CaptureSnapshotTableCreationSql(
            ITestContainer container,
            SnapshotConfig config)
        {
            var capturedSql = new StringBuilder();

            // Create a properly configured DataConnectionFactory with table/column mappings for snapshots
            var factory = new AkkaPersistenceDataConnectionFactory(config);
            var connection = factory.GetConnection();

            // Create the snapshot table (SQL Server uses DateTime, others use Long ticks)
            if (connection.UseDateTime)
            {
                await connection.CreateTableAsync<DateTimeSnapshotRow>(
                    TableOptions.None,
                    null);
            }
            else
            {
                await connection.CreateTableAsync<LongSnapshotRow>(
                    TableOptions.None,
                    null);
            }

            // Generate CREATE TABLE statement from schema introspection
            Console.WriteLine("[INFO] Generating snapshot CREATE TABLE DDL from mapped schema...");
            try
            {
                var schema = connection.GetSchema();

                // List all tables for debugging
                Console.WriteLine($"[DEBUG] Found {schema.Tables.Count} tables in schema:");
                foreach (var t in schema.Tables)
                {
                    Console.WriteLine($"[DEBUG]   - {t.SchemaName}.{t.TableName}");
                }

                var table = schema.Tables.FirstOrDefault(t => t.TableName.ToLowerInvariant().Contains("snapshot"));

                if (table != null)
                {
                    Console.WriteLine($"[INFO] Found snapshot table: {table.SchemaName}.{table.TableName}");
                    var createTable = GenerateCreateTableFromSchema(table, container.ProviderName);
                    capturedSql.AppendLine(createTable);
                    capturedSql.AppendLine();
                }
                else
                {
                    Console.WriteLine("[ERROR] Snapshot table not found in schema");
                    capturedSql.AppendLine("-- ERROR: Snapshot table not found in database schema");
                    capturedSql.AppendLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Schema introspection failed: {ex.Message}");
                capturedSql.AppendLine("-- ERROR: Unable to retrieve schema information");
                capturedSql.AppendLine($"-- {ex.Message}");
                capturedSql.AppendLine();
            }
            finally
            {
                await connection.DisposeAsync();
            }

            return capturedSql.ToString();
        }
    }
}
