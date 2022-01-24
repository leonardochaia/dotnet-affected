using System.Collections.Generic;
using System.Linq;

namespace Affected.Cli
{
    internal class ProjectDiscoveryResult
    {
        public ProjectDiscoveryResult()
        {
            Projects = Enumerable.Empty<string>();
        }
        
        public string? DirectoryPackagesPropsFile { get; set; }
        public IEnumerable<string> Projects { get; set; }
    }
}
