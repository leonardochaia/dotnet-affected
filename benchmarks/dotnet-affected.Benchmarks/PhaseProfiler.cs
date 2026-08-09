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

        /// <summary>
        /// Times every predictor on its own over the same graph.
        ///
        /// The collector discards outputs, so any predictor that only produces them is pure
        /// cost. This says which ones are worth dropping before anything is changed.
        /// </summary>
        public static async Task RunPredictorBreakdownAsync(int totalProjects, int childrenPerProject)
        {
            Console.WriteLine();
            Console.WriteLine($"=== predictor breakdown: {totalProjects} projects, " +
                              $"{childrenPerProject} children each ===");

            using var repository = new TemporaryRepository();

            var rootNodes = repository.CreateCsProjTree(totalProjects, childrenPerProject).ToList();
            repository.StageAndCommit();

            var seedGraph = new ProjectGraph(rootNodes.Select(x => x.FullPath));
            await repository.MakeChangesInProjectTree(seedGraph);

            var options = new AffectedOptions(repository.Path);
            var graph = Measure("build project graph",
                () => new ProjectGraphFactory(options).BuildProjectGraph());

            Console.WriteLine($"graph nodes : {graph.ProjectNodes.Count()}");
            Console.WriteLine();

            // Cost of walking the graph with nothing to run, so each predictor below can be
            // read as its own cost rather than cost plus harness.
            var baseline = TimePredictors("(no predictors: harness only)",
                graph, Array.Empty<IProjectPredictor>(), Array.Empty<IProjectGraphPredictor>());

            Console.WriteLine();
            Console.WriteLine($"{"predictor",-52} {"time",8} {"inputs",14}");

            foreach (var predictor in ProjectPredictors.AllProjectPredictors.OrderBy(p => p.GetType().Name))
            {
                TimePredictors(predictor.GetType().Name, graph,
                    new[] { predictor }, Array.Empty<IProjectGraphPredictor>(), baseline);
            }

            foreach (var predictor in ProjectPredictors.AllProjectGraphPredictors.OrderBy(p => p.GetType().Name))
            {
                TimePredictors($"[graph] {predictor.GetType().Name}", graph,
                    Array.Empty<IProjectPredictor>(), new[] { predictor }, baseline);
            }
        }

        private static TimeSpan TimePredictors(
            string label,
            ProjectGraph graph,
            IProjectPredictor[] projectPredictors,
            IProjectGraphPredictor[] graphPredictors,
            TimeSpan baseline = default)
        {
            GC.Collect(2, GCCollectionMode.Forced, blocking: true);
            GC.WaitForPendingFinalizers();

            var sink = new CountingCollector();
            var executor = new ProjectGraphPredictionExecutor(graphPredictors, projectPredictors);

            var watch = Stopwatch.StartNew();
            executor.PredictInputsAndOutputs(graph, sink);
            watch.Stop();

            var net = watch.Elapsed - baseline;
            if (net < TimeSpan.Zero)
                net = TimeSpan.Zero;

            Console.WriteLine($"{label,-52} {net.TotalSeconds,7:F2}s {sink.Count,14:N0}");

            return watch.Elapsed;
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

            // Every predictor MSBuild.Prediction ships, discarding the results. This is not the
            // set the tool actually uses, it is here to show what prediction costs in total and
            // what dropping a predictor is worth. Compare against the attribute lines below,
            // which run the real configuration.
            Measure("prediction, all predictors", () =>
            {
                var executor = new ProjectGraphPredictionExecutor(
                    ProjectPredictors.AllProjectGraphPredictors,
                    ProjectPredictors.AllProjectPredictors);
                var sink = new CountingCollector();
                executor.PredictInputsAndOutputs(graph, sink);
                return sink.Count;
            });

            // Same call with one file and with all of them. Both pay for prediction, so the
            // difference is what attributing the extra files actually costs.
            Measure("attribute 1 file",
                () => new PredictionChangedProjectsProvider(graph, options)
                    .GetReferencingProjects(changedFiles.Take(1))
                    .ToArray());

            var changedProjects = Measure($"attribute {changedFiles.Length} files",
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
