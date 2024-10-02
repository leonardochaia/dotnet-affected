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
        public ProjectGraph Graph { get; }

        internal string[] ChangedFiles { get; set; } = Array.Empty<string>();
        internal ProjectGraphNode[] ChangedProjects { get; set; } = Array.Empty<ProjectGraphNode>();
        internal PackageChange[] ChangedPackages { get; set; } = Array.Empty<PackageChange>();
        internal ProjectGraphNode[] AffectedProjects { get; set; } = Array.Empty<ProjectGraphNode>();
        internal Dictionary<object, object> Data { get; } = new Dictionary<object, object>();
        
        public AffectedProcessorContext(AffectedOptions options,
            IChangesProvider changesProvider)
        {
            ChangesProvider = changesProvider;
            Options = options;
            Graph = new ProjectGraphFactory(Options)
                .BuildProjectGraph(changesProvider.CreateMsBuildFileSystem());
            ChangedProjectsProvider = new PredictionChangedProjectsProvider(Graph, Options);

            RepositoryPath = Path.TrimEndingDirectorySeparator(options.RepositoryPath);
            FromRef = options.FromRef;
            ToRef = options.ToRef;
        }
    }
}
