using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Affected.Cli.Tests
{
    /// <summary>
    /// Tests for detecting when a set of projects have changed.
    /// This should cover all possible change detection scenarios.
    /// </summary>
    public class GitChangeDetectionTests
        : BaseServiceProviderRepositoryTest
    {
        [Fact]
        public void When_has_changes_project_should_have_changed()
        {
            // Create a project
            var projectName = "InventoryManagement";
            var msBuildProject = Repository.CreateCsProject(projectName);
            
            Assert.Single(Context.ChangedProjects);
            Assert.Empty(Context.AffectedProjects);

            var projectInfo = Context.ChangedProjects.Single();
            Assert.Equal(projectName, projectInfo.Name);
            Assert.Equal(msBuildProject.FullPath, projectInfo.FilePath);
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

            Assert.Single(Context.ChangedProjects);
            Assert.Empty(Context.AffectedProjects);

            var projectInfo = Context.ChangedProjects.Single();
            Assert.Equal(projectName, projectInfo.Name);
            Assert.Equal(msBuildProject.FullPath, projectInfo.FilePath);
        }

        [Fact]
        public void When_nothing_has_changed_should_throw_nothing_has_changed()
        {
            Assert.Throws<NoChangesException>(() => Context.ChangedProjects);
        }
    }
}
