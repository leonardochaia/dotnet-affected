using DotnetAffected.Testing.Utils;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DotnetAffected.Core.Tests
{
    /// <summary>
    /// Tests for detecting when a set of projects have changed.
    /// This should cover all possible change detection scenarios.
    /// </summary>
    public class GitChangeDetectionTests
        : BaseDotnetAffectedTest
    {
        [Fact]
        public void When_has_changes_project_should_have_changed()
        {
            // Create a project
            var projectName = "InventoryManagement";
            var msBuildProject = Repository.CreateCsProject(projectName);

            Assert.Single(AffectedSummary.FilesThatChanged);
            Assert.Single(AffectedSummary.ProjectsWithChangedFiles);
            Assert.Empty(AffectedSummary.AffectedProjects);

            var projectInfo = AffectedSummary.ProjectsWithChangedFiles.Single();
            Assert.Equal(projectName, projectInfo.GetProjectName());
            Assert.Equal(msBuildProject.FullPath, projectInfo.GetFullPath());
        }

        [Fact]
        public async Task When_has_changes_to_file_inside_project_directory_project_should_have_changes()
        {
            // Create a project
            var projectName = "InventoryManagement";
            var msBuildProject = Repository.CreateCsProject(projectName);

            // Create a file with some changes
            var targetFilePath = Path.Combine(projectName, "file.cs");
            await this.Repository.CreateTextFileAsync(targetFilePath, "// Initial content");

            Assert.Equal(2, AffectedSummary.FilesThatChanged.Count());
            Assert.Single(AffectedSummary.ProjectsWithChangedFiles);
            Assert.Empty(AffectedSummary.AffectedProjects);

            var projectInfo = AffectedSummary.ProjectsWithChangedFiles.Single();
            Assert.Equal(projectName, projectInfo.GetProjectName());
            Assert.Equal(msBuildProject.FullPath, projectInfo.GetFullPath());
        }

        /// <summary>
        /// Regression test for https://github.com/leonardochaia/dotnet-affected/issues/84
        /// Deleted files no longer exist on disk, so they are not an input of any project
        /// in the current graph. The owning project must still be reported as changed,
        /// otherwise its build/test is silently skipped.
        /// </summary>
        [Fact]
        public async Task When_file_inside_project_directory_is_deleted_project_should_have_changes()
        {
            // Create a project with a source file, and commit both so that
            // the deletion is the only pending change.
            var projectName = "InventoryManagement";
            var msBuildProject = Repository.CreateCsProject(projectName);

            var targetFilePath = Path.Combine(projectName, "file.cs");
            await this.Repository.CreateTextFileAsync(targetFilePath, "// Initial content");

            this.Repository.StageAndCommit();

            this.Repository.DeleteFile(targetFilePath);

            Assert.Single(AffectedSummary.FilesThatChanged);

            var projectInfo = Assert.Single(AffectedSummary.ProjectsWithChangedFiles);
            Assert.Equal(projectName, projectInfo.GetProjectName());
            Assert.Equal(msBuildProject.FullPath, projectInfo.GetFullPath());
        }

        /// <summary>
        /// The containment fallback for deleted files must reach files nested any number
        /// of directories below the project, not just its immediate children.
        /// </summary>
        [Fact]
        public async Task When_nested_file_inside_project_directory_is_deleted_project_should_have_changes()
        {
            var projectName = "InventoryManagement";
            var msBuildProject = Repository.CreateCsProject(projectName);

            var targetFilePath = Path.Combine(projectName, "Services", "Nested", "OrderService.cs");
            await this.Repository.CreateTextFileAsync(targetFilePath, "// Initial content");

            this.Repository.StageAndCommit();

            this.Repository.DeleteFile(targetFilePath);

            Assert.Single(AffectedSummary.FilesThatChanged);

            var projectInfo = Assert.Single(AffectedSummary.ProjectsWithChangedFiles);
            Assert.Equal(projectName, projectInfo.GetProjectName());
            Assert.Equal(msBuildProject.FullPath, projectInfo.GetFullPath());
        }

        /// <summary>
        /// The containment fallback must not over-attribute: deleting a file that lives
        /// outside every project directory should not mark any project as changed.
        /// </summary>
        [Fact]
        public async Task When_file_outside_any_project_directory_is_deleted_no_project_should_have_changes()
        {
            Repository.CreateCsProject("InventoryManagement");

            await this.Repository.CreateTextFileAsync("README.md", "# Initial content");

            this.Repository.StageAndCommit();

            this.Repository.DeleteFile("README.md");

            Assert.Single(AffectedSummary.FilesThatChanged);
            Assert.Empty(AffectedSummary.ProjectsWithChangedFiles);
            Assert.Empty(AffectedSummary.AffectedProjects);
        }
    }
}
