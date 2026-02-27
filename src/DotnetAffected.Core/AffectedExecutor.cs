using DotnetAffected.Abstractions;
using DotnetAffected.Core.Processor;

namespace DotnetAffected.Core
{
    /// <summary>
    /// Analyzes MSBuild projects in order to determine which are affected by a set of changes.
    /// </summary>
    public class AffectedExecutor : IAffectedExecutor
    {
        private readonly AffectedProcessorContext _context;

        /// <summary>
        /// Creates the executor using all parameters.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="buildResult"></param>
        /// <param name="changesProvider"></param>
        /// <param name="changedProjectsProvider"></param>
        public AffectedExecutor(
            AffectedOptions options,
            ProjectGraphBuildResult buildResult,
            IChangesProvider? changesProvider = null,
            IChangedProjectsProvider? changedProjectsProvider = null)
        {
            _context = new AffectedProcessorContext(
                options, buildResult, changesProvider, changedProjectsProvider);
        }

        /// <inheritdoc />
        public AffectedSummary Execute() => new AffectedProcessor().Process(_context);
    }
}
