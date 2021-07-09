using Moq;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using System.Threading.Tasks;

namespace Affected.Cli.Tests
{
    public class AffectedCommandTests
        : BaseDotnetAffectedCommandTest
    {
        public AffectedCommandTests(ITestOutputHelper helper) : base(helper)
        {
        }

        [Fact]
        public async Task When_changes_are_made_to_a_project_dependant_projects_are_affected()
        {
            // Create a project
            var projectName = "InventoryManagement";
            using var directory = new FileUtilities.TempWorkingDirectory();
            var projectPath = directory.CreateTemporaryCsProjFile();

            CreateProject(projectPath, projectName)
                .Save();

            // Create another project that depends on the first one
            var dependantProjectName = "InventoryManagement.Tests";
            var dependantProjectPath = directory.CreateTemporaryCsProjFile();

            CreateProject(dependantProjectPath, dependantProjectName)
                .AddDependency(projectPath)
                .Save();

            var (output, exitCode) = await InvokeAsync(
                $"--assume-changes {projectName} -p {directory.Path}");

            Assert.Equal(0, exitCode);

            RenderingAssertions.LineSequenceEquals(output,
                l => Assert.Contains("Files inside these projects have changed:", l),
                l => Assert.Contains(projectName, l),
                l => Assert.Contains("These projects are affected by those changes", l),
                l => Assert.Contains(dependantProjectName, l));
        }

        [Fact]
        public async Task When_any_project_has_changes_should_exit_successfully()
        {
            var projectName = "InventoryManagement";
            using var directory = CreateSingleProject(projectName);

            var (output, exitCode) = await this.InvokeAsync($"--assume-changes {projectName} -p {directory.Path}");

            Assert.Equal(0, exitCode);

            RenderingAssertions.LineSequenceEquals(output,
                l => Assert.Contains("Files inside these projects have changed:", l),
                l => Assert.Contains(projectName, l),
                l => Assert.Contains("No affected projects where found for the current changes", l));
        }

        [Fact]
        public async Task When_nothing_has_changed_should_exit_with_NothingChanged_status_code()
        {
            var projectName = "InventoryManagement";
            using var directory = CreateSingleProject(projectName);

            SetupChanges(directory.Path, Enumerable.Empty<string>());

            var (output, exitCode) = await this.InvokeAsync($"-p {directory.Path}");

            Assert.Equal(AffectedExitCodes.NothingChanged, exitCode);

            RenderingAssertions.LineSequenceEquals(output,
                l => Assert.Contains("No affected projects where found for the current changes", l));
        }
    }
}
