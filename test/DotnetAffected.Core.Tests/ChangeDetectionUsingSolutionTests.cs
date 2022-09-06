using DotnetAffected.Testing.Utils;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DotnetAffected.Core.Tests
{
    /// <summary>
    /// Tests for detecting changed projects when using a SolutionFile to filter.
    /// This should cover all tests where filtering should be applied
    /// </summary>
    public class ChangeDetectionUsingSolutionTests
        : BaseDotnetAffectedTest
    {
        private readonly string _solutionPath = "test-solution.sln";

        protected override AffectedOptions Options => new AffectedOptions(
            this.Repository.Path,
            Path.Combine(this.Repository.Path, this._solutionPath));

        [Fact]
        public async Task When_project_inside_solution_has_changes_project_should_have_changed()
        {
            // Create a project
            var projectName = "InventoryManagement";
            var msBuildProject = this.Repository.CreateCsProject(projectName);

            // Create a solution which includes the project
            await this.Repository.CreateSolutionAsync(_solutionPath, msBuildProject.FullPath);

            Assert.Single(AffectedSummary.ProjectsWithChangedFiles);
            Assert.Empty(AffectedSummary.AffectedProjects);

            var projectInfo = AffectedSummary.ProjectsWithChangedFiles.Single();
            Assert.Equal(projectName, projectInfo.GetProjectName());
            Assert.Equal(msBuildProject.FullPath, projectInfo.GetFullPath());
        }

        [Fact]
        public async Task When_project_inside_solution_should_ignore_changes_outside_solution()
        {
            // Create a project
            var projectName = "InventoryManagement";
            var msBuildProject = this.Repository.CreateCsProject(projectName);

            // Create a solution which includes the project
            await this.Repository.CreateSolutionAsync(_solutionPath, msBuildProject.FullPath);

            // Create a project that is outside the solution
            var outsiderproject = "OutsiderProject";
            this.Repository.CreateCsProject(outsiderproject);

            Assert.Single(AffectedSummary.ProjectsWithChangedFiles);
            Assert.Empty(AffectedSummary.AffectedProjects);

            var projectInfo = AffectedSummary.ProjectsWithChangedFiles.Single();
            Assert.Equal(projectName, projectInfo.GetProjectName());
            Assert.Equal(msBuildProject.FullPath, projectInfo.GetFullPath());
        }

        [Fact]
        public async Task When_project_outside_solution_has_changed_nothing_should_be_affected()
        {
            // Create a project
            var projectName = "InventoryManagement";
            var msBuildProject = this.Repository.CreateCsProject(projectName);

            // Create a solution which includes the project
            await this.Repository.CreateSolutionAsync(_solutionPath, msBuildProject.FullPath);

            // Commit so there are no changes
            this.Repository.StageAndCommit();

            // Create a project that is outside the solution
            var outsiderName = "OutsiderProject";
            this.Repository.CreateCsProject(outsiderName);

            Assert.Empty(AffectedSummary.AffectedProjects);
        }
    }
}
