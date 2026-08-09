using BenchmarkDotNet.Attributes;
using DotnetAffected.Abstractions;
using DotnetAffected.Core;
using DotnetAffected.Testing.Utils;
using Microsoft.Build.Graph;
using Microsoft.Build.Locator;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Affected.Cli.Benchmarks
{
    /// <summary>
    /// Benchmarks the affected detection algorithm AFTER
    /// project discovery and building the MSBuild graph.
    /// </summary>
    [MemoryDiagnoser]
    public class MicroBenchmarks
    {
        static MicroBenchmarks()
        {
            MSBuildLocator.RegisterDefaults();
        }

        [Params(500, 1000)] public int TotalProjects { get; set; }

        [Params(20)] public int ChildrenPerProject { get; set; }

        private TemporaryRepository Repository { get; } = new();

        [GlobalSetup]
        public async Task GlobalSetup()
        {
            Console.WriteLine($"Seeding project graph with {TotalProjects} projects");

            // Create a random tree of csproj
            var rootNodes = Repository
                .CreateCsProjTree(TotalProjects, ChildrenPerProject)
                .ToList();

            // Commit so there are no changes
            Repository.StageAndCommit();

            // Add random files to the tree so that some projects have changes
            var graph = new ProjectGraph(rootNodes.Select(x => x.FullPath));
            var changedFiles = await Repository.MakeChangesInProjectTree(graph);

            Console.WriteLine($"Built graph with total of {graph.ProjectNodes.Count()} " +
                              $"projects in {graph.ConstructionMetrics.ConstructionTime}, " +
                              $"{changedFiles} files changed");

            // Keep graph construction out of the measured region by running against the graph
            // built above. Deleted files are not attributed on this path, which costs this
            // benchmark nothing: the scenario only adds files.
            Executor = AffectedExecutor.ForPreEvaluatedGraph(
                new AffectedOptions(Repository.Path),
                graph);
        }

        public AffectedExecutor Executor { get; set; }

        [GlobalCleanup]
        public void GlobalCleanUp()
        {
            Repository.Dispose();
        }

        [Benchmark]
        public AffectedSummary AffectedAlgorithm()
        {
            return Executor.Execute();
        }
    }
}
