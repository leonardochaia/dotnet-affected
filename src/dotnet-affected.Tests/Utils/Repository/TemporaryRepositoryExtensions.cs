using Microsoft.Build.Construction;
using Microsoft.Build.Graph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Affected.Cli.Tests
{
    public static class TemporaryRepositoryExtensions
    {
        public static ProjectRootElement CreateCsProject(
            this TemporaryRepository repo,
            string projectName,
            Action<ProjectRootElement> customizer = null)
        {
            var path = Path.Combine(repo.Path, projectName, $"{projectName}.csproj");
            var project = ProjectRootElement
                .Create(path)
                .SetName(projectName);

            customizer?.Invoke(project);

            project.Save();

            return project;
        }
        
        public static ProjectRootElement CreateDirectoryPackageProps(
            this TemporaryRepository repo,
            Action<ProjectRootElement> customizer)
        {
            var path = Path.Combine(repo.Path, "Directory.Packages.props");
            var project = ProjectRootElement
                .Create(path);

            customizer?.Invoke(project);

            project.Save();

            return project;
        }
        
        public static void RemoveDirectoryPackageProps(
            this TemporaryRepository repo)
        {
            repo.DeleteFile("Directory.Packages.props");
        }

        public static void DeleteFile(
            this TemporaryRepository repo,
            string relativePath)
        {
            var path = Path.Combine(repo.Path, relativePath);
            File.Delete(path);
        }
        
        public static ProjectRootElement UpdateDirectoryPackageProps(
            this TemporaryRepository repo,
            Action<ProjectRootElement> customizer)
        {
            var path = Path.Combine(repo.Path, "Directory.Packages.props");
            var project = ProjectRootElement.Open(path) ?? throw new InvalidOperationException("Failed to load msbuild project");

            customizer?.Invoke(project);

            project.Save();

            return project;
        }

        public static async Task CreateSolutionAsync(
            this TemporaryRepository repo,
            string solutionName,
            params string[] projectPaths)
        {
            var i = 0;
            var solutionContents = new SolutionFileBuilder
            {
                Projects = projectPaths.ToDictionary(p => i++.ToString())
            }.BuildSolution();

            var solutionPath = Path.Combine(repo.Path, solutionName);

            await File.WriteAllTextAsync(solutionPath, solutionContents);
        }

        public static async Task CreateTextFileAsync(
            this TemporaryRepository repo,
            string path,
            string contents)
        {
            path = Path.Combine(repo.Path, path);
            var file = File.CreateText(path);
            await file.DisposeAsync();
            await File.WriteAllTextAsync(path, contents);
        }
        
        public static IEnumerable<ProjectRootElement> CreateTree(
            this TemporaryRepository repository,
            int totalProjects,
            int childrenPerProject)
        {
            ProjectRootElement parent = null;

            var currentProjects = 0;
            var currentChildCount = 0;
            do
            {
                var name = $"project-{currentProjects}";
                var node = repository.CreateCsProject(name);

                parent?.AddProjectDependency(node.FullPath);
                currentChildCount++;

                if (currentChildCount >= childrenPerProject)
                {
                    parent = node;
                }

                currentProjects++;
                yield return node;
            } while (currentProjects < totalProjects);
        }

        public static void RandomizeChangesInProjectTree(
            this TemporaryRepository repository,
            ProjectGraph graph)
        {
            var current = 0;
            foreach (var node in graph.ProjectNodes)
            {
                if (current % 5 != 0) continue;

                var filePath = Path.Combine(node.ProjectInstance.Directory, $"file-{current}.cs");
                var fileContents = $"// contents {current}";
                Task.Run(() => repository.CreateTextFileAsync(filePath, fileContents));

                current++;
            }
        }
    }
}
