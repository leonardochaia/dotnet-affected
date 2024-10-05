using System.Collections.Generic;

namespace Affected.Cli
{
    internal class Output : IOutput
    {
        public string Name { get; }
        public string Directory { get; }
        public IEnumerable<IProjectInfo> Projects { get; }
        
        public Output(string name, string directory, IEnumerable<IProjectInfo> projects)
        {
            Name = name;
            Directory = directory;
            Projects = projects;
        }
    }
}
