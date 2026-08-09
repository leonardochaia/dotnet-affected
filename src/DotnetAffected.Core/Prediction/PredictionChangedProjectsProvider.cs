using DotnetAffected.Abstractions;
using Microsoft.Build.Graph;
using Microsoft.Build.Prediction;
using Microsoft.Build.Prediction.Predictors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DotnetAffected.Core
{
    /// <summary>
    /// Determines which projects have changed based on the list of files that have changed.
    /// Uses MSBuild.Prediction to figure out which files are input of which projects.
    /// </summary>
    public class PredictionChangedProjectsProvider : IChangedProjectsProvider
    {
        private readonly ProjectGraph _graph;

        /// <summary>
        /// No graph predictors are used.
        ///
        /// ProjectFileAndImportsGraphPredictor used to be here. For every project it walks the
        /// whole transitive dependency closure and reports each dependency's project file and
        /// imports as an input, so a change to one project marks every project below it as
        /// changed. That is the relationship <see cref="ProjectGraphNodeExtensions.FindReferencingProjects(ProjectGraphNode)"/>
        /// already derives from the graph in a single pass, and the output is the union of the
        /// changed and the affected projects, so the same projects come out either way.
        ///
        /// The cost of deriving it twice is not small: on a 4000 node graph it emitted around
        /// 660 million inputs, which the collector then had to store and search.
        /// </summary>
        private static readonly IProjectGraphPredictor[] GraphPredictors =
            Array.Empty<IProjectGraphPredictor>();

        /// <summary>
        /// Keeps a list of all predictors that predict input files.
        /// When Microsoft.Build.Prediction is updated, this list needs to be reviewed.
        /// </summary>
        private static readonly IProjectPredictor[] ProjectPredictors = Microsoft.Build.Prediction.ProjectPredictors
            .AllProjectPredictors
            .Where(p => p.GetType() != typeof(OutDirOrOutputPathPredictor))
            .ToArray();

        private readonly ProjectGraphPredictionExecutor _executor = new ProjectGraphPredictionExecutor(
            GraphPredictors,
            ProjectPredictors);

        /// <summary>
        /// REMARKS: we have other means for detecting changes excluded files 
        /// </summary>
        private readonly string[] _fileExclusions = new[]
        {
            // Predictors won't take into account package references
            "Directory.Packages.props"
        };

        private readonly string _repositoryPath;

        /// <summary>
        /// Creates the <see cref="PredictionChangedProjectsProvider"/>.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="options"></param>
        public PredictionChangedProjectsProvider(
            ProjectGraph graph,
            IDiscoveryOptions options)
        {
            _graph = graph;
            _repositoryPath = options.RepositoryPath;
        }

        /// <inheritdoc />
        public IEnumerable<ProjectGraphNode> GetReferencingProjects(
            IEnumerable<string> files)
        {
            var hasReturned = new HashSet<string>();

            var collector = new FilesByProjectGraphCollector(this._graph, this._repositoryPath);
            _executor.PredictInputsAndOutputs(_graph, collector);

            // normalize paths so that they match on windows.
            var normalizedFiles = files
                .Where(f => !_fileExclusions.Any(f.EndsWith))
                .Select(Path.GetFullPath);

            foreach (var file in normalizedFiles)
            {
                // determine nodes depending on the changed file
                var nodesWithFiles = collector.PredictionsPerNode
                    .Where(x => x.Value.Contains(file));

                foreach (var (key, _) in nodesWithFiles)
                {
                    if (hasReturned.Add(key.ProjectInstance.FullPath))
                    {
                        yield return key;
                    }
                }
            }
        }
    }
}
