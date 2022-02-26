using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Affected.Cli.Tests
{
    /// <summary>
    /// Ensures that when a project has changed, dependant projects are affected by those changes.
    /// This should consider all cases where a project should be affected by a set of changes.
    /// </summary>
    public class AffectedDetectionTests
        : BaseServiceProviderRepositoryTest
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

            Assert.Single(Context.ChangedProjects);
            Assert.Single(Context.AffectedProjects);

            var changedProject = Context.ChangedProjects.Single();
            Assert.Equal(projectName, changedProject.Name);
            Assert.Equal(msBuildProject.FullPath, changedProject.FilePath);

            var affectedProject = Context.AffectedProjects.Single();
            Assert.Equal(dependantProjectName, affectedProject.Name);
            Assert.Equal(dependantMsBuildProject.FullPath, affectedProject.FilePath);
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

            Assert.Single(Context.ChangedProjects);
            Assert.Equal(2, Context.AffectedProjects.Count());

            var changedProject = Context.ChangedProjects.Single();
            Assert.Equal(sharedProjectName, changedProject.Name);
            Assert.Equal(sharedMsBuildProject.FullPath, changedProject.FilePath);

            var firstAffectedProject = Context.AffectedProjects.First();
            Assert.Equal(projectName, firstAffectedProject.Name);
            Assert.Equal(msBuildProject.FullPath, firstAffectedProject.FilePath);

            var secondAffectedProject = Context.AffectedProjects.ElementAt(1);
            Assert.Equal(dependantProjectName, secondAffectedProject.Name);
            Assert.Equal(dependantMsBuildProject.FullPath, secondAffectedProject.FilePath);
        }
    }
}
