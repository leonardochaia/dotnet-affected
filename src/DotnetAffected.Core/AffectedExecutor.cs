using DotnetAffected.Abstractions;
using DotnetAffected.Core.Processor;
using Microsoft.Build.Graph;

namespace DotnetAffected.Core
{
    /// <summary>
    /// Analyzes MSBuild projects in order to determine which projects are affected by a set of changes.
    /// </summary>
    public class AffectedExecutor : IAffectedExecutor
    {
        private AffectedProcessorContext _context;

        /// <summary>
        /// Creates an executor for a repository path and a graph.
        /// </summary>
        /// <param name="repositoryPath"></param>
        /// <param name="graph"></param>
        public AffectedExecutor(string repositoryPath, ProjectGraph? graph = null)
            : this(new AffectedOptions(repositoryPath), graph)
        {
        }

        /// <summary>
        /// Creates the executor using all parameters.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="changesProvider"></param>
        /// <param name="graph"></param>
        /// <param name="changedProjectsProvider"></param>
        public AffectedExecutor(
            AffectedOptions options,
            ProjectGraph? graph = null,
            IChangesProvider? changesProvider = null,
            IChangedProjectsProvider? changedProjectsProvider = null)
        {
            _context = new AffectedProcessorContext(options, graph, changesProvider, changedProjectsProvider);
        }
        
        /// <inheritdoc />
        public AffectedSummary Execute() => GitChangesProvider.MsBuildFileSystemSupported
            ? new AffectedProcessor().Process(_context)
            : new AffectedProcessorLegacy().Process(_context);
    }
}
