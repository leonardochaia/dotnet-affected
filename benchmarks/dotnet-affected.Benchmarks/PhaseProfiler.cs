using DotnetAffected.Core;
using DotnetAffected.Testing.Utils;
using Microsoft.Build.Graph;
using Microsoft.Build.Prediction;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Affected.Cli.Benchmarks
{
    /// <summary>
    /// Times each phase of a run separately, so a slow run can be attributed to a phase rather
    /// than guessed at. BenchmarkDotNet measures the whole algorithm as one number, which cannot
    /// tell prediction apart from attribution or from the affected traversal.
    ///
    /// Run with: dotnet run -c Release -f net10.0 -- profile [projectCounts...]
    /// </summary>
    public static class PhaseProfiler
    {
        public static async Task RunAsync(int[] projectCounts, int childrenPerProject)
        {
            foreach (var totalProjects in projectCounts)
            {
                await ProfileAsync(totalProjects, childrenPerProject);
            }
        }

        private static async Task ProfileAsync(int totalProjects, int childrenPerProject)
        {
            Console.WriteLine();
            Console.WriteLine($"=== {totalProjects} projects, {childrenPerProject} children each ===");

            using var repository = new TemporaryRepository();

            var rootNodes = repository.CreateCsProjTree(totalProjects, childrenPerProject).ToList();
            repository.StageAndCommit();

            var seedGraph = new ProjectGraph(rootNodes.Select(x => x.FullPath));
            var changedFileCount = await repository.MakeChangesInProjectTree(seedGraph);

            Console.WriteLine($"graph nodes : {seedGraph.ProjectNodes.Count()} " +
                              $"(from {totalProjects} project files)");
            Console.WriteLine($"changed     : {changedFileCount} files");
            Console.WriteLine();

            var options = new AffectedOptions(repository.Path);

            var graph = Measure("build project graph",
                () => new ProjectGraphFactory(options).BuildProjectGraph());

            var changedFiles = Measure("git diff (changed files)",
                () => new GitChangesProvider()
                    .GetChangedFiles(repository.Path, string.Empty, string.Empty)
                    .ToArray());

            // Prediction alone, discarding everything it produces, to separate the cost of
            // running the predictors from the cost of storing and searching their output.
            Measure("msbuild prediction only", () =>
            {
                var executor = new ProjectGraphPredictionExecutor(
                    ProjectPredictors.AllProjectGraphPredictors,
                    ProjectPredictors.AllProjectPredictors);
                var sink = new CountingCollector();
                executor.PredictInputsAndOutputs(graph, sink);
                return sink.Count;
            });

            var changedProjects = Measure("attribute files to projects",
                () => new PredictionChangedProjectsProvider(graph, options)
                    .GetReferencingProjects(changedFiles)
                    .ToArray());

            Measure("find affected (traversal)",
                () => changedProjects.FindReferencingProjects().ToArray());
        }

        private static T Measure<T>(string label, Func<T> action)
        {
            GC.Collect(2, GCCollectionMode.Forced, blocking: true);
            GC.WaitForPendingFinalizers();

            var before = GC.GetTotalAllocatedBytes(precise: true);
            var watch = Stopwatch.StartNew();

            var result = action();

            watch.Stop();
            var allocated = GC.GetTotalAllocatedBytes(precise: true) - before;

            var count = result is System.Collections.ICollection collection
                ? collection.Count.ToString()
                : result?.ToString() ?? "";

            Console.WriteLine($"{label,-28} {watch.Elapsed.TotalSeconds,8:F2}s " +
                              $"{allocated / 1024d / 1024d,9:F0} MB   {count}");

            return result;
        }

        /// <summary>Counts predicted inputs without keeping any of them.</summary>
        private sealed class CountingCollector : IProjectPredictionCollector
        {
            private int _count;

            public int Count => _count;

            public void AddInputFile(string path, Microsoft.Build.Execution.ProjectInstance projectInstance,
                string predictorName) => _count++;

            public void AddInputDirectory(string path, Microsoft.Build.Execution.ProjectInstance projectInstance,
                string predictorName)
            {
            }

            public void AddOutputFile(string path, Microsoft.Build.Execution.ProjectInstance projectInstance,
                string predictorName)
            {
            }

            public void AddOutputDirectory(string path, Microsoft.Build.Execution.ProjectInstance projectInstance,
                string predictorName)
            {
            }
        }
    }
}
