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

            // When a filtering file is provided, use a specific discoverer based on its path.
            if (options.FilterFilePath.EndsWith(".sln") || options.FilterFilePath.EndsWith(".slnx"))
            {
                return new SolutionFileProjectDiscoverer().DiscoverProjects(options);
            }
            
            // Solution filters cannot go through SolutionSerializers,
            // Microsoft.VisualStudio.SolutionPersistence does not support them.
            if (options.FilterFilePath.EndsWith(".slnf"))
            {
                return new SolutionFilterProjectDiscoverer().DiscoverProjects(options);
            }

            if (options.FilterFilePath.EndsWith(".proj"))
            {
                return new MSBuildProjectDiscoverer().DiscoverProjects(options);
            }

            throw new NotImplementedException($"Filtering by {options.FilterFilePath} is not yet implemented");
        }
    }
}
