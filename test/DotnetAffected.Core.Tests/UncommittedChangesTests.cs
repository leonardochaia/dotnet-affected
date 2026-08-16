using DotnetAffected.Abstractions;
using DotnetAffected.Testing.Utils;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DotnetAffected.Core.Tests
{
    /// <summary>
    /// The working tree is the endpoint of every comparison. These cover how much of it counts.
    /// </summary>
    public class UncommittedChangesTests
        : BaseDotnetAffectedTest
    {
        private string _fromCommit;
        private UncommittedChanges _uncommitted = UncommittedChanges.All;

        protected override AffectedOptions Options =>
            new AffectedOptions(this.Repository.Path,
                fromRef: _fromCommit,
                uncommittedChanges: _uncommitted);

        [Fact]
        public async Task All_should_detect_unstaged_changes()
        {
            var projectName = "InventoryManagement";
            Repository.CreateCsProject(projectName);

            this._fromCommit = Repository.StageAndCommit()
                .Sha;

            await this.Repository.CreateTextFileAsync(Path.Combine(projectName, "file.cs"), "// Changes");

            var changed = Assert.Single(AffectedSummary.ProjectsWithChangedFiles);
            Assert.Equal(projectName, changed.GetProjectName());
        }

        [Fact]
        public async Task Staged_should_ignore_unstaged_changes()
        {
            this._uncommitted = UncommittedChanges.Staged;

            var projectName = "InventoryManagement";
            Repository.CreateCsProject(projectName);

            this._fromCommit = Repository.StageAndCommit()
                .Sha;

            await this.Repository.CreateTextFileAsync(Path.Combine(projectName, "file.cs"), "// Changes");

            Assert.Empty(AffectedSummary.FilesThatChanged);
            Assert.Empty(AffectedSummary.ProjectsWithChangedFiles);
        }

        [Fact]
        public async Task Staged_should_detect_staged_changes()
        {
            this._uncommitted = UncommittedChanges.Staged;

            var projectName = "InventoryManagement";
            Repository.CreateCsProject(projectName);

            this._fromCommit = Repository.StageAndCommit()
                .Sha;

            await this.Repository.CreateTextFileAsync(Path.Combine(projectName, "file.cs"), "// Changes");
            Repository.StageAll();

            var changed = Assert.Single(AffectedSummary.ProjectsWithChangedFiles);
            Assert.Equal(projectName, changed.GetProjectName());
        }

        [Fact]
        public async Task None_should_ignore_staged_changes()
        {
            this._uncommitted = UncommittedChanges.None;

            var projectName = "InventoryManagement";
            Repository.CreateCsProject(projectName);

            this._fromCommit = Repository.StageAndCommit()
                .Sha;

            await this.Repository.CreateTextFileAsync(Path.Combine(projectName, "file.cs"), "// Changes");
            Repository.StageAll();

            Assert.Empty(AffectedSummary.FilesThatChanged);
            Assert.Empty(AffectedSummary.ProjectsWithChangedFiles);
        }

        [Fact]
        public async Task None_should_detect_committed_changes()
        {
            this._uncommitted = UncommittedChanges.None;

            var projectName = "InventoryManagement";
            Repository.CreateCsProject(projectName);

            this._fromCommit = Repository.StageAndCommit()
                .Sha;

            await this.Repository.CreateTextFileAsync(Path.Combine(projectName, "file.cs"), "// Changes");
            Repository.StageAndCommit();

            var changed = Assert.Single(AffectedSummary.ProjectsWithChangedFiles);
            Assert.Equal(projectName, changed.GetProjectName());
        }

        /// <summary>
        /// Package versions are read from the same two revisions the file diff uses. Reading the
        /// current side from HEAD instead left an uncommitted version bump counted among the
        /// files that changed while no package change was reported for it.
        /// </summary>
        [Fact]
        public void All_should_detect_uncommitted_package_version_changes()
        {
            var packageName = "Some.Library";
            Repository.CreateDirectoryPackageProps(b => b.AddPackageVersion(packageName, "1.0.0"));

            var projectName = "InventoryManagement";
            Repository.CreateCsProject(projectName, b => b.AddNuGetDependency(packageName));

            this._fromCommit = Repository.StageAndCommit()
                .Sha;

            Repository.UpdateDirectoryPackageProps(b => b.AddPackageVersion(packageName, "2.0.0"));

            var changedPackage = Assert.Single(AffectedSummary.ChangedPackages);
            Assert.Equal(packageName, changedPackage.Name);
            Assert.Equal("1.0.0", changedPackage.OldVersions.Single());
            Assert.Equal("2.0.0", changedPackage.NewVersions.Single());
        }

        [Fact]
        public void None_should_ignore_uncommitted_package_version_changes()
        {
            this._uncommitted = UncommittedChanges.None;

            var packageName = "Some.Library";
            Repository.CreateDirectoryPackageProps(b => b.AddPackageVersion(packageName, "1.0.0"));

            var projectName = "InventoryManagement";
            Repository.CreateCsProject(projectName, b => b.AddNuGetDependency(packageName));

            this._fromCommit = Repository.StageAndCommit()
                .Sha;

            Repository.UpdateDirectoryPackageProps(b => b.AddPackageVersion(packageName, "2.0.0"));

            Assert.Empty(AffectedSummary.ChangedPackages);
        }
    }
}
