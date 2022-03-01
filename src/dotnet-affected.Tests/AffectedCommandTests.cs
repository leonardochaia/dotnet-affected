using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Affected.Cli.Tests
{
    /// <summary>
    /// Tests for the public API.
    /// These tests ensures that when using the CLI, output is generated correctly.
    /// They should not cover any domain/business logic, but input/output instead.
    /// </summary>
    public class AffectedCommandTests : BaseInvocationTest
    {
        public AffectedCommandTests(ITestOutputHelper helper) : base(helper)
        {
        }

        [Fact]
        public async Task When_any_changes_should_output_traversal_sdk()
        {
            // Create a project
            var projectName = "InventoryManagement";
            var msBuildProject = this.Repository.CreateCsProject(projectName);

            var (output, exitCode) =
                await this.InvokeAsync($"-p {Repository.Path} --dry-run");

            Assert.Equal(0, exitCode);
            
            Assert.Contains($"WRITE {Path.Combine(Repository.Path, "affected.proj")}", output);
            Assert.Contains($"Microsoft.Build.Traversal", output);
            Assert.Contains($"Include=\"{msBuildProject.FullPath}\"", output);
        }

        [Fact]
        public async Task When_any_changes_using_text_formatter_should_output_text()
        {
            // Create a project
            var projectName = "InventoryManagement";
            var msBuildProject = this.Repository.CreateCsProject(projectName);

            var (output, exitCode) =
                await this.InvokeAsync($"-p {Repository.Path} --dry-run -f text");

            Assert.Equal(0, exitCode);

            Assert.Contains($"WRITE {Path.Combine(Repository.Path, "affected.txt")}", output);
            Assert.Contains(msBuildProject.FullPath, output);
        }
        
        [Fact]
        public async Task When_any_changes_and_verbosity_should_output_changed_and_affected_projects()
        {
            // Create a project
            var projectName = "InventoryManagement";
            this.Repository.CreateCsProject(projectName);

            var (output, exitCode) =
                await this.InvokeAsync($"-p {Repository.Path} -f text --verbose");

            Assert.Equal(0, exitCode);

            Assert.Contains($"WRITE: {Path.Combine(Repository.Path, "affected.txt")}", output);
            Assert.Contains(projectName, output);
            Assert.Contains("No projects where affected by any of the changed projects.", output);
        }

        [Fact]
        public async Task When_any_changes_using_multiple_formatter_should_output()
        {
            // Create a project
            var projectName = "InventoryManagement";
            this.Repository.CreateCsProject(projectName);

            var (output, exitCode) =
                await this.InvokeAsync($"-p {Repository.Path} --dry-run -f traversal text");

            Assert.Equal(0, exitCode);

            Assert.Contains($"WRITE {Path.Combine(Repository.Path, "affected.txt")}", output);
            Assert.Contains($"WRITE {Path.Combine(Repository.Path, "affected.proj")}", output);
        }

        [Fact]
        public async Task When_no_changes_should_exit_with_code()
        {
            // Create a project
            var projectName = "InventoryManagement";
            this.Repository.CreateCsProject(projectName);

            // Commit so there are no changes.
            this.Repository.StageAndCommit();

            var (output, exitCode) =
                await this.InvokeAsync($"-p {Repository.Path} --dry-run -f text");

            Assert.Equal(AffectedExitCodes.NothingChanged, exitCode);

            Assert.Contains($"No affected projects where found for the current changes", output);
        }
    }
}
