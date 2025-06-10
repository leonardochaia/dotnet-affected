using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace Affected.Cli.Formatters
{
    internal class TraversalProjectOutputFormatter : IOutputFormatter
    {
        public string Type => "traversal";
        public string NewFileExtension => ".proj";

        public Task<string> Format(IEnumerable<IProjectInfo> projects)
        {
            var projectRootElement = @"<Project Sdk=""Microsoft.Build.Traversal/4.1.82""></Project>";
            var stringReader = new StringReader(projectRootElement);
            var xmlReader = new XmlTextReader(stringReader);
            var root = ProjectRootElement.Create(xmlReader);
            
            // REMARKS: IgnoreMissingImports is required due to the Microsoft.Build.Traversal Sdk not being found
            // on macos/darwin. We don't really need to evaluate the project, we just need to build the RawXml.
            var project = new Project(root, null, null, ProjectCollection.GlobalProjectCollection, ProjectLoadSettings.IgnoreMissingImports);

            // Find all affected and add them as project references
            foreach (var projectInfo in projects)
            {
                var currentProjectPath = projectInfo.FilePath;

                // Ignore the current project
                if (project.Items.All(i => i.EvaluatedInclude != currentProjectPath))
                {
                    project.AddItem("ProjectReference", currentProjectPath);
                }
            }

            return Task.FromResult(project.Xml.RawXml);
        }
    }
}
