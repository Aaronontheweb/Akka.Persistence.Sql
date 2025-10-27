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

namespace Akka.Persistence.Sql.DdlGenerator
{
    public class DdlGenerator
    {
        private readonly string _outputPath;

        public DdlGenerator(string outputPath)
        {
            _outputPath = outputPath;
        }

        public async Task GenerateAll()
        {
            await GenerateForProvider("SqlServer");
            await GenerateForProvider("PostgreSQL");
            await GenerateForProvider("MySQL");
            await GenerateForProvider("SQLite");
        }

        public async Task GenerateForProvider(string providerName)
        {
            Console.WriteLine($"Generating DDL for {providerName}...");

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

                // Generate DDL files
                var providerDir = Path.Combine(_outputPath, providerName.ToLowerInvariant());
                Directory.CreateDirectory(providerDir);

                // Generate Journal DDL
                var journalSql = await GenerateJournalDdl(container);
                await File.WriteAllTextAsync(
                    Path.Combine(providerDir, "journal.sql"),
                    journalSql);
                Console.WriteLine($"  ✓ journal.sql");

                // Generate Journal Tags DDL
                var tagsSql = await GenerateJournalTagsDdl(container);
                await File.WriteAllTextAsync(
                    Path.Combine(providerDir, "journal-tags.sql"),
                    tagsSql);
                Console.WriteLine($"  ✓ journal-tags.sql");

                // Generate Snapshot DDL
                var snapshotSql = await GenerateSnapshotDdl(container);
                await File.WriteAllTextAsync(
                    Path.Combine(providerDir, "snapshot.sql"),
                    snapshotSql);
                Console.WriteLine($"  ✓ snapshot.sql");

                // Generate Metadata DDL (for compatibility mode)
                var metadataSql = await GenerateMetadataDdl(container);
                await File.WriteAllTextAsync(
                    Path.Combine(providerDir, "metadata.sql"),
                    metadataSql);
                Console.WriteLine($"  ✓ metadata.sql");

                Console.WriteLine($"✓ Completed {providerName}");
            }
            finally
            {
                if (container != null)
                {
                    await container.DisposeAsync();
                }
            }
        }

        private async Task<string> GenerateJournalDdl(ITestContainer container)
        {
            var sql = new StringBuilder();
            sql.AppendLine("-- Journal Table DDL");
            sql.AppendLine($"-- Generated for {container.ProviderName}");
            sql.AppendLine("-- This table stores all persisted events");
            sql.AppendLine();

            // Capture table creation first (without footer)
            // Drop table first, then use TableOptions.None to force CREATE TABLE generation
            var tableSql = await CaptureTableCreationSql(
                container,
                async (dataConnection, connection, config) =>
                {
                    await dataConnection.DropTableAsync<JournalRow>(throwExceptionIfNotExists: false);
                    await connection.CreateTableAsync<JournalRow>(
                        TableOptions.None,
                        null); // No footer yet
                });

            sql.Append(tableSql);

            // Then add the footer SQL as a comment/separate section
            var config = await CreateJournalConfig(container);
            var footer = config.GenerateJournalFooter();
            if (!string.IsNullOrEmpty(footer))
            {
                sql.AppendLine();
                sql.AppendLine("-- Additional constraints and indexes:");
                sql.AppendLine(footer);
            }

            return sql.ToString();
        }

        private async Task<string> GenerateJournalTagsDdl(ITestContainer container)
        {
            var sql = new StringBuilder();
            sql.AppendLine("-- Journal Tags Table DDL");
            sql.AppendLine($"-- Generated for {container.ProviderName}");
            sql.AppendLine("-- This table stores tags in normalized form (TagMode.TagTable)");
            sql.AppendLine();

            // Capture table creation first (without footer)
            // Drop table first, then use TableOptions.None to force CREATE TABLE generation
            var tableSql = await CaptureTableCreationSql(
                container,
                async (dataConnection, connection, config) =>
                {
                    await dataConnection.DropTableAsync<JournalTagRow>(throwExceptionIfNotExists: false);
                    await connection.CreateTableAsync<JournalTagRow>(
                        TableOptions.None,
                        null); // No footer yet
                });

            sql.Append(tableSql);

            // Then add the footer SQL as a comment/separate section
            var config = await CreateJournalConfig(container);
            var footer = config.GenerateTagFooter();
            if (!string.IsNullOrEmpty(footer))
            {
                sql.AppendLine();
                sql.AppendLine("-- Additional constraints and indexes:");
                sql.AppendLine(footer);
            }

            return sql.ToString();
        }

        private async Task<string> GenerateSnapshotDdl(ITestContainer container)
        {
            var sql = new StringBuilder();
            sql.AppendLine("-- Snapshot Table DDL");
            sql.AppendLine($"-- Generated for {container.ProviderName}");
            sql.AppendLine("-- This table stores actor state snapshots");
            sql.AppendLine();

            // Determine table-mapping based on provider
            var tableMapping = container.ProviderName.ToLowerInvariant() switch
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

        private async Task<string> GenerateMetadataDdl(ITestContainer container)
        {
            var sql = new StringBuilder();
            sql.AppendLine("-- Journal Metadata Table DDL");
            sql.AppendLine($"-- Generated for {container.ProviderName}");
            sql.AppendLine("-- This table is used for delete-compatibility-mode");
            sql.AppendLine();

            // Drop table first, then use TableOptions.None to force CREATE TABLE generation
            var capturedSql = await CaptureTableCreationSql(
                container,
                async (dataConnection, connection, config) =>
                {
                    await dataConnection.DropTableAsync<JournalMetaData>(throwExceptionIfNotExists: false);
                    await connection.CreateTableAsync<JournalMetaData>(
                        TableOptions.None,
                        null);
                });

            sql.Append(capturedSql);
            return sql.ToString();
        }

        private Task<JournalConfig> CreateJournalConfig(ITestContainer container)
        {
            // Determine table-mapping based on provider
            var tableMapping = container.ProviderName.ToLowerInvariant() switch
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
                }}
            ")
            .WithFallback(SqlPersistence.DefaultConfiguration)
            .GetConfig("akka.persistence.journal.sql");

