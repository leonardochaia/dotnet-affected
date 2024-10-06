using System.Collections.Generic;
using System.Linq;

namespace Affected.Cli
{
    internal class SplitOutputStrategy : IOutputStrategy
    {
        private readonly IReadOnlyList<string> _suffixes = OutputFilters.All.Select(s => $".{s}").ToList();
        public string Name { get; }
        public string Directory { get; }
        public IEnumerable<IProjectInfo> Projects { get; }

        public SplitOutputStrategy(string name, string directory, IEnumerable<IProjectInfo> projects)
        {
            Name = name;
            Directory = directory;
            Projects = projects;
        }
        
        public IEnumerable<IOutput> GetOutputs()
        {
            return Projects
                .GroupBy(p => p.Status)
                .Select(g =>
                {
                    var suffix = _suffixes[(int)g.Key];
                    var name = $"{this.Name}{suffix}";
                    return new Output(name, Directory, g);
                });
        }
    }
}
