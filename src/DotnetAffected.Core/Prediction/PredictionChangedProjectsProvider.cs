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
        /// File systems on Windows are case insensitive, everywhere else they are not.
        /// </summary>
        private static readonly StringComparison PathComparison = GitChangesProvider.IsWindows
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        private static readonly ProjectFileAndImportsGraphPredictor[] GraphPredictors = new[]
        {
            new ProjectFileAndImportsGraphPredictor()
        };

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
                    .Where(x => x.Value.Contains(file))
                    .Select(x => x.Key)
                    .ToList();

                // A deleted file is gone from disk, so it is no longer an input of any
                // project in the current graph and no predictor can claim it. Fall back
                // to the project whose directory contained it, otherwise the owning
                // project is silently left out of the build.
                // See https://github.com/leonardochaia/dotnet-affected/issues/84
                if (nodesWithFiles.Count == 0 && !File.Exists(file))
                {
                    var containingNode = FindDeepestProjectContaining(file);
                    if (containingNode is not null)
                    {
                        nodesWithFiles.Add(containingNode);
                    }
                }

                foreach (var node in nodesWithFiles)
                {
                    if (hasReturned.Add(node.ProjectInstance.FullPath))
                    {
                        yield return node;
                    }
                }
            }
        }

        /// <summary>
        /// Finds the project whose directory contains <paramref name="file"/>.
        /// The deepest directory wins, so nested projects attribute to the innermost one.
        /// </summary>
        private ProjectGraphNode? FindDeepestProjectContaining(string file)
        {
            ProjectGraphNode? deepest = null;
            var deepestLength = -1;

            foreach (var node in _graph.ProjectNodes)
            {
                var directory = node.ProjectInstance.Directory;
                if (string.IsNullOrEmpty(directory))
                    continue;

                var prefix = directory.EndsWith(Path.DirectorySeparatorChar)
                    ? directory
                    : directory + Path.DirectorySeparatorChar;

                if (!file.StartsWith(prefix, PathComparison))
                    continue;

                if (prefix.Length > deepestLength)
                {
                    deepestLength = prefix.Length;
                    deepest = node;
                }
            }

            return deepest;
        }
    }
}
