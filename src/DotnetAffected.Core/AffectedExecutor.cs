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
        /// Creates the <see cref="AffectedExecutor"/>.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="changesProvider"></param>
        public AffectedExecutor(
            AffectedOptions options,
            IChangesProvider? changesProvider = null)
        {
            _context = new AffectedProcessorContext(options, changesProvider ?? new GitChangesProvider(options));
        }

        /// <inheritdoc />
        public AffectedSummary Execute() => new AffectedProcessor().Process(_context);
    }
}
