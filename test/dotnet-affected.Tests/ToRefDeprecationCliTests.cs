using DotnetAffected.Testing.Utils;
using LibGit2Sharp;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Affected.Cli.Tests
{
    /// <summary>
    /// --to is kept for compatibility but only names the commit the working tree is already
    /// checked out at. These cover it being announced as deprecated and refused otherwise.
    /// </summary>
    public class ToRefDeprecationCliTests : BaseInvocationTest
    {
        public ToRefDeprecationCliTests(ITestOutputHelper helper)
            : base(helper)
        {
        }

        private string Error => Terminal.Error.ToString();

        [Fact]
        public async Task When_to_ref_is_head_should_succeed_and_warn()
        {
            var projectName = "InventoryManagement";
            this.Repository.CreateCsProject(projectName);

            var fromCommit = Repository.StageAndCommit()
                .Sha;

            await this.Repository.CreateTextFileAsync(Path.Combine(projectName, "file.cs"), "// Changes");

            var toCommit = Repository.StageAndCommit()
                .Sha;

            var (_, exitCode) =
                await this.InvokeAsync($"-p {Repository.Path} --from {fromCommit} --to {toCommit} --dry-run");

            Assert.Equal(0, exitCode);
            Assert.Contains("--to is deprecated", Error);
            Assert.Contains("v8", Error);
        }

        [Fact]
        public async Task When_to_ref_is_not_head_should_fail()
        {
            var projectName = "InventoryManagement";
            this.Repository.CreateCsProject(projectName);

            var fromCommit = Repository.StageAndCommit()
                .Sha;

            await this.Repository.CreateTextFileAsync(Path.Combine(projectName, "file.cs"), "// Changes");

            var toCommit = Repository.StageAndCommit()
                .Sha;

            // Put the working tree back where it was, the way a checkout of the base branch would.
            Repository.Repository.Reset(ResetMode.Hard, fromCommit);

            var (_, exitCode) =
                await this.InvokeAsync($"-p {Repository.Path} --from {fromCommit} --to {toCommit} --dry-run");

            Assert.Equal(AffectedExitCodes.Failure, exitCode);
            Assert.Contains("the working tree is checked out at", Error);
        }

        [Fact]
        public async Task When_no_to_ref_should_not_warn()
        {
            var projectName = "InventoryManagement";
            this.Repository.CreateCsProject(projectName);

            var (_, exitCode) = await this.InvokeAsync($"-p {Repository.Path} --dry-run");

            Assert.Equal(0, exitCode);
            Assert.DoesNotContain("deprecated", Error);
        }
    }
}
