using DotnetAffected.Abstractions;
using DotnetAffected.Testing.Utils;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DotnetAffected.Core.Tests
{
    /// <summary>
    /// Edges of excluding projects from discovery, where it interacts with the rest of the pipeline.
    /// </summary>
    public class ExcludeDiscoveryTests : BaseRepositoryTest
    {
        private const string SolutionPath = "test-solution.sln";

        /// <summary>
        /// Unconditionally imports something that is not there, which is what makes a legacy SSDT
        /// database project unevaluatable outside of Visual Studio.
        /// </summary>
        private const string UnevaluatableProject = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project ToolsVersion=""4.0"" DefaultTargets=""Build"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <Import Project=""$(MSBuildExtensionsPath)\Microsoft\VisualStudio\v$(VisualStudioVersion)\SSDT\Microsoft.Data.Tools.Schema.SqlTasks.targets"" />
</Project>
";

        private AffectedSummary Execute(string excludeDiscoveryRegex, params string[] assumeChanges)
            => new AffectedExecutor(new AffectedOptions(
                    Repository.Path,
                    Path.Combine(Repository.Path, SolutionPath),
                    assumeChanges: assumeChanges.Length == 0 ? null : assumeChanges,
                    excludeDiscoveryRegex: excludeDiscoveryRegex))
                .Execute();

        [Fact]
        public async Task Excluded_projects_should_be_reported_by_path()
        {
            var project = Repository.CreateCsProject("InventoryManagement");

            var sqlProjectPath = Path.Combine("Database", "Database.sqlproj");
            await Repository.CreateTextFileAsync(sqlProjectPath, UnevaluatableProject);

            await Repository.CreateSolutionAsync(
                SolutionPath, project.FullPath, Path.Combine(Repository.Path, sqlProjectPath));

            var summary = Execute(@"\.sqlproj$");

            var excluded = Assert.Single(summary.ProjectsExcludedFromDiscovery);
            Assert.Equal(Path.Combine(Repository.Path, sqlProjectPath), excluded);
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
            var sqlProjectPath = Path.Combine(Repository.Path, "Database", "Database.sqlproj");
            await Repository.CreateTextFileAsync(
                Path.Combine("Database", "Database.sqlproj"), UnevaluatableProject);

            // The reference is what drags the excluded project back into evaluation.
            var project = Repository.CreateCsProject(
                "InventoryManagement",
                p => p.AddProjectDependency(sqlProjectPath));

            await Repository.CreateSolutionAsync(SolutionPath, project.FullPath, sqlProjectPath);

            var exception = Record.Exception(() => Execute(@"\.sqlproj$"));

            Assert.NotNull(exception);
        }

        /// <summary>
        /// An excluded project is not in the graph, so it cannot be named as an assumed change.
        /// Failing loudly beats reporting nothing as affected, which reads like a correct answer.
        /// </summary>
        [Fact]
        public async Task When_assuming_changes_for_an_excluded_project_should_fail()
        {
            var project = Repository.CreateCsProject("InventoryManagement");

            var sqlProjectPath = Path.Combine("Database", "Database.sqlproj");
            await Repository.CreateTextFileAsync(sqlProjectPath, UnevaluatableProject);

            await Repository.CreateSolutionAsync(
                SolutionPath, project.FullPath, Path.Combine(Repository.Path, sqlProjectPath));

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
