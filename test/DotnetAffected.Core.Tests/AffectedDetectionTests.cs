using DotnetAffected.Testing.Utils;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DotnetAffected.Core.Tests
{
    /// <summary>
    /// Ensures that when a project has changed, dependant projects are affected by those changes.
    /// This should consider all cases where a project should be affected by a set of changes.
    /// </summary>
    public class AffectedDetectionTests
        : BaseDotnetAffectedTest
    {
        [Fact]
        public async Task When_changes_are_made_to_a_project_dependant_projects_should_be_affected()
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

            Assert.Single(AffectedSummary.ProjectsWithChangedFiles);
            Assert.Single(AffectedSummary.AffectedProjects);

            var changedProject = AffectedSummary.ProjectsWithChangedFiles.Single();
            Assert.Equal(projectName, changedProject.GetProjectName());
            Assert.Equal(msBuildProject.FullPath, changedProject.GetFullPath());

            var affectedProject = AffectedSummary.AffectedProjects.Single();
            Assert.Equal(dependantProjectName, affectedProject.GetProjectName());
            Assert.Equal(dependantMsBuildProject.FullPath, affectedProject.GetFullPath());
        }

        [Fact]
        public async Task When_recursively_changes_are_made_to_a_project_dependant_projects_should_be_affected()
        {
            // Create a shared project
            var sharedProjectName = "InventoryManagement.Domain";
            var sharedMsBuildProject = this.Repository.CreateCsProject(sharedProjectName);

            // Create a project
            var projectName = "InventoryManagement";
            var msBuildProject = this.Repository.CreateCsProject(
                projectName,
                p => p.AddProjectDependency(sharedMsBuildProject.FullPath));

            // Create another project that depends on the first one
            var dependantProjectName = "InventoryManagement.Tests";
            var dependantMsBuildProject = this.Repository.CreateCsProject(
                dependantProjectName,
                p => p.AddProjectDependency(msBuildProject.FullPath));

            // Commit so there are no changes
            this.Repository.StageAndCommit();

            // Create changes in the first project
            var targetFilePath = Path.Combine(sharedProjectName, "file.cs");
            await this.Repository.CreateTextFileAsync(targetFilePath, "// Initial content");

            Assert.Single(AffectedSummary.ProjectsWithChangedFiles);
            Assert.Equal(2, AffectedSummary.AffectedProjects.Count());

            var changedProject = AffectedSummary.ProjectsWithChangedFiles.Single();
            Assert.Equal(sharedProjectName, changedProject.GetProjectName());
            Assert.Equal(sharedMsBuildProject.FullPath, changedProject.GetFullPath());

            Assert.Contains(AffectedSummary.AffectedProjects,
                p => p.GetFullPath() == msBuildProject.FullPath);

            Assert.Contains(AffectedSummary.AffectedProjects,
                p => p.GetFullPath() == dependantMsBuildProject.FullPath);
        }

        [Fact]
        public async Task When_changes_are_made_to_an_fsharp_project_dependant_projects_should_be_affected()
        {
            // Create a project
            var projectName = "InventoryManagement";
            var fileFs = "file.fs";
            var targetFilePath = Path.Combine(projectName, fileFs);
            var msBuildProject = this.Repository.CreateFsProject(projectName, p =>
            {
                // REMARKS: This is required for fsharp projects
                p.AddItemGroup()
                    .AddItem("Compile", fileFs);
            });

            // Create another project that depends on the first one
            var dependantProjectName = "InventoryManagement.Tests";
            var dependantMsBuildProject = this.Repository.CreateFsProject(
                dependantProjectName,
                p => p.AddProjectDependency(msBuildProject.FullPath));

            // Commit so there are no changes
            this.Repository.StageAndCommit();

            // Create changes in the first project
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

        [Fact]
        public async Task When_changes_are_made_to_a_visualbasic_project_dependant_projects_should_be_affected()
        {
            // Create a project
            var projectName = "InventoryManagement";
            var fileFs = "file.vb";
            var targetFilePath = Path.Combine(projectName, fileFs);
            var msBuildProject = this.Repository.CreateVbProject(projectName);

            // Create another project that depends on the first one
            var dependantProjectName = "InventoryManagement.Tests";
            var dependantMsBuildProject = this.Repository.CreateVbProject(
                dependantProjectName,
                p => p.AddProjectDependency(msBuildProject.FullPath));

            // Commit so there are no changes
            this.Repository.StageAndCommit();

            // Create changes in the first project
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

        [Fact]
        public async Task When_changes_are_made_to_a_non_sdk_project_dependant_projects_should_be_affected()
        {
            // Create a project
            var projectName = "InventoryManagement";
            var fileCs = "file.cs";

            var projPath = Path.Combine(Repository.Path, $"{projectName}.csproj");
            var msBuildProject = Repository.CreateNonSdkMsBuildProject(projectName, ".csproj", p =>
            {
                p.AddItemGroup()
                    .AddItem("Compile", fileCs);
            });

            // Create another project that depends on the first one
            var dependantProjectName = "InventoryManagement.Tests";
            var dependantMsBuildProject = this.Repository.CreateNonSdkMsBuildProject(
                dependantProjectName,
                ".csproj",
                p => p.AddProjectDependency(msBuildProject.FullPath));

            // Commit so there are no changes
            this.Repository.StageAndCommit();

            // Create changes in the first project
            await this.Repository.CreateTextFileAsync(Path.Join(projectName, fileCs), "// Initial content");

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
