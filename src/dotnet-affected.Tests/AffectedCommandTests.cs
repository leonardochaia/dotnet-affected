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
            var imPath = directory.GetTemporaryCsProjFile();

            CreateProject(imPath, projectName)
                .Save();

            // Create another project that depends on the first one
            var testProjectName = "InventoryManagement.Tests";
            var imTestPath = directory.GetTemporaryCsProjFile();

            CreateProject(imTestPath, testProjectName)
                .AddDependency(imPath)
                .Save();

            var (output, exitCode) = await InvokeAsync(
                $"-v --assume-changes {projectName} -p {directory.Path}");

            Assert.Equal(0, exitCode);
            Assert.Contains("These projects are affected by those changes", output);
            Assert.Contains(testProjectName, output);
        }

        [Fact]
        public async Task When_no_projects_are_affected_by_a_change_should_exit_with_custom_status_code()
        {
            var projectName = "InventoryManagement";
            using var directory = CreateSingleProject(projectName);

            var (output, exitCode) = await this.InvokeAsync($"-v --assume-changes {projectName} -p {directory.Path}");

            Assert.Equal(AffectedExitCodes.NothingAffected, exitCode);

            Assert.Contains("No affected projects where found for the current changes", output);
        }
    }
}
