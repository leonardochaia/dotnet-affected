using DotnetAffected.Testing.Utils;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Affected.Cli.Tests
{
    /// <summary>
    /// The CLI run from inside a linked git worktree.
    ///
    /// Covered separately from the library tests on purpose: the frontends and the library reached
    /// the project graph by different routes, and that is precisely how deleted files stayed broken
    /// for the CLI while the library suite was green. See issue #84.
    /// </summary>
    public class GitWorktreeCliTests : BaseInvocationTest
    {
        private const string ProjectName = "InventoryManagement";

        public GitWorktreeCliTests(ITestOutputHelper helper)
            : base(helper)
        {
        }

        [Fact]
        public async Task When_running_inside_a_worktree_changed_project_should_be_reported()
        {
            this.Repository.CreateCsProject(ProjectName);
            await this.Repository.CreateTextFileAsync(
                Path.Combine(ProjectName, "Keep.cs"), "public class Keep {}");
            this.Repository.StageAndCommit();

            using var worktree = new TemporaryWorktree(this.Repository);

            await File.WriteAllTextAsync(
                Path.Combine(worktree.Path, ProjectName, "Keep.cs"), "// changed inside the worktree");

            var (output, exitCode) = await this.InvokeAsync($"-p {worktree.Path} --dry-run -f text");

            Assert.Equal(0, exitCode);
            Assert.Contains(
                Path.Combine(worktree.Path, ProjectName, $"{ProjectName}.csproj"),
                output);
        }

        /// <summary>
        /// Deleting a file is attributed through the overlay, which reads the file back out of the
        /// commit. Doing that from a worktree exercises the overlay and the split git directory at
        /// the same time, which is the combination a monorepo CI actually runs.
        /// </summary>
        [Fact]
        public async Task When_running_inside_a_worktree_deleted_file_should_be_reported()
        {
            this.Repository.CreateCsProject(ProjectName);
            await this.Repository.CreateTextFileAsync(
                Path.Combine(ProjectName, "Gone.cs"), "public class Gone {}");
            await this.Repository.CreateTextFileAsync(
                Path.Combine(ProjectName, "Keep.cs"), "public class Keep {}");
            this.Repository.StageAndCommit();

            using var worktree = new TemporaryWorktree(this.Repository);

            File.Delete(Path.Combine(worktree.Path, ProjectName, "Gone.cs"));

            var (output, exitCode) = await this.InvokeAsync($"-p {worktree.Path} --dry-run -f text");

            Assert.Equal(0, exitCode);
            Assert.Contains(
                Path.Combine(worktree.Path, ProjectName, $"{ProjectName}.csproj"),
                output);
        }
    }
}
