using DotnetAffected.Testing.Utils;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DotnetAffected.Core.Tests
{
    /// <summary>
    /// Tests that ensures that the exclusion regex pattern
    /// is applied for excluding projects.
    /// </summary>
    public class ProjectExclusionTests
        : BaseDotnetAffectedTest
    {
        protected override AffectedOptions Options =>
            new AffectedOptions(this.Repository.Path, exclusionRegex: ".Inventory.");

        [Fact]
        public async Task When_exclusion_regex_is_present_matching_projects_should_be_excluded()
        {
            // Create a project
            var projectName = "InventoryManagement";
            var msBuildProject = this.Repository.CreateCsProject(projectName);

            // Create another project that depends on the first one
            var dependantProjectName = "InventoryManagement.Tests";
            var dependantMsBuildProject = this.Repository.CreateCsProject(
                dependantProjectName,
                p => p.AddProjectDependency(msBuildProject.FullPath));

            // Commit so there are no changes
            this.Repository.StageAndCommit();

            // Create changes in the first project
            var targetFilePath = Path.Combine(projectName, "file.cs");
            await this.Repository.CreateTextFileAsync(targetFilePath, "// Initial content");

            Assert.Empty(AffectedSummary.ProjectsWithChangedFiles);
            Assert.Empty(AffectedSummary.AffectedProjects);
        }

        [Fact]
        public async Task When_exclusion_regex_is_present_only_matching_projects_should_be_excluded()
        {
            // Create a project
            var projectName = "PurchasingManagement";
            var msBuildProject = this.Repository.CreateCsProject(projectName);

            // Create another project that depends on the first one
            var dependantProjectName = "PurchasingManagement.Tests";
            var dependantMsBuildProject = this.Repository.CreateCsProject(
                dependantProjectName,
                p => p.AddProjectDependency(msBuildProject.FullPath));

            this.Repository.CreateCsProject("InventoryManagement");
            this.Repository.CreateCsProject("InventoryManagement.Api");
            this.Repository.CreateCsProject("InventoryManagement.Tests");

            // Commit so there are no changes
            this.Repository.StageAndCommit();

            // Create changes in the first project
            var targetFilePath = Path.Combine(projectName, "file.cs");
            await this.Repository.CreateTextFileAsync(targetFilePath, "// Initial content");

            Assert.Single(AffectedSummary.ProjectsWithChangedFiles);
            Assert.Single(AffectedSummary.AffectedProjects);

            var changedProject = AffectedSummary.ProjectsWithChangedFiles.Single();
            Assert.Equal(projectName, changedProject.GetProjectName());
            Assert.Equal(msBuildProject.FullPath, changedProject.GetFullPath());

            var affectedProject = AffectedSummary.AffectedProjects.Single();
            Assert.Equal(dependantProjectName, affectedProject.GetProjectName());
            Assert.Equal(dependantMsBuildProject.FullPath, affectedProject.GetFullPath());
        }
    }
}
