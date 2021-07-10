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
        public async Task When_changes_are_made_to_a_project_dependant_projects_should_be_affected()
        {
            // Create a project
            var projectName = "InventoryManagement";
            using var directory = new TempWorkingDirectory();
            var projectPath = directory.MakePathForCsProj(projectName);

            CreateProject(projectPath, projectName)
                .Save();

            // Create another project that depends on the first one
            var dependantProjectName = "InventoryManagement.Tests";
            var dependantProjectPath = directory.MakePathForCsProj(dependantProjectName);

            CreateProject(dependantProjectPath, dependantProjectName)
                .AddDependency(projectPath)
                .Save();

            // Fake changes to first project's csproj file.
            SetupChanges(directory.Path, projectPath);

            var (output, exitCode) = await InvokeAsync($"-p {directory.Path}");

            Assert.Equal(0, exitCode);

            RenderingAssertions.LineSequenceEquals(output,
                l => Assert.Contains("Files inside these projects have changed:", l),
                l => Assert.Contains(projectName, l),
                l => Assert.Contains("These projects are affected by those changes", l),
                l => Assert.Contains(dependantProjectName, l));
        }

        [Fact]
        public async Task When_recursively_changes_are_made_to_a_project_dependant_projects_should_be_affected()
        {
            using var directory = new TempWorkingDirectory();

            // Create a shared project
            var sharedProjectName = "InventoryManagement.Domain";
            var sharedProjectPath = directory.MakePathForCsProj(sharedProjectName);

            CreateProject(sharedProjectPath, sharedProjectName)
                .Save();

            // Create a project
            var projectName = "InventoryManagement";
            var projectPath = directory.MakePathForCsProj(projectName);

            CreateProject(projectPath, projectName)
                .AddDependency(sharedProjectPath)
                .Save();

            // Create another project that depends on the first one
            var dependantProjectName = "InventoryManagement.Tests";
            var dependantProjectPath = directory.MakePathForCsProj(dependantProjectName);

            CreateProject(dependantProjectPath, dependantProjectName)
                .AddDependency(projectPath)
                .Save();

            // Fake changes to first project's csproj file.
            SetupChanges(directory.Path, sharedProjectPath);

            var (output, exitCode) = await InvokeAsync($"-p {directory.Path}");

            Assert.Equal(0, exitCode);

            RenderingAssertions.LineSequenceEquals(output,
                l => Assert.Contains("Files inside these projects have changed:", l),
                l => Assert.Contains(sharedProjectName, l),
                l => Assert.Contains("These projects are affected by those changes", l),
                l => Assert.Contains(projectName, l),
                l => Assert.Contains(dependantProjectName, l)
            );
        }

        [Fact]
        public async Task When_a_project_has_changes_and_nothing_is_affected_should_exit_successfully()
        {
            // Create a project
            var projectName = "InventoryManagement";
            using var directory = new TempWorkingDirectory();
            var projectPath = directory.MakePathForCsProj(projectName);

            CreateProject(projectPath, projectName)
                .Save();

            // Fake changes to it's project's csproj file.
            SetupChanges(directory.Path, projectPath);

            var (output, exitCode) = await this.InvokeAsync($"-p {directory.Path}");

            Assert.Equal(0, exitCode);

            RenderingAssertions.LineSequenceEquals(output,
                l => Assert.Contains("Files inside these projects have changed:", l),
                l => Assert.Contains(projectName, l),
                l => Assert.Contains("No affected projects where found for the current changes", l));
        }
    }
}
