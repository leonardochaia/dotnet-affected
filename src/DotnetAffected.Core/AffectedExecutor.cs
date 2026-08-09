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
        private readonly AffectedProcessorContext _context;

        /// <summary>
        /// Creates an executor for a repository path.
        /// </summary>
        /// <param name="repositoryPath"></param>
        public AffectedExecutor(string repositoryPath)
            : this(new AffectedOptions(repositoryPath))
        {
        }

        /// <summary>
        /// Creates the executor.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="changesProvider"></param>
        /// <param name="changedProjectsProvider"></param>
        public AffectedExecutor(
            AffectedOptions options,
            IChangesProvider? changesProvider = null,
            IChangedProjectsProvider? changedProjectsProvider = null)
            : this(options, null, changesProvider, changedProjectsProvider)
        {
        }

        private AffectedExecutor(
            AffectedOptions options,
            ProjectGraph? graph,
            IChangesProvider? changesProvider,
            IChangedProjectsProvider? changedProjectsProvider)
        {
            _context = new AffectedProcessorContext(options, graph, changesProvider, changedProjectsProvider);
        }

        /// <summary>
        /// Creates an executor that runs against a graph which has already been evaluated, instead
        /// of building one.
        /// </summary>
        /// <remarks>
        /// Note that not all features are available this way:
        /// Deleted files are not attributed to their project since restoring them requires
        /// evaluating the projects after the diff has been read, and a graph passed here was by
        /// definition evaluated before it, so the files are simply absent from it.
        /// </remarks>
        /// <param name="options"></param>
        /// <param name="graph"></param>
        /// <param name="changesProvider"></param>
        /// <param name="changedProjectsProvider"></param>
        /// <returns>An executor bound to <paramref name="graph"/>.</returns>
        internal static AffectedExecutor ForPreEvaluatedGraph(
            AffectedOptions options,
            ProjectGraph graph,
            IChangesProvider? changesProvider = null,
            IChangedProjectsProvider? changedProjectsProvider = null)
            => new AffectedExecutor(options, graph, changesProvider, changedProjectsProvider);

        /// <inheritdoc />
        public AffectedSummary Execute() => new AffectedProcessor().Process(_context);
    }
}
