using Affected.Cli.Commands;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Affected.Cli
{
    internal class DirectoryProjectDiscoverer : IProjectDiscoverer
    {
        public ProjectDiscoveryResult DiscoverProjects(CommandExecutionData data)
        {
            // TODO: Find *.*proj ?
            var projects = Directory.GetFiles(data.RepositoryPath, "*.csproj", SearchOption.AllDirectories)
                .ToArray();
            
            // TODO Log a not supported message if we find multiple Directory.Package.props files?
            var directoryPackagesPropsFilePath = Directory.GetFiles(data.RepositoryPath, "Directory.Packages.props", SearchOption.AllDirectories)
                .SingleOrDefault();

            return new ProjectDiscoveryResult()
            {
                Projects = projects, 
                DirectoryPackagesPropsFile = directoryPackagesPropsFilePath
            };
        }
    }
}
