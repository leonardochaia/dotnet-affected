using DotnetAffected.Abstractions;
using DotnetAffected.Testing.Utils;
using System;
using System.Linq;
using Xunit;

namespace DotnetAffected.Core.Tests
{
    public class AssumeChangesTests : BaseRepositoryTest
    {
        private readonly string _projectName = "InventoryManagement";

        private readonly Lazy<AffectedSummary> _affectedSummaryLazy;

        public AssumeChangesTests()
        {
            var options = new AffectedOptions(this.Repository.Path);
            this._affectedSummaryLazy = new Lazy<AffectedSummary>(() =>
            {
                var factory = new ProjectGraphFactory(options);
                var graph = factory.BuildProjectGraph();
                var changesProvider = new AssumptionChangesProvider(graph, new[]
                {
                    _projectName
                });
                var executor = new AffectedExecutor(options, graph, changesProvider);
                return executor.Execute();
            });
        }

        private AffectedSummary AffectedSummary => _affectedSummaryLazy.Value;

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
