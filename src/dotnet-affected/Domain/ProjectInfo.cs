using DotnetAffected.Core;
using Microsoft.Build.Graph;

namespace Affected.Cli
{
    internal class ProjectInfo : IProjectInfo
    {
        public ProjectInfo(string name, string filePath)
        {
            this.Name = name;
            this.FilePath = filePath;
        }

        public ProjectInfo(ProjectGraphNode node)
        {
            this.Name = node.GetProjectName();
            this.FilePath = node.ProjectInstance.FullPath;
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
    }
}
