// -----------------------------------------------------------------------
//  <copyright file="Program.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2023 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using System.CommandLine;
using Akka.Persistence.Sql.DdlGenerator;

namespace Akka.Persistence.Sql.DdlGenerator
{
    public static class Program
    {
        private static readonly Option<string> OutputPath;
        private static readonly Option<bool> AllProviders;
        private static readonly Option<string?> Provider;
        private static readonly Option<string> TableMapping;

        static Program()
        {
            OutputPath = new Option<string>(
                aliases: new[] { "--output", "-o" },
                description: "Output directory for generated DDL files",
                getDefaultValue: () => Path.Combine(GetRepositoryRoot(), "docs", "ddl"))
            {
                IsRequired = false,
            };

            AllProviders = new Option<bool>(
                aliases: new[] { "--all", "-a" },
                description: "Generate DDL for all supported providers",
                getDefaultValue: () => true);

            Provider = new Option<string?>(
                aliases: new[] { "--provider", "-p" },
                description: "Generate DDL for specific provider (SqlServer, PostgreSQL, MySQL, SQLite)");

            TableMapping = new Option<string>(
                aliases: new[] { "--table-mapping", "-m" },
                description: "Table mapping mode: 'default' (new deployments), 'compat' (legacy migration), or 'all' (both)",
                getDefaultValue: () => "all");
        }

        public static async Task<int> Main(params string[] args)
        {
            var root = new RootCommand(
                "Generates DDL scripts for Akka.Persistence.Sql database schemas");

            root.AddOption(OutputPath);
            root.AddOption(AllProviders);
            root.AddOption(Provider);
            root.AddOption(TableMapping);

            root.SetHandler(
                async (outputPath, all, provider, tableMapping) =>
                {
                    Console.WriteLine("Akka.Persistence.Sql DDL Generator");
                    Console.WriteLine("===================================");
                    Console.WriteLine();

                    // Determine which table mappings to generate
                    var mappings = tableMapping.ToLowerInvariant() switch
                    {
                        "default" => new[] { TableMappingMode.Default },
                        "compat" => new[] { TableMappingMode.Compat },
                        _ => new[] { TableMappingMode.Default, TableMappingMode.Compat }
                    };

                    var generator = new DdlGenerator(outputPath);

                    if (!string.IsNullOrEmpty(provider))
                    {
                        foreach (var mapping in mappings)
                        {
                            await generator.GenerateForProvider(provider, mapping);
                        }
                    }
                    else if (all)
                    {
                        foreach (var mapping in mappings)
                        {
                            await generator.GenerateAll(mapping);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Please specify --all or --provider <name>");
                        return;
                    }

                    Console.WriteLine();
                    Console.WriteLine("DDL generation complete!");
                    Console.WriteLine($"Output location: {outputPath}");
                },
                OutputPath,
                AllProviders,
                Provider,
                TableMapping);

            return await root.InvokeAsync(args);
        }

        private static string GetRepositoryRoot()
        {
            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "Akka.Persistence.Sql.sln")))
            {
                directory = directory.Parent;
            }
            return directory?.FullName ?? Directory.GetCurrentDirectory();
        }
    }
}
