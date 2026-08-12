using DotnetAffected.Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Affected.Cli.Formatters
{
    internal class TraversalProjectOutputFormatter : IOutputFormatter
    {
        public string Type => "traversal";
        public string NewFileExtension => ".proj";

        public Task<string> Format(IEnumerable<IProjectInfo> projects, OutputFormatterContext context)
        {
            var root = TraversalProject.Create();

            // Find all affected and add them as project references
            foreach (var projectInfo in projects)
            {
                var currentProjectPath = projectInfo.FilePath;

                // Ignore the current project
                if (root.Items.All(i => i.Include != currentProjectPath))
                {
                    root.AddItem("ProjectReference", currentProjectPath);
                }
            }

            return Task.FromResult(root.RawXml);
        }
    }
}
