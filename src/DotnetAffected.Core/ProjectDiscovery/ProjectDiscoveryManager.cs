using DotnetAffected.Abstractions;
using System;
using System.Collections.Generic;

namespace DotnetAffected.Core
{
    internal class ProjectDiscoveryManager : IProjectDiscoverer
    {
        public IEnumerable<string> DiscoverProjects(IDiscoveryOptions options)
        {
            // Whe no filtering file is provided, discover from file system.
            if (options.FilterFilePath == null)
            {
                return new DirectoryProjectDiscoverer().DiscoverProjects(options);
            }

            // When a filtering file is provided, use an specific discoverer based on its path.
            if (options.FilterFilePath.EndsWith(".sln"))
            {
                return new SolutionFileProjectDiscoverer().DiscoverProjects(options);
            }

            throw new NotImplementedException($"Filtering by {options.FilterFilePath} is not yet implemented");
        }
    }
}
