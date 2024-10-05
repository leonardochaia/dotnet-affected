using System.Collections.Generic;
using System.Linq;

namespace Affected.Cli
{
    internal class CombinedOutputStrategy : IOutputStrategy
    {
        public string Name { get; }
        public string Directory { get; }
        public IEnumerable<IProjectInfo> Projects { get; }

        public CombinedOutputStrategy(string name, string directory, IEnumerable<IProjectInfo> projects)
        {
            Name = name;
            Directory = directory;
            Projects = projects;
        }

        public IEnumerable<IOutput> GetOutputs()
        {
            return new IOutput[] { new Output(Name, Directory, Projects) };
        }
    }
}
