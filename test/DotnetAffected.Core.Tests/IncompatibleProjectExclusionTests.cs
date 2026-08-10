using DotnetAffected.Testing.Utils;
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
    /// while discovering projects, before they are ever handed to the graph.
    /// </summary>
    public class IncompatibleProjectExclusionTests : BaseDotnetAffectedTest
    {
        private const string SolutionPath = "test-solution.sln";

        /// <summary>
        /// Mimics the import that makes a legacy SSDT database project unevaluatable outside of
        /// Visual Studio. The import is unconditional, so evaluation throws
        /// InvalidProjectFileException.
        /// </summary>
        private const string SqlProjectContents = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project ToolsVersion=""4.0"" DefaultTargets=""Build"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <PropertyGroup>
    <Configuration Condition="" '$(Configuration)' == '' "">Debug</Configuration>
    <Platform Condition="" '$(Platform)' == '' "">AnyCPU</Platform>
    <Name>Database</Name>
  </PropertyGroup>
  <Import Project=""$(MSBuildExtensionsPath)\Microsoft\VisualStudio\v$(VisualStudioVersion)\SSDT\Microsoft.Data.Tools.Schema.SqlTasks.targets"" />
</Project>
";

        protected override AffectedOptions Options => new AffectedOptions(
            Repository.Path,
            Path.Combine(Repository.Path, SolutionPath),
            excludeDiscoveryRegex: @"\.sqlproj$");

        [Fact]
        public async Task When_excluded_project_cannot_be_evaluated_it_should_not_fail()
        {
            var projectName = "InventoryManagement";
            var msBuildProject = Repository.CreateCsProject(projectName);

            var sqlProjectPath = Path.Combine("Database", "Database.sqlproj");
            await Repository.CreateTextFileAsync(sqlProjectPath, SqlProjectContents);

            await Repository.CreateSolutionAsync(
                SolutionPath,
                msBuildProject.FullPath,
                Path.Combine(Repository.Path, sqlProjectPath));

            var projectInfo = Assert.Single(AffectedSummary.ProjectsWithChangedFiles);
            Assert.Equal(msBuildProject.FullPath, projectInfo.GetFullPath());
        }

        [Fact]
        public async Task When_excluded_project_cannot_be_evaluated_other_projects_should_still_be_affected()
        {
            var projectName = "InventoryManagement";
            var msBuildProject = Repository.CreateCsProject(projectName);

            var dependantProjectName = "InventoryManagement.Tests";
            var dependantMsBuildProject = Repository.CreateCsProject(
                dependantProjectName,
                p => p.AddProjectDependency(msBuildProject.FullPath));

            var sqlProjectPath = Path.Combine("Database", "Database.sqlproj");
            await Repository.CreateTextFileAsync(sqlProjectPath, SqlProjectContents);

            await Repository.CreateSolutionAsync(
                SolutionPath,
                msBuildProject.FullPath,
                dependantMsBuildProject.FullPath,
                Path.Combine(Repository.Path, sqlProjectPath));

            Repository.StageAndCommit();

            await Repository.CreateTextFileAsync(Path.Combine(projectName, "file.cs"), "// Initial content");

            var changedProject = Assert.Single(AffectedSummary.ProjectsWithChangedFiles);
            Assert.Equal(msBuildProject.FullPath, changedProject.GetFullPath());

            var affectedProject = Assert.Single(AffectedSummary.AffectedProjects);
            Assert.Equal(dependantMsBuildProject.FullPath, affectedProject.GetFullPath());
        }
    }
}
