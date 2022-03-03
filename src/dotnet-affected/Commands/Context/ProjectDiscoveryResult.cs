using System.Collections.Generic;
using System.Linq;

namespace Affected.Cli
{
    internal class ProjectDiscoveryResult
    {
        public string? DirectoryPackagesPropsFile { get; set; }

        public bool UsesCentralPackageManagement => DirectoryPackagesPropsFile != null;

        public IEnumerable<string> Projects { get; set; } = Enumerable.Empty<string>();
    }
}
