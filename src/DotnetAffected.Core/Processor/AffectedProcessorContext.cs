using DotnetAffected.Abstractions;
using Microsoft.Build.Graph;
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
        public ProjectGraph Graph => _graph ??= new ProjectGraphFactory(Options).BuildProjectGraph();

        internal string[] ChangedFiles { get; set; }
        internal ProjectGraphNode[] ChangedProjects { get; set; }
        internal PackageChange[] ChangedPackages { get; set; }
        internal ProjectGraphNode[] AffectedProjects { get; set; }
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
