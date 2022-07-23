using Affected.Cli.Tests;
using BenchmarkDotNet.Attributes;
using Microsoft.Build.Graph;
using Microsoft.Build.Locator;
using System;
using System.CommandLine.Parsing;
using System.Linq;
using System.Threading.Tasks;

namespace Affected.Cli.Benchmarks
{
    /// <summary>
    ///     Benchmarks the equivalent of running `dotnet affected`
    ///     Complete dotnet-affected benchmark with I/O.
    /// </summary>
    [MemoryDiagnoser]
    [WarmupCount(1)] // Should be enough to populate caches
    public class MacroBenchmarks
    {
        static MacroBenchmarks()
        {
            MSBuildLocator.RegisterDefaults();
        }

        [Params(500, 1000)] public int TotalProjects { get; set; }

        [Params(20)] public int ChildrenPerProject { get; set; }

        private TemporaryRepository Repository { get; } = new();

        [GlobalSetup]
        public async Task GlobalSetup()
        {
            Console.WriteLine("Seeding project graph");

            // Create a random tree of csproj
            var rootNodes = Repository
                .CreateCsProjTree(TotalProjects, ChildrenPerProject)
                .ToList();

            // Commit so there are no changes
            Repository.StageAndCommit();

            // Add random files to the tree so that some projects have changes
            var graph = new ProjectGraph(rootNodes.Select(x => x.FullPath));
            await Repository.MakeChangesInProjectTree(graph);

            Console.WriteLine($"Built graph with total of {graph.ProjectNodes.Count()} " +
                              $"projects in {graph.ConstructionMetrics.ConstructionTime}");
        }

        [GlobalCleanup]
        public void GlobalCleanUp()
        {
            Repository.Dispose();
        }

        [Benchmark]
        public int MacroBenchmark()
        {
            var exitCode = AffectedCli
                .CreateAffectedCommandLineBuilder()
                .Build()
                .Invoke($"-p {Repository.Path}");

            if (exitCode == 0)
            {
                return exitCode;
            }

            throw new InvalidOperationException($"Exit code {exitCode} is > 0");
        }
    }
}