            return Task.FromResult(new JournalConfig(hoconConfig));
        }

        private async Task<string> CaptureTableCreationSql(
            ITestContainer container,
            Func<DataConnection, AkkaDataConnection, JournalConfig, Task> createAction)
        {
            var capturedSql = new StringBuilder();

            var config = await CreateJournalConfig(container);

            // Create connection with SQL tracing - capture ALL SQL for debugging
            var options = new DataOptions()
                .UseConnectionString(container.ProviderName, container.ConnectionString)
                .UseTracing(info =>
                {
                    if (info.TraceInfoStep == TraceInfoStep.BeforeExecute &&
                        info.CommandText != null)
                    {
                        Console.WriteLine($"[TRACE] {info.CommandText.Substring(0, Math.Min(100, info.CommandText.Length))}...");

                        if ((info.CommandText.Contains("CREATE TABLE") ||
                             info.CommandText.Contains("DROP TABLE") ||
                             info.CommandText.Contains("ALTER TABLE") ||
                             info.CommandText.Contains("CREATE INDEX") ||
                             info.CommandText.Contains("CREATE UNIQUE INDEX")))
                        {
                            // Only capture CREATE TABLE, not DROP TABLE
                            if (!info.CommandText.Contains("DROP TABLE"))
                            {
                                capturedSql.AppendLine(info.CommandText);
                                capturedSql.AppendLine();
                            }
                        }
                    }
                });

            await using var dataConnection = new DataConnection(options);
            var connection = new AkkaDataConnection(container.ProviderName, dataConnection);

            // Execute the table creation to capture SQL (with drops to force execution)
            await createAction(dataConnection, connection, config);

            // If no SQL was captured via tracing, generate DDL from schema
            if (capturedSql.Length == 0)
            {
                Console.WriteLine("[DEBUG] No SQL captured via tracing, generating from SQL Builder...");
                // Use Linq2Db's SQL builder to generate CREATE TABLE statement
                try
                {
                    var schema = connection.GetSchema();
                    var journalTable = schema.Tables.FirstOrDefault(t => t.TableName.Contains("EventJournal") || t.TableName.Contains("tags") || t.TableName.Contains("SnapshotStore") || t.TableName.Contains("JournalMetaData"));
                    if (journalTable != null)
                    {
                        capturedSql.AppendLine($"-- Table: {journalTable.TableName}");
                        capturedSql.AppendLine($"-- Schema introspected after creation");
                        capturedSql.AppendLine($"-- Columns: {string.Join(", ", journalTable.Columns.Select(c => $"{c.ColumnName} ({c.ColumnType})"))}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DEBUG] Schema introspection failed: {ex.Message}");
                }
            }

            return capturedSql.ToString();
        }

        private async Task<string> CaptureSnapshotTableCreationSql(
            ITestContainer container,
            SnapshotConfig config)
        {
            var capturedSql = new StringBuilder();

            // Create connection with SQL tracing
            var options = new DataOptions()
                .UseConnectionString(container.ProviderName, container.ConnectionString)
                .UseTracing(info =>
                {
                    if (info.TraceInfoStep == TraceInfoStep.BeforeExecute &&
                        info.CommandText != null &&
                        (info.CommandText.Contains("CREATE TABLE") ||
                         info.CommandText.Contains("DROP TABLE") ||
                         info.CommandText.Contains("ALTER TABLE") ||
                         info.CommandText.Contains("CREATE INDEX") ||
                         info.CommandText.Contains("CREATE UNIQUE INDEX")))
                    {
                        // Only capture CREATE TABLE, not DROP TABLE
                        if (!info.CommandText.Contains("DROP TABLE"))
                        {
                            capturedSql.AppendLine(info.CommandText);
                            capturedSql.AppendLine();
                        }
                    }
                });

            await using var dataConnection = new DataConnection(options);
            var connection = new AkkaDataConnection(container.ProviderName, dataConnection);

            // Drop table first, then use TableOptions.None to force CREATE TABLE generation
            // SQL Server uses DateTime, others use Long (ticks)
            if (connection.UseDateTime)
            {
                await dataConnection.DropTableAsync<DateTimeSnapshotRow>(throwExceptionIfNotExists: false);
                await connection.CreateTableAsync<DateTimeSnapshotRow>(
                    TableOptions.None,
                    null); // No footer yet
            }
            else
            {
                await dataConnection.DropTableAsync<LongSnapshotRow>(throwExceptionIfNotExists: false);
                await connection.CreateTableAsync<LongSnapshotRow>(
                    TableOptions.None,
                    null); // No footer yet
            }

            return capturedSql.ToString();
        }
    }
}
