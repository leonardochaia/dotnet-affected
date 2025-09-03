using DotnetAffected.Core;
using Microsoft.Build.Graph;
using System.Collections.Generic;
using System.Linq;

namespace Affected.Cli
{
    internal class ProjectInfo : IProjectInfo
    {
        public ProjectInfo(string name, string filePath)
        {
            this.Name = name;
            this.FilePath = filePath;
            this.AdditionalProperties = Enumerable.Empty<KeyValuePair<string, string>>().ToDictionary();
        }

        public ProjectInfo(ProjectGraphNode node)
        {
            this.Name = node.GetProjectName();
            this.FilePath = node.ProjectInstance.FullPath;
            this.AdditionalProperties = Enumerable.Empty<KeyValuePair<string, string>>().ToDictionary();
        }

        public ProjectInfo(ProjectGraphNode node, IEnumerable<string> additionalProperties)
        {
            this.Name = node.GetProjectName();
            this.FilePath = node.ProjectInstance.FullPath;
            this.AdditionalProperties = node.ProjectInstance.Properties
                .Where(prop => additionalProperties.Contains(prop.Name))
                .Select(prop => new KeyValuePair<string, string>(prop.Name, prop.EvaluatedValue))
                .ToDictionary();
        }

        /// <summary>
        /// Gets the calculated name of the project.
        /// This does not include the project's extension.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the full path to the project's file.
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Gets the additional properties and values from the project's file if they exist.
        /// </summary>
        public IDictionary<string, string> AdditionalProperties { get; }
    }
}
