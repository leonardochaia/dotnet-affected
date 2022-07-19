using BenchmarkDotNet.Attributes;
using System.CommandLine.Parsing;

namespace Affected.Cli.Benchmarks
{
    public class InvocationBenchmarks : BaseBenchmark
    {
        [Params(10, 50, 100)] public int ProjectGraphDepth { get; set; }
        [Params(10, 50, 100, 1000)] public int MaxNodeChildrenAmount { get; set; }

        [GlobalSetup]
        protected void GlobalSetup()
        {
            // Create a random tree of csproj
            var rootNode = this.Repository.CreateCsProjTree(ProjectGraphDepth, MaxNodeChildrenAmount);

            // Commit so there are no changes
            this.Repository.StageAndCommit();

            // Add random files to the tree so that some projects have changes
            this.Repository.RandomizeChangesInProjectTree(rootNode);
        }

        [Benchmark]
        public int Benchmark()
        {
            return this.BuildAffectedCli()
                .Build()
                .Invoke($"-p {Repository.Path} --dry-run", Terminal);
        }
    }
}
