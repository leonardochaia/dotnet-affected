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
    public class GitBetweenCommitsChangeDetectionTests
        : BaseDotnetAffectedTest
    {
        private string _fromCommit;
        private string _toCommit;

        protected override AffectedOptions Options =>
            new AffectedOptions(this.Repository.Path, fromRef: _fromCommit, toRef: _toCommit);

        [Fact]
        public async Task When_has_changes_project_should_have_changed()
        {
            // Create a project
            var projectName = "InventoryManagement";
            var msBuildProject = Repository.CreateCsProject(projectName);

            // Make a commit and keep track of the sha
            this._fromCommit = Repository.StageAndCommit()
                .Sha;

            // Create a file with some changes
            var targetFilePath = Path.Combine(projectName, "file.cs");
            await this.Repository.CreateTextFileAsync(targetFilePath, "// Initial content");

            // Commit the changes
            this._toCommit = Repository.StageAndCommit()
                .Sha;

            Assert.Single(AffectedSummary.ProjectsWithChangedFiles);
            Assert.Empty(AffectedSummary.AffectedProjects);

            var projectInfo = AffectedSummary.ProjectsWithChangedFiles.Single();
            Assert.Equal(projectName, projectInfo.GetProjectName());
            Assert.Equal(msBuildProject.FullPath, projectInfo.GetFullPath());
        }
    }
}
