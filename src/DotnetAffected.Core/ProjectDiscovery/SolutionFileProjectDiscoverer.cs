using DotnetAffected.Abstractions;
using Microsoft.Build.Construction;
using System.Collections.Generic;
using System.Linq;

namespace DotnetAffected.Core
{
    internal class SolutionFileProjectDiscoverer : IProjectDiscoverer
    {
        public IEnumerable<string> DiscoverProjects(IDiscoveryOptions options)
        {
            var solution = SolutionFile.Parse(options.SolutionPath);

            return solution.ProjectsInOrder
                .Where(x => x.ProjectType != SolutionProjectType.SolutionFolder)
                .Select(x => x.AbsolutePath)
                .ToArray();
        }
    }
}
