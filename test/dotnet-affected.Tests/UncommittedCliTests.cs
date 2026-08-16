using DotnetAffected.Testing.Utils;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Affected.Cli.Tests
{
    public class UncommittedCliTests : BaseInvocationTest
    {
        public UncommittedCliTests(ITestOutputHelper helper)
            : base(helper)
        {
        }

        [Fact]
        public async Task When_uncommitted_is_none_should_ignore_the_working_tree()
        {
            var projectName = "InventoryManagement";
            this.Repository.CreateCsProject(projectName);

            Repository.StageAndCommit();

            await this.Repository.CreateTextFileAsync(Path.Combine(projectName, "file.cs"), "// Changes");

            var (_, exitCode) =
                await this.InvokeAsync($"-p {Repository.Path} --uncommitted none --dry-run");

            Assert.Equal(AffectedExitCodes.NothingChanged, exitCode);
        }

        [Fact]
        public async Task When_uncommitted_is_staged_should_ignore_unstaged_changes()
        {
            var projectName = "InventoryManagement";
            this.Repository.CreateCsProject(projectName);

            Repository.StageAndCommit();

            await this.Repository.CreateTextFileAsync(Path.Combine(projectName, "file.cs"), "// Changes");

            var (_, exitCode) =
                await this.InvokeAsync($"-p {Repository.Path} --uncommitted staged --dry-run");

            Assert.Equal(AffectedExitCodes.NothingChanged, exitCode);
        }

        [Fact]
        public async Task When_uncommitted_is_staged_should_detect_staged_changes()
        {
            var projectName = "InventoryManagement";
            var msBuildProject = this.Repository.CreateCsProject(projectName);

            Repository.StageAndCommit();

            await this.Repository.CreateTextFileAsync(Path.Combine(projectName, "file.cs"), "// Changes");
            Repository.StageAll();

            var (output, exitCode) =
                await this.InvokeAsync($"-p {Repository.Path} --uncommitted staged --dry-run");

            Assert.Equal(0, exitCode);
            Assert.Contains(msBuildProject.FullPath, output);
        }

        /// <summary>
        /// The invocation the README recommends for CI: a baseline, and a result that depends on
        /// the commits alone.
        /// </summary>
        [Fact]
        public async Task When_from_is_given_with_uncommitted_none_should_compare_commits()
        {
            var projectName = "InventoryManagement";
            var msBuildProject = this.Repository.CreateCsProject(projectName);

            var fromCommit = Repository.StageAndCommit()
                .Sha;

            await this.Repository.CreateTextFileAsync(Path.Combine(projectName, "file.cs"), "// Changes");
            Repository.StageAndCommit();

            // Written after the baseline commit, the way a generator running earlier in the
            // pipeline would. It must not decide anything.
            await this.Repository.CreateTextFileAsync("Untracked.cs", "// Not committed");

            var (output, exitCode) =
                await this.InvokeAsync($"-p {Repository.Path} --from {fromCommit} --uncommitted none --dry-run");

            Assert.Equal(0, exitCode);
            Assert.Contains(msBuildProject.FullPath, output);
        }

        [Fact]
        public async Task When_uncommitted_is_not_a_known_value_should_fail()
        {
            this.Repository.CreateCsProject("InventoryManagement");

            var (_, exitCode) =
                await this.InvokeAsync($"-p {Repository.Path} --uncommitted sometimes --dry-run");

            Assert.NotEqual(0, exitCode);
        }
    }
}
