using Microsoft.Build.Construction;
using Microsoft.Build.Graph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace DotnetAffected.Testing.Utils
{
    public static class TemporaryRepositoryExtensions
    {
        public static ProjectRootElement CreateCsProject(
            this TemporaryRepository repo,
            string projectName,
            Action<ProjectRootElement> customizer = null)
        {
            return CreateMsBuildProject(repo, projectName, ".csproj", customizer);
        }

        public static ProjectRootElement CreateFsProject(
            this TemporaryRepository repo,
            string projectName,
            Action<ProjectRootElement> customizer = null)
        {
            return CreateMsBuildProject(repo, projectName, ".fsproj", customizer);
        }

        public static ProjectRootElement CreateVbProject(
            this TemporaryRepository repo,
            string projectName,
            Action<ProjectRootElement> customizer = null)
        {
            return CreateMsBuildProject(repo, projectName, ".vbproj", customizer);
        }

        public static ProjectRootElement CreateMsBuildProject(
            this TemporaryRepository repo,
            string projectName,
            string fileExtension,
            Action<ProjectRootElement> customizer = null)
        {
            var path = Path.Combine(repo.Path, projectName, $"{projectName}{fileExtension}");
            var project = ProjectRootElement
                .Create(path)
                .SetName(projectName);

            // REMARKS: Required for test cases using
            // Directory.Build.Props / Directory.Packages.props
            project.Sdk = "Microsoft.NET.Sdk";
            customizer?.Invoke(project);

            project.Save();

            return project;
        }

        public static ProjectRootElement CreateNonSdkMsBuildProject(
            this TemporaryRepository repo,
            string projectName,
            string fileExtension,
            Action<ProjectRootElement> customizer = null)
        {
            var projFile = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project ToolsVersion=""14.0"" DefaultTargets=""Build"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <Import Project=""$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props"" Condition=""Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')"" />
  <ItemGroup>
    <Compile Include=""file.cs"" />
  </ItemGroup>
  <Import Project=""$(MSBuildToolsPath)\Microsoft.CSharp.targets"" />
</Project>
";

            var path = Path.Combine(repo.Path, projectName, $"{projectName}{fileExtension}");

            var stringReader = new StringReader(projFile);
            var xmlReader = new XmlTextReader(stringReader);

            var project = ProjectRootElement
                .Create(xmlReader)
                .SetName(projectName);

            customizer?.Invoke(project);

            project.Save(path);

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
            var project = ProjectRootElement.Open(path) ??
                          throw new InvalidOperationException("Failed to load msbuild project");

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

        public static async Task CreateTextFileAsync(
            this TemporaryRepository repo,
            ProjectRootElement project,
            string path,
            string contents)
        {
            path = Path.Combine(repo.Path, project.GetName(), path);
            var file = File.CreateText(path);
            await file.DisposeAsync();
            await File.WriteAllTextAsync(path, contents);
        }

        /// <summary>
        /// Creates a tree of csproj with a total of <paramref name="totalProjects" />
        /// Each project will try to have <paramref name="childrenPerProject" /> without
        /// surpassing the <paramref name="totalProjects" /> count.
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="totalProjects"></param>
        /// <param name="childrenPerProject"></param>
        /// <returns></returns>
        public static IEnumerable<ProjectRootElement> CreateCsProjTree(
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
                var node = repository.CreateCsProject(name,
                    project =>
                    {
                        // REMARKS: This is what makes building the ProjectGraph EXTREMELY slow
                        // We expect target projects to be SDK based, so this makes our tests/benchmkars more accurate.
                        project.Sdk = "Microsoft.NET.Sdk";
                    });

                parent?.AddProjectDependency(node.FullPath);
                currentChildCount++;

                if (currentChildCount >= childrenPerProject)
                {
                    parent?.Save();
                    parent = node;
                }

                currentProjects++;
                yield return node;
            } while (currentProjects < totalProjects);
        }

        /// <summary>
        /// Adds a .cs file every <paramref name="everyProjectCount" />
        /// Useful for making changes to a tree of csproj created with <see cref="CreateCsProjTree" />.
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="graph"></param>
        /// <param name="everyProjectCount"></param>
        public static async Task MakeChangesInProjectTree(
            this TemporaryRepository repository,
            ProjectGraph graph,
            int everyProjectCount = 5)
        {
            var current = 0;
            foreach (var node in graph.ProjectNodes)
            {
                if (current % everyProjectCount != 0)
                {
                    continue;
                }

                var filePath = Path.Combine(node.ProjectInstance.Directory, $"file-{current}.cs");
                var fileContents = $"// contents {current}";
                await repository.CreateTextFileAsync(filePath, fileContents);

                current++;
            }
        }
    }
}
