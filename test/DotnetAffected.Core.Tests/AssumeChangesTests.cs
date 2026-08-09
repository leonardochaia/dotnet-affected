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
            var options = new AffectedOptions(this.Repository.Path, assumeChanges: new[]
            {
                _projectName
            });

            this._affectedSummaryLazy = new Lazy<AffectedSummary>(
                () => new AffectedExecutor(options).Execute());
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
        public void When_assumption_matches_no_project_should_throw()
        {
            this.Repository.CreateCsProject(_projectName);
            this.Repository.StageAndCommit();

            var options = new AffectedOptions(this.Repository.Path, assumeChanges: new[]
            {
                "NoSuchProject"
            });

            var exception = Assert.Throws<AssumedProjectNotFoundException>(
                () => new AffectedExecutor(options).Execute());

            Assert.Equal("NoSuchProject", exception.Assumption);
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
