using Affected.Cli.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
    public class GitBetweenCommitsChangeDetectionTests
        : BaseServiceProviderRepositoryTest
    {
        private string _fromCommit;
        private string _toCommit;

        protected override void ConfigureServices(IServiceCollection services)
        {
            services.Replace(ServiceDescriptor.Singleton(new CommandExecutionData(
                this.Repository.Path,
                string.Empty,
                this._fromCommit,
                this._toCommit,
                true,
                Enumerable.Empty<string>(),
                Array.Empty<string>(),
                true,
                string.Empty,
                string.Empty)));
        }

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
