using DotnetAffected.Testing.Utils;
using LibGit2Sharp;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DotnetAffected.Core.Tests
{
    /// <summary>
    /// Tests for discovering projects from the file system while honouring .gitignore.
    ///
    /// Anything git ignores is build output, tooling scratch or a nested clone, none of which is
    /// a project of this repository. Discovering them means MSBuild evaluates copies of projects
    /// that are already in the graph under their real path, which at best doubles the work and at
    /// worst fails the whole run on something that was never meant to be built.
    ///
    /// See https://github.com/leonardochaia/dotnet-affected/issues/170
    /// </summary>
    public class GitIgnoredProjectDiscoveryTests : BaseRepositoryTest
    {
        internal const string ProjectContents = @"<Project Sdk=""Microsoft.NET.Sdk"">
    <PropertyGroup>
        <TargetFrameworks>net8.0;net9.0;net10.0</TargetFrameworks>
    </PropertyGroup>
</Project>";

        private string[] DiscoverProjects(bool honourGitIgnore = true)
            => DiscoverProjects(new AffectedOptions(Repository.Path, honourGitIgnore: honourGitIgnore));

        /// <summary>
        /// The graph holds a node per target framework, so the paths are deduplicated to get back
        /// to the set of discovered project files.
        /// </summary>
        internal static string[] DiscoverProjects(AffectedOptions options)
            => new ProjectGraphFactory(options)
                .BuildProjectGraph()
                .ProjectNodes
                .Select(node => node.ProjectInstance.FullPath)
                .Distinct()
                .OrderBy(path => path)
                .ToArray();

        [Fact]
        public async Task Projects_inside_an_ignored_directory_should_not_be_discovered()
        {
            var project = Repository.CreateCsProject("Lib");

            await Repository.CreateTextFileAsync(".gitignore", "artifacts/\n");
            await Repository.CreateTextFileAsync("artifacts/Lib/Lib.csproj", ProjectContents);

            Assert.Equal(new[] { project.FullPath }, DiscoverProjects());
        }

        [Fact]
        public async Task Ignored_project_files_should_not_be_discovered()
        {
            var project = Repository.CreateCsProject("Lib");

            // A pattern matching the file itself, rather than the directory holding it.
            await Repository.CreateTextFileAsync(".gitignore", "Scratch.csproj\n");
            await Repository.CreateTextFileAsync("scratch/Scratch.csproj", ProjectContents);

            Assert.Equal(new[] { project.FullPath }, DiscoverProjects());
        }

        [Fact]
        public async Task Nested_gitignore_files_should_be_honoured()
        {
            var project = Repository.CreateCsProject("Lib");

            await Repository.CreateTextFileAsync("src/.gitignore", "generated/\n");
            await Repository.CreateTextFileAsync("src/generated/Gen/Gen.csproj", ProjectContents);

            Assert.Equal(new[] { project.FullPath }, DiscoverProjects());
        }

        /// <summary>
        /// `git add -f` beats the ignore rules: git considers a tracked file to be repository
        /// content whatever .gitignore says about it, and so must discovery. This is the escape
        /// hatch for a project that has to live inside an ignored directory.
        /// </summary>
        [Fact]
        public async Task Tracked_projects_should_be_discovered_even_when_a_pattern_matches_them()
        {
            var project = Repository.CreateCsProject("Lib");

            await Repository.CreateTextFileAsync(".gitignore", "build/\n");
            await Repository.CreateTextFileAsync("build/Tool/Tool.csproj", ProjectContents);

            Commands.Stage(Repository.Repository, "build/Tool/Tool.csproj", new StageOptions
            {
                IncludeIgnored = true
            });

            var discovered = DiscoverProjects();

            Assert.Equal(2, discovered.Length);
            Assert.Contains(project.FullPath, discovered);
            Assert.Contains(Path.Combine(Repository.Path, "build", "Tool", "Tool.csproj"), discovered);
        }

        [Fact]
        public async Task Passing_no_gitignore_should_discover_ignored_projects()
        {
            var project = Repository.CreateCsProject("Lib");

            await Repository.CreateTextFileAsync(".gitignore", "artifacts/\n");
            await Repository.CreateTextFileAsync("artifacts/Copy/Copy.csproj", ProjectContents);

            var discovered = DiscoverProjects(honourGitIgnore: false);

            Assert.Equal(2, discovered.Length);
            Assert.Contains(project.FullPath, discovered);
            Assert.Contains(Path.Combine(Repository.Path, "artifacts", "Copy", "Copy.csproj"), discovered);
        }
    }

    /// <summary>
    /// Discovery is reachable without any repository at all, through <see cref="ProjectGraphFactory"/>,
    /// and must not start requiring one.
    /// </summary>
    public class ProjectDiscoveryWithoutGitTests : BaseMSBuildTest
    {
        [Fact]
        public async Task Everything_should_be_discovered_when_the_directory_is_not_a_repository()
        {
            using var directory = new TempWorkingDirectory();

            await CreateProjectAsync(directory.Path, Path.Combine("Lib", "Lib.csproj"));
            await CreateProjectAsync(directory.Path, Path.Combine("bin", "Copy", "Copy.csproj"));

            var discovered = GitIgnoredProjectDiscoveryTests.DiscoverProjects(
                new AffectedOptions(directory.Path));

            Assert.Equal(2, discovered.Length);
        }

        private static Task CreateProjectAsync(string root, string relativePath)
        {
            var path = Path.Combine(root, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            return File.WriteAllTextAsync(path, GitIgnoredProjectDiscoveryTests.ProjectContents);
        }
    }
}
