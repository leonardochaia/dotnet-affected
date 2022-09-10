using Microsoft.Build.Construction;
using Microsoft.Build.Definition;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Evaluation.Context;
using Microsoft.Build.FileSystem;
using System;
using System.IO;
using System.Xml;

namespace DotnetAffected.Core
{
    internal class ProjectFactory : IDisposable
    {
        public readonly MSBuildFileSystemBase FileSystem;
        public ProjectCollection ProjectCollection { get; }

        public ProjectFactory(MSBuildFileSystemBase fileSystem, ProjectCollection? projectCollection = null)
        {
            FileSystem = fileSystem;
            ProjectCollection = projectCollection ?? ProjectCollection.GlobalProjectCollection;
            
        }

        private ProjectRootElement CreateProjectRootElement(string path)
        {
            // Loading using a file path will not work since creating/opening the root project is done directly, no virtual FS there.
            // We must use a reader so we control where the content comes.
            // Later, when we attach it to a Project, imports will be loaded via the git file system...
            using var reader = new XmlTextReader(FileSystem.GetFileStream(path, FileMode.Open, FileAccess.Read, FileShare.None));
            var projectRootElement = ProjectRootElement.Create(reader, ProjectCollection);

            // Creating from an XML reader does not have a file system context, i.e. it does not know the root path.
            // It will use the process root path.
            // We need to set the exact path so dynamic imports are resolved relative to the original path.
            // Not that this one must be the absolute path, not relative!
            projectRootElement.FullPath = path;
            return projectRootElement;
        }

        public Project CreateProject(string projectRootElementFilePath)
        {
            var projectRootElement = CreateProjectRootElement(projectRootElementFilePath);
            return Project
                .FromProjectRootElement(projectRootElement, new ProjectOptions
                {
                    LoadSettings = ProjectLoadSettings.Default,
                    ProjectCollection = ProjectCollection,
                    EvaluationContext = EvaluationContext.Create(EvaluationContext.SharingPolicy.Shared, FileSystem),
                });
        }

        public void Dispose()
        {
            ProjectCollection.Dispose();
        }
    }
}
