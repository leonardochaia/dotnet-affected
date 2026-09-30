using DotnetAffected.Abstractions;
using DotnetAffected.Testing.Utils;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace DotnetAffected.Core.Tests
{
    /// <summary>
    /// Detection in a repository that has a worktree linked with relative paths.
    ///
    /// Since git 2.48, <c>git worktree add --relative-paths</c> (or <c>worktree.useRelativePaths</c>)
    /// writes <c>extensions.relativeWorktrees = true</c> into the repository's config. Opening the
    /// repository used to fail with "unsupported extension name extensions.relativeworktrees",
    /// both from the worktree and from the original checkout.
    /// </summary>
    public class GitRelativeWorktreeDetectionTests : BaseRepositoryTest
    {
        private const string ProjectName = "InventoryManagement";

        private static AffectedSummary Execute(string repositoryPath, string fromRef = null)
            => new AffectedExecutor(new AffectedOptions(repositoryPath, fromRef: fromRef)).Execute();

        /// <summary>
        /// The extension lands in the shared config, so the original checkout is affected too.
        /// </summary>
        [Fact]
        public async Task When_a_relative_worktree_exists_changes_in_the_repository_should_be_detected()
        {
            this.Repository.CreateCsProject(ProjectName);
            await this.Repository.CreateTextFileAsync(
                Path.Combine(ProjectName, "Keep.cs"), "public class Keep {}");
            this.Repository.StageAndCommit();

            using var worktree = new TemporaryRelativeWorktree(this.Repository);

            await this.Repository.CreateTextFileAsync(
                Path.Combine(ProjectName, "Keep.cs"), "// changed in the repository");

            var summary = Execute(this.Repository.Path);

            var changed = Assert.Single(summary.ProjectsWithChangedFiles);
            Assert.Equal(
                Path.Combine(this.Repository.Path, ProjectName, $"{ProjectName}.csproj"),
                changed.GetFullPath());
        }

        /// <summary>
        /// Paths must resolve inside the worktree, which is only reachable through its relative
        /// <c>.git</c> file.
        /// </summary>
        [Fact]
        public async Task When_running_inside_a_relative_worktree_changed_project_should_be_detected()
        {
            this.Repository.CreateCsProject(ProjectName);
            await this.Repository.CreateTextFileAsync(
                Path.Combine(ProjectName, "Keep.cs"), "public class Keep {}");
            this.Repository.StageAndCommit();

            using var worktree = new TemporaryRelativeWorktree(this.Repository);

            await File.WriteAllTextAsync(
                Path.Combine(worktree.Path, ProjectName, "Keep.cs"), "// changed inside the worktree");

            var summary = Execute(worktree.Path);

            var file = Assert.Single(summary.FilesThatChanged);
            Assert.Equal(Path.Combine(worktree.Path, ProjectName, "Keep.cs"), file);

            var changed = Assert.Single(summary.ProjectsWithChangedFiles);
            Assert.Equal(
                Path.Combine(worktree.Path, ProjectName, $"{ProjectName}.csproj"),
                changed.GetFullPath());
        }

        /// <summary>
        /// Restoring a deleted file reads its content out of the object database, which the
        /// worktree reaches through the repository it is linked to.
        /// </summary>
        [Fact]
        public async Task When_running_inside_a_relative_worktree_deleted_file_should_be_attributed()
        {
            this.Repository.CreateCsProject(ProjectName);
            await this.Repository.CreateTextFileAsync(
                Path.Combine(ProjectName, "Gone.cs"), "public class Gone {}");
            await this.Repository.CreateTextFileAsync(
                Path.Combine(ProjectName, "Keep.cs"), "public class Keep {}");
            this.Repository.StageAndCommit();

            using var worktree = new TemporaryRelativeWorktree(this.Repository);

            File.Delete(Path.Combine(worktree.Path, ProjectName, "Gone.cs"));

            var summary = Execute(worktree.Path);

            Assert.Single(summary.FilesThatChanged);

            var changed = Assert.Single(summary.ProjectsWithChangedFiles);
            Assert.Equal(
                Path.Combine(worktree.Path, ProjectName, $"{ProjectName}.csproj"),
                changed.GetFullPath());
        }

        /// <summary>
        /// Commits made in the worktree land on its own branch and HEAD, not the repository's.
        /// </summary>
        [Fact]
        public async Task When_running_inside_a_relative_worktree_changes_between_commits_should_be_detected()
        {
            this.Repository.CreateCsProject(ProjectName);
            var baseCommit = this.Repository.StageAndCommit();

            using var worktree = new TemporaryRelativeWorktree(this.Repository);

            await File.WriteAllTextAsync(
                Path.Combine(worktree.Path, ProjectName, "Added.cs"), "public class Added {}");
            worktree.StageAndCommit();

            var summary = Execute(worktree.Path, fromRef: baseCommit.Sha);

            var file = Assert.Single(summary.FilesThatChanged);
            Assert.Equal(Path.Combine(worktree.Path, ProjectName, "Added.cs"), file);

            var changed = Assert.Single(summary.ProjectsWithChangedFiles);
            Assert.Equal(
                Path.Combine(worktree.Path, ProjectName, $"{ProjectName}.csproj"),
                changed.GetFullPath());
        }

        /// <summary>
        /// Changes made in the repository the worktree came from are not the worktree's changes.
        /// </summary>
        [Fact]
        public async Task When_running_inside_a_relative_worktree_changes_outside_it_should_be_ignored()
        {
            this.Repository.CreateCsProject(ProjectName);
            await this.Repository.CreateTextFileAsync(
                Path.Combine(ProjectName, "Keep.cs"), "public class Keep {}");
            this.Repository.StageAndCommit();

            using var worktree = new TemporaryRelativeWorktree(this.Repository);

            // Touch the original checkout only.
            await this.Repository.CreateTextFileAsync(
                Path.Combine(ProjectName, "Keep.cs"), "// changed outside the worktree");

            var summary = Execute(worktree.Path);

            Assert.Empty(summary.FilesThatChanged);
            Assert.Empty(summary.ProjectsWithChangedFiles);
        }
    }
}
