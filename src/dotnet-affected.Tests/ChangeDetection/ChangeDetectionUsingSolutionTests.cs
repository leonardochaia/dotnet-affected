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
    /// Tests for detecting changed projects when using a SolutionFile to filter.
    /// This should cover all tests where filtering should be applied
    /// </summary>
    public class ChangeDetectionUsingSolutionTests
        : BaseServiceProviderRepositoryTest
    {
        private readonly string _solutionPath = "test-solution.sln";

        protected override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);

            services.Replace(ServiceDescriptor.Singleton(new CommandExecutionData(
                this.Repository.Path,
                Path.Combine(this.Repository.Path, this._solutionPath),
                String.Empty,
                String.Empty,
                true,
                Enumerable.Empty<string>(), 
                Array.Empty<string>(),
                true,
                string.Empty,
                string.Empty)));
        }

        [Fact]
        public async Task When_project_inside_solution_has_changes_project_should_have_changed()
        {
            // Create a project
            var projectName = "InventoryManagement";
            var msBuildProject = this.Repository.CreateCsProject(projectName);

            // Create a solution which includes the project
            await this.Repository.CreateSolutionAsync(_solutionPath, msBuildProject.FullPath);

            Assert.Single(Context.ChangedProjects);
            Assert.Empty(Context.AffectedProjects);

            var projectInfo = Context.ChangedProjects.Single();
            Assert.Equal(projectName, projectInfo.Name);
            Assert.Equal(msBuildProject.FullPath, projectInfo.FilePath);
        }

        [Fact]
        public async Task When_project_inside_solution_should_ignore_changes_outside_solution()
        {
            // Create a project
            var projectName = "InventoryManagement";
            var msBuildProject = this.Repository.CreateCsProject(projectName);

            // Create a solution which includes the project
            await this.Repository.CreateSolutionAsync(_solutionPath, msBuildProject.FullPath);

            // Create a project that is outside the solution
            var outsiderproject = "OutsiderProject";
            this.Repository.CreateCsProject(outsiderproject);

            Assert.Single(Context.ChangedProjects);
            Assert.Empty(Context.AffectedProjects);

            var projectInfo = Context.ChangedProjects.Single();
            Assert.Equal(projectName, projectInfo.Name);
            Assert.Equal(msBuildProject.FullPath, projectInfo.FilePath);
        }

        [Fact]
        public async Task When_project_outside_solution_has_changed_should_throw_nothing_changed()
        {
            // Create a project
            var projectName = "InventoryManagement";
            var msBuildProject = this.Repository.CreateCsProject(projectName);

            // Create a solution which includes the project
            await this.Repository.CreateSolutionAsync(_solutionPath, msBuildProject.FullPath);

            // Commit so there are no changes
            this.Repository.StageAndCommit();

            // Create a project that is outside the solution
            var outsiderName = "OutsiderProject";
            this.Repository.CreateCsProject(outsiderName);

            Assert.Throws<NoChangesException>(() => Context.AffectedProjects);
        }
    }
}
