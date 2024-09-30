using DotnetAffected.Testing.Utils;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DotnetAffected.Core.Tests
{
    /// <summary>
    /// Tests for detecting changed projects when using a Traversal SDK project to filter.
    /// This should cover all tests where filtering should be applied
    /// </summary>
    public class ChangeDetectionUsingTraversalProjectTests
        : BaseDotnetAffectedTest
    {
        private string traversalProjectPath = "traversal-test.proj";

        protected override AffectedOptions Options => new AffectedOptions(
            this.Repository.Path,
            Path.Combine(this.Repository.Path, this.traversalProjectPath));

        [Fact]
        public async Task When_project_inside_filtering_file_has_changes_project_should_have_changed()
        {
            // Create a project
            var projectName = "InventoryManagement";
            var msBuildProject = this.Repository.CreateCsProject(projectName);

            // Create a traversal project which includes the project
            await this.Repository.CreateTraversalProjectAsync(traversalProjectPath, msBuildProject.FullPath);

            Assert.Single(AffectedSummary.ProjectsWithChangedFiles);
            Assert.Empty(AffectedSummary.AffectedProjects);

            var projectInfo = AffectedSummary.ProjectsWithChangedFiles.Single();
            Assert.Equal(projectName, projectInfo.GetProjectName());
            Assert.Equal(msBuildProject.FullPath, projectInfo.GetFullPath());
        }
        
        [Fact]
        public async Task When_project_inside_filtering_file_with_expressions_has_changes_project_should_have_changed()
        {
            // Create a project
            var projectName = "InventoryManagement";
            var msBuildProject = this.Repository.CreateCsProject(projectName);

            // Create a traversal project which includes the project
            await this.Repository.CreateTraversalProjectAsync(traversalProjectPath, p =>
            {
                p.AddItem("ProjectReference", "**/**/InventoryManagement.csproj");
            });

            Assert.Single(AffectedSummary.ProjectsWithChangedFiles);
            Assert.Empty(AffectedSummary.AffectedProjects);

            var projectInfo = AffectedSummary.ProjectsWithChangedFiles.Single();
            Assert.Equal(projectName, projectInfo.GetProjectName());
            Assert.Equal(msBuildProject.FullPath, projectInfo.GetFullPath());
        }
        
        [Fact]
        public async Task When_project_inside_filtering_file_with_expressions_upper_path_has_changes_project_should_have_changed()
        {
            // Create a project
            var projectName = "InventoryManagement";
            var msBuildProject = this.Repository.CreateCsProject(projectName);

            var tvPath = Path.Join(this.Repository.Path, "/traversals");
            Directory.CreateDirectory(tvPath);
            traversalProjectPath = Path.Join(tvPath, "traversal-test.proj");
            // Create a traversal project which includes the project
            await this.Repository.CreateTraversalProjectAsync(traversalProjectPath, p =>
            {
                p.AddItem("ProjectReference", "../../**/**/InventoryManagement.csproj");
            });

            Assert.Single(AffectedSummary.ProjectsWithChangedFiles);
            Assert.Empty(AffectedSummary.AffectedProjects);

            var projectInfo = AffectedSummary.ProjectsWithChangedFiles.Single();
            Assert.Equal(projectName, projectInfo.GetProjectName());
            Assert.Equal(msBuildProject.FullPath, projectInfo.GetFullPath());
        }

        [Fact]
        public async Task When_project_inside_filtering_file_should_ignore_changes_outside_filtering_file()
        {
            // Create a project
            var projectName = "InventoryManagement";
            var msBuildProject = this.Repository.CreateCsProject(projectName);

            // Create a traversal project which includes the project
            await this.Repository.CreateTraversalProjectAsync(traversalProjectPath, msBuildProject.FullPath);

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
        public async Task When_project_outside_filtering_file_has_changed_nothing_should_be_affected()
        {
            // Create a project
            var projectName = "InventoryManagement";
            var msBuildProject = this.Repository.CreateCsProject(projectName);

            // Create a traversal project which includes the project
            await this.Repository.CreateTraversalProjectAsync(traversalProjectPath, msBuildProject.FullPath);

            // Commit so there are no changes
            this.Repository.StageAndCommit();

            // Create a project that is outside the solution
            var outsiderName = "OutsiderProject";
            this.Repository.CreateCsProject(outsiderName);

            Assert.Empty(AffectedSummary.AffectedProjects);
        }
    }
}
