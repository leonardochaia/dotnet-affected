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
            string name,
            Action<ProjectRootElement> customizer = null)
        {
            var directory = repo.Directory.MakePathForCsProj(name);
            var project = ProjectRootElement
                .Create(directory)
                .SetName(name);

            customizer?.Invoke(project);

            project.Save();

            return project;
        }

        public static async Task<string> CreateSolutionAsync(
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

            return solutionPath;
        }

        public static async Task CreateTextFileAsync(
            this TemporaryRepository repo,
            string path,
            string contents)
        {
            path = Path.Combine(repo.Path, path);
            await using var _ = File.CreateText(path);
            await File.WriteAllTextAsync(path, contents);
        }
    }
}
