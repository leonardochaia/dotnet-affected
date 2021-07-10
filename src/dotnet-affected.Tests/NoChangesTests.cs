using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Affected.Cli.Tests
{
    public class NoChangesTests : BaseDotnetAffectedCommandTest
    {
        public NoChangesTests(ITestOutputHelper helper) : base(helper)
        {
        }

        [Theory]
        [InlineData("")]
        [InlineData("changes")]
        [InlineData("generate")]
        public async Task When_nothing_has_changed_should_exit_with_NothingChanged_status_code(string command)
        {
            var projectName = "InventoryManagement";
            using var directory = CreateSingleProject(projectName);

            var (output, exitCode) = await this.InvokeAsync($"{command} -p {directory.Path}");

            Assert.Equal(AffectedExitCodes.NothingChanged, exitCode);

            RenderingAssertions.LineSequenceEquals(output,
                l => Assert.Contains("No affected projects where found for the current changes", l));
        }

        [Theory]
        [InlineData("")]
        [InlineData("changes")]
        [InlineData("generate")]
        public async Task Using_solution_when_nothing_has_changed_should_exit_with_NothingChanged_status_code(
            string command)
        {
            // Create a project
            var projectName = "InventoryManagement";
            using var directory = new TempWorkingDirectory();
            var projectPath = directory.MakePathForCsProj(projectName);

            CreateProject(projectPath, projectName)
                .Save();

            // Create a solution which includes the project
            var solutionPath = await directory.CreateSolutionFileForProjects("test-solution.sln", projectPath);

            var (output, exitCode) = await this.InvokeAsync($"{command} --solution-path {solutionPath}");

            Assert.Equal(AffectedExitCodes.NothingChanged, exitCode);

            Assert.Contains("No affected projects where found for the current changes", output);
        }

        [Theory]
        [InlineData("")]
        [InlineData("changes")]
        [InlineData("generate")]
        public async Task
            Using_solution_when_projects_outside_solution_has_changed_should_exit_with_NothingChanged_status_code(
                string command)
        {
            // Create a project
            var projectName = "InventoryManagement";
            using var directory = new TempWorkingDirectory();
            var projectPath = directory.MakePathForCsProj(projectName);

            CreateProject(projectPath, projectName)
                .Save();

            // Create a solution which includes the project
            var solutionPath = await directory.CreateSolutionFileForProjects("test-solution.sln", projectPath);

            // Create a project that is outside the solution
            var outsiderName = "OutsiderProject";
            var outsiderPath = directory.MakePathForCsProj(outsiderName);

            CreateProject(outsiderPath, outsiderName)
                .Save();

            // Fake changes for the outsider
            SetupChanges(directory.Path, outsiderPath);

            var (output, exitCode) = await this.InvokeAsync($"{command} --solution-path {solutionPath}");

            Assert.Equal(AffectedExitCodes.NothingChanged, exitCode);

            Assert.Contains("No affected projects where found for the current changes", output);
        }
    }
}
