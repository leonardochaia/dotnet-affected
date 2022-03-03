using Affected.Cli.Commands;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Affected.Cli
{
    internal class DirectoryProjectDiscoverer : IProjectDiscoverer
    {
        public IEnumerable<string> DiscoverProjects(CommandExecutionData data)
        {
            // TODO: Find *.*proj ?
            return Directory.GetFiles(data.RepositoryPath, "*.csproj", SearchOption.AllDirectories)
                .ToArray();
        }
    }
}
