using DotnetAffected.Abstractions;
using DotnetAffected.Testing.Utils;
using Microsoft.Build.Exceptions;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace DotnetAffected.Core.Tests
{
    /// <summary>
    /// Projects that MSBuild cannot evaluate at all, such as a .sqlproj importing SSDT targets
    /// that only ship with Visual Studio, take the whole run down when the graph is built.
    ///
    /// Excluding them is the documented escape hatch, so the exclusion pattern has to be honored
    /// while discovering projects, before they are ever handed to the graph. These cover that,
    /// and the edges where it meets the rest of the pipeline.
    /// </summary>
    public class ExcludeDiscoveryTests : BaseRepositoryTest
    {
        private const string SolutionPath = "test-solution.sln";

        /// <summary>
        /// Mimics the import that makes a legacy SSDT database project unevaluatable outside of
        /// Visual Studio. The import is unconditional, so evaluation throws
        /// InvalidProjectFileException however little else the project declares.
        /// </summary>
        private const string UnevaluatableProject = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project ToolsVersion=""4.0"" DefaultTargets=""Build"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <Import Project=""$(MSBuildExtensionsPath)\Microsoft\VisualStudio\v$(VisualStudioVersion)\SSDT\Microsoft.Data.Tools.Schema.SqlTasks.targets"" />
</Project>
";

        /// <summary>
        /// Writes an unevaluatable database project and returns its full path.
        /// </summary>
        private async Task<string> CreateSqlProjectAsync()
        {
            var relativePath = Path.Combine("Database", "Database.sqlproj");
            await Repository.CreateTextFileAsync(relativePath, UnevaluatableProject);

            return Path.Combine(Repository.Path, relativePath);
        }

        private AffectedSummary Execute(string excludeDiscoveryRegex, params string[] assumeChanges)
            => new AffectedExecutor(new AffectedOptions(
                    Repository.Path,
                    Path.Combine(Repository.Path, SolutionPath),
                    assumeChanges: assumeChanges.Length == 0 ? null : assumeChanges,
                    excludeDiscoveryRegex: excludeDiscoveryRegex))
                .Execute();

        [Fact]
        public async Task When_excluded_project_cannot_be_evaluated_it_should_not_fail()
        {
            var project = Repository.CreateCsProject("InventoryManagement");
            var sqlProjectPath = await CreateSqlProjectAsync();

            await Repository.CreateSolutionAsync(SolutionPath, project.FullPath, sqlProjectPath);

            var summary = Execute(@"\.sqlproj$");

            var projectInfo = Assert.Single(summary.ProjectsWithChangedFiles);
            Assert.Equal(project.FullPath, projectInfo.GetFullPath());
        }

        [Fact]
        public async Task When_excluded_project_cannot_be_evaluated_other_projects_should_still_be_affected()
        {
            var project = Repository.CreateCsProject("InventoryManagement");
            var dependant = Repository.CreateCsProject(
                "InventoryManagement.Tests",
                p => p.AddProjectDependency(project.FullPath));

            var sqlProjectPath = await CreateSqlProjectAsync();

            await Repository.CreateSolutionAsync(
                SolutionPath, project.FullPath, dependant.FullPath, sqlProjectPath);

            Repository.StageAndCommit();

            await Repository.CreateTextFileAsync(
                Path.Combine("InventoryManagement", "file.cs"), "// Initial content");

            var summary = Execute(@"\.sqlproj$");

            var changedProject = Assert.Single(summary.ProjectsWithChangedFiles);
            Assert.Equal(project.FullPath, changedProject.GetFullPath());

            var affectedProject = Assert.Single(summary.AffectedProjects);
            Assert.Equal(dependant.FullPath, affectedProject.GetFullPath());
        }

        [Fact]
        public async Task Excluded_projects_should_be_reported_by_path()
        {
            var project = Repository.CreateCsProject("InventoryManagement");
            var sqlProjectPath = await CreateSqlProjectAsync();

            await Repository.CreateSolutionAsync(SolutionPath, project.FullPath, sqlProjectPath);

            var summary = Execute(@"\.sqlproj$");

            var excluded = Assert.Single(summary.ProjectsExcludedFromDiscovery);
            Assert.Equal(sqlProjectPath, excluded);
        }

        /// <summary>
        /// KNOWN LIMITATION. Excluding a project keeps it out of the graph's entry points, but
        /// MSBuild still evaluates whatever the remaining entry points reference, so a project that
        /// something depends on is evaluated regardless and an unevaluatable one still fails.
        ///
        /// This is pinned rather than fixed: the reported case is a database project nothing
        /// references. Covering the referenced case means serving excluded projects as a stub
        /// through an MSBuildFileSystemBase overlay, which is a much larger commitment.
        /// </summary>
        [Fact]
        public async Task When_an_excluded_project_is_referenced_it_is_still_evaluated()
        {
            var sqlProjectPath = await CreateSqlProjectAsync();

            // The reference is what drags the excluded project back into evaluation.
            var project = Repository.CreateCsProject(
                "InventoryManagement",
                p => p.AddProjectDependency(sqlProjectPath));

            await Repository.CreateSolutionAsync(SolutionPath, project.FullPath, sqlProjectPath);

            var exception = Record.Exception(() => Execute(@"\.sqlproj$"));

            // Pinned to the evaluation failure specifically. Asserting only that something threw
            // would keep passing once it starts throwing for some unrelated reason, which is the
            // one way a test like this can quietly stop testing anything.
            var aggregate = Assert.IsType<AggregateException>(exception);
            var inner = Assert.IsType<InvalidProjectFileException>(
                Assert.Single(aggregate.InnerExceptions));

            Assert.Equal(sqlProjectPath, inner.ProjectFile);
        }

        /// <summary>
        /// An excluded project is not in the graph, so it cannot be named as an assumed change.
        /// Failing loudly beats reporting nothing as affected, which reads like a correct answer.
        /// </summary>
        [Fact]
        public async Task When_assuming_changes_for_an_excluded_project_should_fail()
        {
            var project = Repository.CreateCsProject("InventoryManagement");
            var sqlProjectPath = await CreateSqlProjectAsync();

            await Repository.CreateSolutionAsync(SolutionPath, project.FullPath, sqlProjectPath);

            var exception = Assert.Throws<AssumedProjectNotFoundException>(
                () => Execute(@"\.sqlproj$", "Database"));

            Assert.Equal("Database", exception.Assumption);
        }

        /// <summary>
        /// Excluding from discovery removes the project outright, so nothing downstream of it is
        /// reachable either. This differs from excluding from the output, which keeps the project in
        /// the graph and lets changes travel through it.
        /// </summary>
        [Fact]
        public async Task Excluding_a_project_from_discovery_severs_what_depends_on_it()
        {
            var shared = Repository.CreateCsProject("Shared");
            var dependant = Repository.CreateCsProject(
                "Shared.Consumer",
                p => p.AddProjectDependency(shared.FullPath));

            await Repository.CreateSolutionAsync(SolutionPath, shared.FullPath, dependant.FullPath);
            Repository.StageAndCommit();

            await Repository.CreateTextFileAsync(Path.Combine("Shared", "file.cs"), "// changed");

            // Excluding the consumer means the change to Shared affects nothing.
            var summary = Execute(@"Shared\.Consumer");

            Assert.Single(summary.ProjectsWithChangedFiles);
            Assert.Empty(summary.AffectedProjects);
            Assert.Single(summary.ProjectsExcludedFromDiscovery);
        }
    }
}
