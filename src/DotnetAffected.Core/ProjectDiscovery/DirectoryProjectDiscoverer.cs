using DotnetAffected.Abstractions;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DotnetAffected.Core
{
    internal class DirectoryProjectDiscoverer : IProjectDiscoverer
    {
        public IEnumerable<string> DiscoverProjects(IDiscoveryOptions options)
        {
            // TODO: Find *.*proj ?
            return Directory.GetFiles(options.RepositoryPath, "*.csproj", SearchOption.AllDirectories)
                .ToArray();
        }
    }
}
