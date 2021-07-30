using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Affected.Cli.Tests
{
    public class AffectedCommandTests : BaseDotnetAffectedCommandTest
    {
        public AffectedCommandTests(ITestOutputHelper helper) : base(helper)
        {
        }

        [Fact]
        public async Task When_any_changes_should_output_traversal_sdk()
        {
            // Create a project
            var projectName = "InventoryManagement";
            using var directory = new TempWorkingDirectory();
            var projectPath = directory.MakePathForCsProj(projectName);

            CreateProject(projectPath, projectName)
                .Save();

            // Fake changes to it's project's csproj file.
            SetupChanges(directory.Path, projectPath);

            var (output, exitCode) =
                await this.InvokeAsync($"-p {directory.Path} --dry-run");

            Assert.Equal(0, exitCode);

            Assert.Contains($"WRITE {directory.Path}/affected.proj", output);
            Assert.Contains($"Microsoft.Build.Traversal", output);
            Assert.Contains($"Include=\"{projectPath}\"", output);
        }

        [Fact]
        public async Task When_any_changes_using_text_formatter_should_output_text()
        {
            // Create a project
            var projectName = "InventoryManagement";
            using var directory = new TempWorkingDirectory();
            var projectPath = directory.MakePathForCsProj(projectName);

            CreateProject(projectPath, projectName)
                .Save();

            // Fake changes to it's project's csproj file.
            SetupChanges(directory.Path, projectPath);

            var (output, exitCode) =
                await this.InvokeAsync($"-p {directory.Path} --dry-run -f text");

            Assert.Equal(0, exitCode);

            Assert.Contains($"WRITE {directory.Path}/affected.txt", output);
            Assert.Contains(projectPath, output);
        }

        [Fact]
        public async Task When_any_changes_using_multiple_formatter_should_output()
        {
            // Create a project
            var projectName = "InventoryManagement";
            using var directory = new TempWorkingDirectory();
            var projectPath = directory.MakePathForCsProj(projectName);

            CreateProject(projectPath, projectName)
                .Save();

            // Fake changes to it's project's csproj file.
            SetupChanges(directory.Path, projectPath);

            var (output, exitCode) =
                await this.InvokeAsync($"-p {directory.Path} --dry-run -f traversal text");

            Assert.Equal(0, exitCode);

            Assert.Contains($"WRITE {directory.Path}/affected.txt", output);
            Assert.Contains($"WRITE {directory.Path}/affected.proj", output);
        }

        [Fact]
        public async Task When_no_changes_should_exit_with_code()
        {
            // Create a project
            var projectName = "InventoryManagement";
            using var directory = new TempWorkingDirectory();
            var projectPath = directory.MakePathForCsProj(projectName);

            CreateProject(projectPath, projectName)
                .Save();

            var (output, exitCode) =
                await this.InvokeAsync($"-p {directory.Path} --dry-run -f text");

            Assert.Equal(AffectedExitCodes.NothingChanged, exitCode);

            Assert.Contains($"No affected projects where found for the current changes", output);
        }
    }
}
