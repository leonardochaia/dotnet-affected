using Affected.Cli.Commands;
using Affected.Cli.Tests;
using BenchmarkDotNet.Attributes;
using Microsoft.Build.Graph;
using Microsoft.Build.Locator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.CommandLine;
using System.CommandLine.IO;
using System.CommandLine.Rendering;
using System.Linq;

namespace Affected.Cli.Benchmarks
{
    /// <summary>
    ///     Benchmarks the affected detection algorithm AFTER
    ///     project discovery and building the MSBuild graph.
    /// </summary>
    [MemoryDiagnoser]
    public class AffectedBenchmarks
    {
        static AffectedBenchmarks()
        {
            MSBuildLocator.RegisterDefaults();
        }

        [Params(500, 1000)] public int TotalProjects { get; set; }

        [Params(20)] public int ChildrenPerProject { get; set; }

        private TemporaryRepository Repository { get; } = new();

        private ProjectGraph Graph { get; set; }

        private ICommandExecutionContext Context { get; set; }

        private ServiceProvider ServiceProvider { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            Console.WriteLine("Seeding project graph");
            // Create a random tree of csproj
            var rootNodes = Repository
                .CreateTree(TotalProjects, ChildrenPerProject)
                .ToList();

            // Commit so there are no changes
            Repository.StageAndCommit();

            // Add random files to the tree so that some projects have changes
            var graph = new ProjectGraph(rootNodes.Select(x => x.FullPath));
            Repository.RandomizeChangesInProjectTree(graph);

            Console.WriteLine($"Seeded graph with total of {graph.ProjectNodes.Count()} " +
                              $"projects in {graph.ConstructionMetrics.ConstructionTime}");

            ServiceProvider = AffectedCli
                .CreateAffectedCommandLineBuilder()
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IConsole, SystemConsole>();
                    services.AddSingleton<ITerminal, SystemConsoleTerminal>();
                    services.Replace(ServiceDescriptor.Singleton(new CommandExecutionData(
                        Repository.Path,
                        string.Empty,
                        String.Empty,
                        String.Empty,
                        true,
                        Enumerable.Empty<string>(),
                        Array.Empty<string>(),
                        true,
                        string.Empty,
                        string.Empty)));
                })
                .ComposeServiceCollection()
                .BuildServiceProvider();

            Context = ServiceProvider.GetRequiredService<ICommandExecutionContext>();

            // REMARKS: Ensure the graph is composed at setup time
            Graph = ServiceProvider.GetRequiredService<IProjectGraphRef>()
                .Value;
        }

        [GlobalCleanup]
        public void GlobalCleanUp()
        {
            Repository.Dispose();
        }

        [Benchmark]
        public int AffectedAlgorithm()
        {
            return Context.AffectedProjects.ToList()
                .Count;
        }
    }
}
