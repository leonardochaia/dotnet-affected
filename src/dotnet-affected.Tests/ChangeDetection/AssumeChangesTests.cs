using Affected.Cli.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;
using Xunit;

namespace Affected.Cli.Tests
{
    public class AssumeChangesTests : BaseServiceProviderRepositoryTest
    {
        private readonly string _projectName = "InventoryManagement";

        protected override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);

            services.Replace(ServiceDescriptor.Singleton(new CommandExecutionData(
                this.Repository.Path,
                string.Empty,
                String.Empty,
                String.Empty,
                true,
                new[]
                {
                    _projectName
                },
                new string[0],
                true,
                string.Empty,
                string.Empty)));
        }

        [Fact]
        public void When_has_changes_project_should_have_changes()
        {
            // Create a project and commit so there are no changes
            var msBuildProject = this.Repository.CreateCsProject(_projectName);
            this.Repository.StageAndCommit();

            Assert.Single(AffectedSummary.ProjectsWithChangedFiles);
            Assert.Empty(AffectedSummary.AffectedProjects);

            var projectInfo = AffectedSummary.ProjectsWithChangedFiles.Single();
            Assert.Equal(_projectName, projectInfo.GetProjectName());
            Assert.Equal(msBuildProject.FullPath, projectInfo.GetFullPath());
        }

        [Fact]
        public void Using_assume_changes_should_ignore_other_changes()
        {
            // Create a project
            var msBuildProject = this.Repository.CreateCsProject(_projectName);

            // Create a second project
            var otherName = "OtherProjectWhichHasChanges";
            this.Repository.CreateCsProject(otherName);

            // Commit so there are no changes
            this.Repository.StageAndCommit();

            Assert.Single(AffectedSummary.ProjectsWithChangedFiles);
            Assert.Empty(AffectedSummary.AffectedProjects);

            var projectInfo = AffectedSummary.ProjectsWithChangedFiles.Single();
            Assert.Equal(_projectName, projectInfo.GetProjectName());
            Assert.Equal(msBuildProject.FullPath, projectInfo.GetFullPath());
        }
    }
}
