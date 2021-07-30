using System.Collections.Generic;

namespace Affected.Cli
{
    /// <summary>
    /// Keeps compiled information about the current
    /// command execution.
    /// </summary>
    public interface ICommandExecutionContext
    {
        /// <summary>
        /// Gets the list of projects that have any change in the current context.
        /// A change may be due to a change to the project file itself or files related to the project
        /// </summary>
        IEnumerable<IProjectInfo> ChangedProjects { get; }

        /// <summary>
        /// Gets the list of projects that are affected by <see cref="ChangedProjects"/> having change.
        /// i.e if A depends on B, and B has any changes, A will be affected.
        /// </summary>
        IEnumerable<IProjectInfo> AffectedProjects { get; }
    }
}
