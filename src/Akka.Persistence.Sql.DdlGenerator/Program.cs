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
        }

        public static async Task<int> Main(params string[] args)
        {
            var root = new RootCommand(
                "Generates DDL scripts for Akka.Persistence.Sql database schemas");

            root.AddOption(OutputPath);
            root.AddOption(AllProviders);
            root.AddOption(Provider);

            root.SetHandler(
                async (outputPath, all, provider) =>
                {
                    Console.WriteLine("Akka.Persistence.Sql DDL Generator");
                    Console.WriteLine("===================================");
                    Console.WriteLine();

                    var generator = new DdlGenerator(outputPath);

                    if (!string.IsNullOrEmpty(provider))
                    {
                        await generator.GenerateForProvider(provider);
                    }
                    else if (all)
                    {
                        await generator.GenerateAll();
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
                Provider);

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
