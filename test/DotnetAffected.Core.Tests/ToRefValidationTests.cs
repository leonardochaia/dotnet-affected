using DotnetAffected.Abstractions;
using DotnetAffected.Testing.Utils;
using LibGit2Sharp;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DotnetAffected.Core.Tests
{
    /// <summary>
    /// The project graph is built from the working tree, so a 'to' ref the working tree is not
    /// checked out at compares the files that changed up to one revision against the project
    /// structure of another. These cover it being refused rather than analysed.
    /// </summary>
    public class ToRefValidationTests
        : BaseDotnetAffectedTest
    {
        private string _fromCommit;
        private string _toCommit;

        protected override AffectedOptions Options =>
            new AffectedOptions(this.Repository.Path, fromRef: _fromCommit, toRef: _toCommit);

        [Fact]
        public async Task When_working_tree_is_behind_to_ref_should_throw()
        {
            var projectName = "InventoryManagement";
            Repository.CreateCsProject(projectName);

            this._fromCommit = Repository.StageAndCommit()
                .Sha;

            await this.Repository.CreateTextFileAsync(Path.Combine(projectName, "file.cs"), "// Changes");

            this._toCommit = Repository.StageAndCommit()
                .Sha;

            // Put the working tree back where it was, the way a checkout of the base branch would.
            Repository.Repository.Reset(ResetMode.Hard, this._fromCommit);

            var exception = Assert.Throws<ToRefNotAtHeadException>(() => _ = AffectedSummary);

            Assert.Equal(this._toCommit, exception.ToRef);
            Assert.Equal(this._fromCommit, exception.HeadSha);
        }

        /// <summary>
        /// The reproduction from https://github.com/leonardochaia/dotnet-affected/issues/162:
        /// the added project's file was counted among the files that changed while being reported
        /// under no project at all, silently, with exit code 0.
        /// </summary>
        [Fact]
        public void When_a_project_added_after_the_working_tree_should_throw()
        {
            Repository.CreateCsProject("Existing");

            this._fromCommit = Repository.StageAndCommit()
                .Sha;

            Repository.CreateCsProject("Added");

            this._toCommit = Repository.StageAndCommit()
                .Sha;

            Repository.Repository.Reset(ResetMode.Hard, this._fromCommit);

            Assert.Throws<ToRefNotAtHeadException>(() => _ = AffectedSummary);
        }

        [Fact]
        public async Task When_to_ref_is_head_should_determine_changes()
        {
            var projectName = "InventoryManagement";
            Repository.CreateCsProject(projectName);

            this._fromCommit = Repository.StageAndCommit()
                .Sha;

            await this.Repository.CreateTextFileAsync(Path.Combine(projectName, "file.cs"), "// Changes");

            this._toCommit = Repository.StageAndCommit()
                .Sha;

            var changed = Assert.Single(AffectedSummary.ProjectsWithChangedFiles);
            Assert.Equal(projectName, changed.GetProjectName());
        }

        /// <summary>
        /// The ref is resolved before being compared, so anything pointing at the checked out
        /// commit is accepted, not just its SHA.
        /// </summary>
        [Fact]
        public async Task When_to_ref_is_a_branch_at_head_should_determine_changes()
        {
            var projectName = "InventoryManagement";
            Repository.CreateCsProject(projectName);

            this._fromCommit = Repository.StageAndCommit()
                .Sha;

            await this.Repository.CreateTextFileAsync(Path.Combine(projectName, "file.cs"), "// Changes");

            Repository.StageAndCommit();

            this._toCommit = Repository.Repository.Head.FriendlyName;

            var changed = Assert.Single(AffectedSummary.ProjectsWithChangedFiles);
            Assert.Equal(projectName, changed.GetProjectName());
        }
    }
}
