using DotnetAffected.Abstractions;
using DotnetAffected.Testing.Utils;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace DotnetAffected.Core.Tests
{
    /// <summary>
    /// Detection while running from inside a linked git worktree.
    ///
    /// A worktree splits the working directory from the git directory, so anything deriving one
    /// from the other breaks here and nowhere else. Every other test in the suite runs against an
    /// ordinary clone, which leaves the split entirely uncovered.
    /// </summary>
    public class GitWorktreeDetectionTests : BaseRepositoryTest
    {
        private const string ProjectName = "InventoryManagement";

        private static AffectedSummary Execute(string repositoryPath, string fromRef = null)
            => new AffectedExecutor(new AffectedOptions(repositoryPath, fromRef: fromRef)).Execute();

        /// <summary>
        /// Paths must resolve inside the worktree, not inside the repository it was linked from.
        /// </summary>
        [Fact]
        public async Task When_running_inside_a_worktree_changed_project_should_be_detected()
        {
            this.Repository.CreateCsProject(ProjectName);
            await this.Repository.CreateTextFileAsync(
                Path.Combine(ProjectName, "Keep.cs"), "public class Keep {}");
            this.Repository.StageAndCommit();

            using var worktree = new TemporaryWorktree(this.Repository);

            await File.WriteAllTextAsync(
                Path.Combine(worktree.Path, ProjectName, "Keep.cs"), "// changed inside the worktree");

            var summary = Execute(worktree.Path);

            var changed = Assert.Single(summary.ProjectsWithChangedFiles);
            Assert.Equal(
                Path.Combine(worktree.Path, ProjectName, $"{ProjectName}.csproj"),
                changed.GetFullPath());
        }

        /// <summary>
        /// Restoring a deleted file reads its content back out of the commit and matches it against
        /// the working directory, which is exactly the pair a worktree separates.
        /// </summary>
        [Fact]
        public async Task When_running_inside_a_worktree_deleted_file_should_be_attributed()
        {
            this.Repository.CreateCsProject(ProjectName);
            await this.Repository.CreateTextFileAsync(
                Path.Combine(ProjectName, "Gone.cs"), "public class Gone {}");
            await this.Repository.CreateTextFileAsync(
                Path.Combine(ProjectName, "Keep.cs"), "public class Keep {}");
            this.Repository.StageAndCommit();

            using var worktree = new TemporaryWorktree(this.Repository);

            File.Delete(Path.Combine(worktree.Path, ProjectName, "Gone.cs"));

            var summary = Execute(worktree.Path);

            Assert.Single(summary.FilesThatChanged);

            var changed = Assert.Single(summary.ProjectsWithChangedFiles);
            Assert.Equal(
                Path.Combine(worktree.Path, ProjectName, $"{ProjectName}.csproj"),
                changed.GetFullPath());
        }

        /// <summary>
        /// Comparing against a ref resolves commits through the repository the worktree is linked
        /// to, while the files being compared live in the worktree.
        /// </summary>
        [Fact]
        public async Task When_running_inside_a_worktree_changes_between_commits_should_be_detected()
        {
            this.Repository.CreateCsProject(ProjectName);
            var baseCommit = this.Repository.StageAndCommit();

            using var worktree = new TemporaryWorktree(this.Repository);

            await File.WriteAllTextAsync(
                Path.Combine(worktree.Path, ProjectName, "Added.cs"), "public class Added {}");
            worktree.StageAndCommit();

            var summary = Execute(worktree.Path, fromRef: baseCommit.Sha);

            var changed = Assert.Single(summary.ProjectsWithChangedFiles);
            Assert.Equal(
                Path.Combine(worktree.Path, ProjectName, $"{ProjectName}.csproj"),
                changed.GetFullPath());
        }

        /// <summary>
        /// Changes made in the repository the worktree came from are not the worktree's changes.
        /// </summary>
        [Fact]
        public async Task When_running_inside_a_worktree_changes_outside_it_should_be_ignored()
        {
            this.Repository.CreateCsProject(ProjectName);
            await this.Repository.CreateTextFileAsync(
                Path.Combine(ProjectName, "Keep.cs"), "public class Keep {}");
            this.Repository.StageAndCommit();

            using var worktree = new TemporaryWorktree(this.Repository);

            // Touch the original checkout only.
            await this.Repository.CreateTextFileAsync(
                Path.Combine(ProjectName, "Keep.cs"), "// changed outside the worktree");

            var summary = Execute(worktree.Path);

            Assert.Empty(summary.FilesThatChanged);
            Assert.Empty(summary.ProjectsWithChangedFiles);
        }
    }
}
