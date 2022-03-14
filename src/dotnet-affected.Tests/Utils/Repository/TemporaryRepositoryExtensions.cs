using Microsoft.Build.Construction;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Affected.Cli.Tests
{
    internal static class TemporaryRepositoryExtensions
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
            var path = Path.Combine(repo.Path, "Directory.Packages.props");
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
    }
}
