// -----------------------------------------------------------------------
//  <copyright file="DdlValidationSpec.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2023 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;
using Akka.Persistence.Sql.Tests.Common.Containers;
using Akka.Persistence.Sql.Tests.Common.Internal.Xunit;
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
    [SkipWindows]
    public class DdlValidationSpec
    {
        private readonly ITestOutputHelper _output;

        public DdlValidationSpec(ITestOutputHelper output)
        {
            _output = output;
        }

        private string GetDdlPath(string mapping, string provider, string filename)
        {
            // Navigate from test assembly location to repository root
            var assemblyPath = Path.GetDirectoryName(typeof(DdlValidationSpec).Assembly.Location);
            var repoRoot = Path.GetFullPath(Path.Combine(assemblyPath!, "..", "..", "..", "..", ".."));
            var ddlPath = Path.Combine(repoRoot, "docs", "ddl", mapping, provider, filename);

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

            _output.WriteLine($"Successfully executed: {Path.GetFileName(ddlPath)}");
        }

        private async Task<ITestContainer> CreateContainer(string provider)
        {
            ITestContainer container = provider.ToLowerInvariant() switch
            {
                "sqlserver" => new SqlServerContainer(),
                "postgresql" => new PostgreSqlContainer(),
                "mysql" => new MySqlContainer(),
                "sqlite" => new MsSqliteContainer(),
                _ => throw new ArgumentException($"Unknown provider: {provider}")
            };

            await container.InitializeAsync();
            return container;
        }

        [Theory]
        [InlineData("compat", "sqlserver")]
        [InlineData("compat", "postgresql")]
        [InlineData("compat", "mysql")]
        [InlineData("compat", "sqlite")]
        [InlineData("default", "sqlserver")]
        [InlineData("default", "postgresql")]
        [InlineData("default", "mysql")]
        [InlineData("default", "sqlite")]
        public async Task DDL_Should_Execute_Successfully(string mapping, string provider)
        {
            // Arrange
            await using var container = await CreateContainer(provider);

            _output.WriteLine($"{provider} container started: {container.ConnectionString}");
            _output.WriteLine($"Testing {mapping} mapping");

            await using var connection = new DataConnection(
                container.ProviderName,
                container.ConnectionString);

            // Act & Assert - Execute each DDL file
            await ExecuteDdlFile(connection, GetDdlPath(mapping, provider, "journal.sql"));
            await ExecuteDdlFile(connection, GetDdlPath(mapping, provider, "journal-tags.sql"));
            await ExecuteDdlFile(connection, GetDdlPath(mapping, provider, "snapshot.sql"));
            await ExecuteDdlFile(connection, GetDdlPath(mapping, provider, "metadata.sql"));

            _output.WriteLine($"All {provider} ({mapping}) DDL files executed successfully");
        }

        [Theory]
        [InlineData("compat", "sqlserver")]
        [InlineData("compat", "postgresql")]
        [InlineData("compat", "mysql")]
        [InlineData("compat", "sqlite")]
        [InlineData("default", "sqlserver")]
        [InlineData("default", "postgresql")]
        [InlineData("default", "mysql")]
        [InlineData("default", "sqlite")]
        public async Task DDL_Should_Be_Idempotent(string mapping, string provider)
        {
            // Arrange
            await using var container = await CreateContainer(provider);

            _output.WriteLine($"{provider} container started");
            _output.WriteLine($"Testing {mapping} mapping idempotency");

            await using var connection = new DataConnection(
                container.ProviderName,
                container.ConnectionString);

            // Act - Execute DDL twice to verify idempotency
            var journalPath = GetDdlPath(mapping, provider, "journal.sql");

            await ExecuteDdlFile(connection, journalPath);
            _output.WriteLine("First execution completed");

            // Should not throw on second execution
            await ExecuteDdlFile(connection, journalPath);
            _output.WriteLine("Second execution completed - DDL is idempotent");

            // Assert
            _output.WriteLine($"{provider} ({mapping}) DDL is idempotent");
        }
    }
}
