using DotnetAffected.Abstractions;
using System;
using System.Collections.Generic;

namespace DotnetAffected.Core
{
    /// <summary>
    /// Discovers projects from a Solution Filter file (.slnf).
    /// </summary>
    internal class SolutionFilterProjectDiscoverer : IProjectDiscoverer
    {
        public IEnumerable<string> DiscoverProjects(IDiscoveryOptions options)
        {
            // It should not be possible for this to be null based on call paths,
            // but this makes the warning go away.
            ArgumentNullException.ThrowIfNull(options.FilterFilePath);

            return SolutionFilter.Load(options.FilterFilePath).ProjectPaths;
        }
    }
}
