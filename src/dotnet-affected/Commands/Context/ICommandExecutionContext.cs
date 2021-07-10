using Microsoft.Build.Graph;
using System.Collections.Generic;

namespace Affected.Cli.Commands
{
    internal interface ICommandExecutionContext
    {
        /// <summary>
        /// Gets the list of projects that have any change in the current context.
        /// A change may be due to a change to the project file itself or files related to the project
        /// </summary>
        IEnumerable<ProjectGraphNode> ChangedProjects { get; }

        /// <summary>
        /// Gets the list of projects that are affected by <see cref="ChangedProjects"/> having change.
        /// i.e if A depends on B, and B has any changes, A will be affected.
        /// </summary>
        IEnumerable<ProjectGraphNode> AffectedProjects { get; }
    }
}
