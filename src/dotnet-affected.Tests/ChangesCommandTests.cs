using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Affected.Cli.Tests
{
    public class ChangesCommandTests
        : BaseDotnetAffectedCommandTest
    {
        public ChangesCommandTests(ITestOutputHelper helper) : base(helper)
        {
        }

        [Fact]
        public async Task When_nothing_has_changed_should_exit_with_NothingChanged_status_code()
        {
            var projectName = "InventoryManagement";
            using var directory = CreateSingleProject(projectName);

            var (output, exitCode) = await this.InvokeAsync($"changes -p {directory.Path}");

            Assert.Equal(AffectedExitCodes.NothingChanged, exitCode);

            RenderingAssertions.LineSequenceEquals(output,
                l => Assert.Contains("No affected projects where found for the current changes", l));
        }

        [Fact]
        public async Task When_has_changes_should_print_and_exit_successfully()
        {
            // Create a project
            var projectName = "InventoryManagement";
            using var directory = new TempWorkingDirectory();
            var projectPath = directory.MakePathForCsProj(projectName);

            CreateProject(projectPath, projectName)
                .Save();

            // Fake changes on the project's file
            SetupChanges(directory.Path, projectPath);

            var (output, exitCode) =
                await this.InvokeAsync($"changes -p {directory.Path}");

            Assert.Equal(0, exitCode);
            Assert.Contains("Files inside these projects have changed", output);
            Assert.Contains(projectName, output);
        }
    }
}
