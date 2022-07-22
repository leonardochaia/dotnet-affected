using Affected.Cli.Tests;
using BenchmarkDotNet.Attributes;
using Microsoft.Build.Graph;
using Microsoft.Build.Locator;
using System;
using System.CommandLine.Parsing;
using System.Linq;

namespace Affected.Cli.Benchmarks
{
    [MemoryDiagnoser]
    public class InvocationBenchmarks
    {
        [Params(500, 1000)] public int TotalProjects { get; set; }

        [Params(20)] public int ChildrenPerProject { get; set; }

        static InvocationBenchmarks()
        {
            MSBuildLocator.RegisterDefaults();
        }

        private TemporaryRepository Repository { get; } = new TemporaryRepository();

        [GlobalSetup]
        public void GlobalSetup()
        {
            // Create a random tree of csproj
            var rootNodes = this.Repository
                .CreateTree(TotalProjects, ChildrenPerProject)
                .ToList();

            // Commit so there are no changes
            this.Repository.StageAndCommit();

            // Add random files to the tree so that some projects have changes
            var graph = new ProjectGraph(rootNodes.Select(x => x.FullPath));
            this.Repository.RandomizeChangesInProjectTree(graph);

            Console.WriteLine($"Seeded graph with total of {graph.ProjectNodes.Count()} projects in {graph.ConstructionMetrics.ConstructionTime}");
        }

        [GlobalCleanup]
        public void GlobalCleanUp()
        {
            this.Repository.Dispose();
        }

        [Benchmark]
        public int Benchmark()
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
