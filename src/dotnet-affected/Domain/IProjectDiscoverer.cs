using Affected.Cli.Commands;
using System.Collections.Generic;

namespace Affected.Cli
{
    /// <summary>
    /// Abstraction for discovering projects.
    /// </summary>
    internal interface IProjectDiscoverer
    {
        /// <summary>
        /// Discovers the projects that will be involved in changed detection.
        /// </summary>
        /// <returns>The list of project files to consider.</returns>
        IEnumerable<string> DiscoverProjects(CommandExecutionData data);
    }
}
