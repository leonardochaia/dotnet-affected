using DotnetAffected.Abstractions;
using Microsoft.Build.Graph;
using System;
using System.Collections.Generic;
using System.IO;

namespace DotnetAffected.Core.Processor
{
    /// <summary>
    /// Context used by <see cref="AffectedProcessorBase"/>
    /// </summary>
    internal class AffectedProcessorContext
    {
        private ProjectGraph? _graph;
        private string[]? _excludedProjectPaths;

        /// <inheritdoc cref="IChangesProvider"/>
        public IChangesProvider ChangesProvider { get; }

        /// <inheritdoc cref="AffectedOptions.RepositoryPath"/>
        public string RepositoryPath { get; }

        /// <inheritdoc cref="AffectedOptions.FromRef"/>
        public string FromRef { get; }

        /// <inheritdoc cref="AffectedOptions.ToRef"/>
        public string ToRef { get; }

        /// <inheritdoc cref="AffectedOptions"/>
        public AffectedOptions Options { get; }

        /// <inheritdoc cref="IChangedProjectsProvider"/>
        public IChangedProjectsProvider? ChangedProjectsProvider { get; }

        /// <inheritdoc cref="ProjectGraph"/>
        public ProjectGraph Graph
        {
            get
            {
                if (_graph == null)
                {
                    var result = new ProjectGraphFactory(Options).BuildProjectGraph();
                    _graph = result.Graph;
                    _excludedProjectPaths = result.ExcludedProjectPaths;
                }

                return _graph;
            }
        }

        /// <summary>
        /// Gets the project paths that were excluded during graph construction.
        /// Accessing this property will trigger graph construction if not already built.
        /// </summary>
        internal string[] ExcludedProjectPaths
        {
            get
            {
                // Ensure the graph is built so excluded paths are populated
                _ = Graph;
                return _excludedProjectPaths ?? Array.Empty<string>();
            }
        }

        internal string[] ChangedFiles { get; set; } = Array.Empty<string>();
        internal ProjectGraphNode[] ChangedProjects { get; set; } = Array.Empty<ProjectGraphNode>();
        internal PackageChange[] ChangedPackages { get; set; } = Array.Empty<PackageChange>();
        internal ProjectGraphNode[] AffectedProjects { get; set; } = Array.Empty<ProjectGraphNode>();
        internal Dictionary<object, object> Data { get; } = new Dictionary<object, object>();

        /// <summary>
        ///
        /// </summary>
        /// <param name="options"></param>
        /// <param name="graph"></param>
        /// <param name="changesProvider"></param>
        /// <param name="changedProjectsProvider"></param>
        /// <param name="excludedProjectPaths"></param>
        public AffectedProcessorContext(AffectedOptions options,
            ProjectGraph? graph = null,
            IChangesProvider? changesProvider = null,
            IChangedProjectsProvider? changedProjectsProvider = null,
            string[]? excludedProjectPaths = null)
        {
            ChangesProvider = changesProvider ?? new GitChangesProvider();
            Options = options;
            _graph = graph;
            _excludedProjectPaths = excludedProjectPaths;
            ChangedProjectsProvider = changedProjectsProvider;

            RepositoryPath = Path.TrimEndingDirectorySeparator(options.RepositoryPath);
            FromRef = options.FromRef;
            ToRef = options.ToRef;
        }
    }
}
