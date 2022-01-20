using Affected.Cli.Commands;
using Microsoft.Build.Construction;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Affected.Cli
{
    internal class SolutionFileProjectDiscoverer : IProjectDiscoverer
    {
        public ProjectDiscoveryResult DiscoverProjects(CommandExecutionData data)
        {
            var solution = SolutionFile.Parse(data.SolutionPath);

            var projects = solution.ProjectsInOrder
                .Where(x => x.ProjectType != SolutionProjectType.SolutionFolder)
                .Select(x => x.AbsolutePath)
                .ToArray();

            var solutionDirectory = Path.GetDirectoryName(data.SolutionPath);
            var directoryPackagesPropsFile = Path.Combine(solutionDirectory!, "Directory.Packages.props");

            return new ProjectDiscoveryResult()
            {
                Projects = projects, 
                DirectoryPackagesPropsFile = File.Exists(directoryPackagesPropsFile) ? directoryPackagesPropsFile : null
            };

        }
    }
}
