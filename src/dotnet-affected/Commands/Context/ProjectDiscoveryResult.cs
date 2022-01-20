using System.Collections.Generic;

namespace Affected.Cli
{
    internal class ProjectDiscoveryResult
    {
        public string? DirectoryPackagesPropsFile { get; set; }
        public IEnumerable<string> Projects { get; set; }
    }
}
