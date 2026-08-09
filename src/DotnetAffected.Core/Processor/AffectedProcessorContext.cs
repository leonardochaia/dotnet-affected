using DotnetAffected.Abstractions;
using DotnetAffected.Core.FileSystem;
using Microsoft.Build.Graph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DotnetAffected.Core.Processor
{
    /// <summary>
    /// Context used by <see cref="AffectedProcessorBase"/>
    /// </summary>
    internal class AffectedProcessorContext
    {
        private ProjectGraph? _graph;

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
        public ProjectGraph Graph => _graph ??= BuildProjectGraph();

        /// <summary>
        /// The graph is built lazily, after the changed files are known, so deleted files can be
        /// put back before evaluation. Without that a deleted file matches no glob and satisfies
        /// no Exists() condition, and the project referencing it is never marked as changed.
        /// Projects are evaluated against the real file system whenever nothing was deleted.
        /// </summary>
        private ProjectGraph BuildProjectGraph()
        {
            var deletedFiles = ChangedFiles
                .Where(file => !File.Exists(file))
                .ToArray();

            if (deletedFiles.Length == 0)
                return new ProjectGraphFactory(Options).BuildProjectGraph();

            var contents = ChangesProvider.ReadFilesAt(RepositoryPath, FromRef, deletedFiles);
            var fileSystem = new DeletedFilesOverlayFileSystem(RepositoryPath, contents);

            return new ProjectGraphFactory(Options, fileSystem).BuildProjectGraph();
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
        public AffectedProcessorContext(AffectedOptions options,
            ProjectGraph? graph = null,
            IChangesProvider? changesProvider = null,
            IChangedProjectsProvider? changedProjectsProvider = null)
        {
            ChangesProvider = changesProvider ?? new GitChangesProvider();
            Options = options;
            _graph = graph;
            ChangedProjectsProvider = changedProjectsProvider;

            RepositoryPath = Path.TrimEndingDirectorySeparator(options.RepositoryPath);
            FromRef = options.FromRef;
            ToRef = options.ToRef;
        }
    }
}
