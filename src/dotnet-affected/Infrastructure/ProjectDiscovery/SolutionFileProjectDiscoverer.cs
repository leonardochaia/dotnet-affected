using Affected.Cli.Commands;
using Microsoft.Build.Construction;
using System.Collections.Generic;
using System.Linq;

namespace Affected.Cli
{
    internal class SolutionFileProjectDiscoverer : IProjectDiscoverer
    {
        public IEnumerable<string> DiscoverProjects(CommandExecutionData data)
        {
            var solution = SolutionFile.Parse(data.SolutionPath);

            return solution.ProjectsInOrder
                .Where(x => x.ProjectType != SolutionProjectType.SolutionFolder)
                .Select(x => x.AbsolutePath)
                .ToArray();
        }
    }
}
