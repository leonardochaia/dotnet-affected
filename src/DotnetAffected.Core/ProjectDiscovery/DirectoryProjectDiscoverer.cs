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
            var allowedExtensions = new[]
            {
                ".csproj", ".fsproj", ".vbproj"
            };
            return Directory
                .GetFiles(options.RepositoryPath, "*", SearchOption.AllDirectories)
                .Where(file => allowedExtensions.Any(file.ToLower()
                    .EndsWith))
                .ToArray();
        }
    }
}
