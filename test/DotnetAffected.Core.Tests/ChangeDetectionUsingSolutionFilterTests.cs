using DotnetAffected.Testing.Utils;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DotnetAffected.Core.Tests
{
    /// <summary>
    /// Tests for detecting changed projects when using a Solution Filter to filter.
    /// This should cover all tests where filtering should be applied
    /// </summary>
    public class ChangeDetectionUsingSolutionFilterTests
        : BaseDotnetAffectedTest
    {
        private readonly string _solutionPath = "test-solution.slnx";
        private readonly string _solutionFilterPath = "test-solution-filter.slnf";

        protected override AffectedOptions Options => new AffectedOptions(
            this.Repository.Path,
            Path.Combine(this.Repository.Path, this._solutionFilterPath));

        [Fact]
        public async Task When_project_inside_solution_filter_has_changes_project_should_have_changed()
        {
            // Create a project
            var projectName = "InventoryManagement";
            var msBuildProject = this.Repository.CreateCsProject(projectName);

            // Create a project that is outside the solution filter
            var outsiderProject = "OutsiderProject";
            var msBuildOutsiderProject = this.Repository.CreateCsProject(outsiderProject);

            // Create a solution which includes the projects
            await this.Repository.CreateXmlSolutionAsync(_solutionPath, msBuildProject.FullPath,
                msBuildOutsiderProject.FullPath);

            // Create a solution filter which includes one of the projects
            await this.Repository.CreateSolutionFilterAsync(_solutionFilterPath, _solutionPath,
                msBuildProject.FullPath);

            Assert.Single(AffectedSummary.ProjectsWithChangedFiles);
            Assert.Empty(AffectedSummary.AffectedProjects);

            var projectInfo = AffectedSummary.ProjectsWithChangedFiles.Single();
            Assert.Equal(projectName, projectInfo.GetProjectName());
            Assert.Equal(msBuildProject.FullPath, projectInfo.GetFullPath());
        }

        [Fact]
        public async Task When_project_outside_solution_filter_has_changed_nothing_should_be_affected()
        {
            var msBuildProject = this.Repository.CreateCsProject("InventoryManagement");
            var msBuildOutsiderProject = this.Repository.CreateCsProject("OutsiderProject");

            // The solution has both projects, the filter only keeps one of them
            await this.Repository.CreateXmlSolutionAsync(_solutionPath, msBuildProject.FullPath,
                msBuildOutsiderProject.FullPath);
            await this.Repository.CreateSolutionFilterAsync(_solutionFilterPath, _solutionPath,
                msBuildProject.FullPath);

            // Commit so there are no changes
            this.Repository.StageAndCommit();

            // Change the project the filter excludes
            await this.Repository.CreateTextFileAsync(msBuildOutsiderProject, "file.cs", "// changed");

            Assert.Empty(AffectedSummary.ProjectsWithChangedFiles);
            Assert.Empty(AffectedSummary.AffectedProjects);
        }
    }

    /// <summary>
    /// Solution filters are commonly generated into an output directory rather than
    /// next to the solution, which means solution.path points back up the tree.
    /// </summary>
    public class ChangeDetectionUsingNestedSolutionFilterTests
        : BaseDotnetAffectedTest
    {
        private readonly string _solutionPath = "test-solution.slnx";
        private readonly string _solutionFilterPath = Path.Combine("artifacts", "test-solution-filter.slnf");

        protected override AffectedOptions Options => new AffectedOptions(
            this.Repository.Path,
            Path.Combine(this.Repository.Path, this._solutionFilterPath));

        [Fact]
        public async Task When_filter_is_not_next_to_the_solution_projects_should_still_resolve()
        {
            var projectName = "InventoryManagement";
            var msBuildProject = this.Repository.CreateCsProject(projectName);

            var msBuildOutsiderProject = this.Repository.CreateCsProject("OutsiderProject");

            await this.Repository.CreateXmlSolutionAsync(_solutionPath, msBuildProject.FullPath,
                msBuildOutsiderProject.FullPath);

            Directory.CreateDirectory(Path.Combine(this.Repository.Path, "artifacts"));
            await this.Repository.CreateSolutionFilterAsync(_solutionFilterPath, _solutionPath,
                msBuildProject.FullPath);

            Assert.Single(AffectedSummary.ProjectsWithChangedFiles);
            Assert.Empty(AffectedSummary.AffectedProjects);

            var projectInfo = AffectedSummary.ProjectsWithChangedFiles.Single();
            Assert.Equal(projectName, projectInfo.GetProjectName());
            Assert.Equal(msBuildProject.FullPath, projectInfo.GetFullPath());
        }
    }
}
