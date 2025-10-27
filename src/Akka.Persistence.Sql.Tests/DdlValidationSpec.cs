// -----------------------------------------------------------------------
//  <copyright file="DdlValidationSpec.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2023 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;
using Akka.Persistence.Sql.Tests.Common.Containers;
using FluentAssertions;
using LinqToDB.Data;
using Xunit;
using Xunit.Abstractions;

namespace Akka.Persistence.Sql.Tests
{
    /// <summary>
    /// Validates that the generated DDL files in docs/ddl/ execute successfully
    /// against real database instances.
    /// </summary>
    public class DdlValidationSpec
    {
        private readonly ITestOutputHelper _output;

        public DdlValidationSpec(ITestOutputHelper output)
        {
            _output = output;
        }

        private string GetDdlPath(string provider, string filename)
        {
            // Navigate from test assembly location to repository root
            var assemblyPath = Path.GetDirectoryName(typeof(DdlValidationSpec).Assembly.Location);
            var repoRoot = Path.GetFullPath(Path.Combine(assemblyPath!, "..", "..", "..", "..", ".."));
            var ddlPath = Path.Combine(repoRoot, "docs", "ddl", provider, filename);

            _output.WriteLine($"Looking for DDL at: {ddlPath}");

            if (!File.Exists(ddlPath))
            {
                throw new FileNotFoundException($"DDL file not found: {ddlPath}");
            }

            return ddlPath;
        }

        private async Task ExecuteDdlFile(DataConnection connection, string ddlPath)
        {
            var sql = await File.ReadAllTextAsync(ddlPath);
            _output.WriteLine($"Executing DDL from: {Path.GetFileName(ddlPath)}");
            _output.WriteLine($"SQL length: {sql.Length} characters");

            // Execute the DDL
            await connection.ExecuteAsync(sql);

            _output.WriteLine($"✓ Successfully executed: {Path.GetFileName(ddlPath)}");
        }

        [Fact]
        public async Task SqlServer_DDL_Should_Execute_Successfully()
        {
            // Arrange
            await using var container = new SqlServerContainer();
            await container.InitializeAsync();

            _output.WriteLine($"SQL Server container started: {container.ConnectionString}");

            await using var connection = new DataConnection(
                container.ProviderName,
                container.ConnectionString);

            // Act & Assert - Execute each DDL file
            await ExecuteDdlFile(connection, GetDdlPath("sqlserver", "journal.sql"));
            await ExecuteDdlFile(connection, GetDdlPath("sqlserver", "journal-tags.sql"));
            await ExecuteDdlFile(connection, GetDdlPath("sqlserver", "snapshot.sql"));
            await ExecuteDdlFile(connection, GetDdlPath("sqlserver", "metadata.sql"));

            _output.WriteLine("✓ All SQL Server DDL files executed successfully");
        }

        [Fact]
        public async Task PostgreSQL_DDL_Should_Execute_Successfully()
        {
            // Arrange
            await using var container = new PostgreSqlContainer();
            await container.InitializeAsync();

            _output.WriteLine($"PostgreSQL container started: {container.ConnectionString}");

            await using var connection = new DataConnection(
                container.ProviderName,
                container.ConnectionString);

            // Act & Assert - Execute each DDL file
            await ExecuteDdlFile(connection, GetDdlPath("postgresql", "journal.sql"));
            await ExecuteDdlFile(connection, GetDdlPath("postgresql", "journal-tags.sql"));
            await ExecuteDdlFile(connection, GetDdlPath("postgresql", "snapshot.sql"));
            await ExecuteDdlFile(connection, GetDdlPath("postgresql", "metadata.sql"));

            _output.WriteLine("✓ All PostgreSQL DDL files executed successfully");
        }

        [Fact]
        public async Task SqlServer_DDL_Should_Be_Idempotent()
        {
            // Arrange
            await using var container = new SqlServerContainer();
            await container.InitializeAsync();

            await using var connection = new DataConnection(
                container.ProviderName,
                container.ConnectionString);

            // Act - Execute DDL twice to verify idempotency
            var journalPath = GetDdlPath("sqlserver", "journal.sql");

            await ExecuteDdlFile(connection, journalPath);
            _output.WriteLine("First execution completed");

            // Should not throw on second execution
            await ExecuteDdlFile(connection, journalPath);
            _output.WriteLine("Second execution completed - DDL is idempotent");

            // Assert
            _output.WriteLine("✓ SQL Server DDL is idempotent");
        }

        [Fact]
        public async Task PostgreSQL_DDL_Should_Be_Idempotent()
        {
            // Arrange
            await using var container = new PostgreSqlContainer();
            await container.InitializeAsync();

            await using var connection = new DataConnection(
                container.ProviderName,
                container.ConnectionString);

            // Act - Execute DDL twice to verify idempotency
            var journalPath = GetDdlPath("postgresql", "journal.sql");

            await ExecuteDdlFile(connection, journalPath);
            _output.WriteLine("First execution completed");

            // Should not throw on second execution
            await ExecuteDdlFile(connection, journalPath);
            _output.WriteLine("Second execution completed - DDL is idempotent");

            // Assert
            _output.WriteLine("✓ PostgreSQL DDL is idempotent");
        }
    }
}
