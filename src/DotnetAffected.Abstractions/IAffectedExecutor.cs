namespace Affected.Cli
{
    /// <summary>
    /// Examines the project graph to determine which projects are affected by a set of changes.
    /// </summary>
    public interface IAffectedExecutor
    {
        /// <summary>
        /// Performs the affected calculation.
        /// </summary>
        /// <returns>A Summary of the operation.</returns>
        AffectedSummary Execute();
    }
}
