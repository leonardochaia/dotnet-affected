using System.Collections.Generic;

namespace DotnetAffected.Abstractions
{
    /// <summary>
    /// Abstraction for discovering projects.
    /// </summary>
    public interface IProjectDiscoverer
    {
        /// <summary>
        /// Discovers the projects that will be involved in changed detection.
        /// </summary>
        /// <returns>The list of project files to consider.</returns>
        IEnumerable<string> DiscoverProjects(IDiscoveryOptions options);
    }
}
