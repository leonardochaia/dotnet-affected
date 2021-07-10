using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Affected.Cli.Tests
{
    public class ChangeDetectionTests
        : BaseDotnetAffectedCommandTest
    {
        public ChangeDetectionTests(ITestOutputHelper helper) : base(helper)
        {
        }

        [Theory]
        [InlineData("")]
        [InlineData("changes")]
        public async Task Using_change_provider_when_has_changes_should_print_and_exit_successfully(string command)
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
                await this.InvokeAsync($"{command} -p {directory.Path}");

            Assert.Equal(0, exitCode);
            Assert.Contains("Files inside these projects have changed", output);
            Assert.Contains(projectName, output);
        }

        [Theory]
        [InlineData("")]
        [InlineData("changes")]
        public async Task
            Using_change_provider_when_has_changes_to_file_inside_project_directory_should_print_and_exit_successfully(
                string command)
        {
            // Create a project
            var projectName = "InventoryManagement";
            using var directory = new TempWorkingDirectory();
            var projectPath = directory.MakePathForCsProj(projectName);

            CreateProject(projectPath, projectName)
                .Save();

            // Fake changes on the project's file
            var projectDirectory = Path.GetDirectoryName(projectPath);
            SetupChanges(directory.Path, Path.Combine(projectDirectory!, "some/random/file.cs"));

            var (output, exitCode) =
                await this.InvokeAsync($"{command} -p {directory.Path}");

            Assert.Equal(0, exitCode);
            Assert.Contains("Files inside these projects have changed", output);
            Assert.Contains(projectName, output);
        }

        [Theory]
        [InlineData("")]
        [InlineData("changes")]
        public async Task Using_assume_changes_when_has_changes_should_print_and_exit_successfully(string command)
        {
            // Create a project
            var projectName = "InventoryManagement";
            using var directory = new TempWorkingDirectory();
            var projectPath = directory.MakePathForCsProj(projectName);

            CreateProject(projectPath, projectName)
                .Save();

            var (output, exitCode) =
                await this.InvokeAsync($"{command} -p {directory.Path} --assume-changes {projectName}");

            Assert.Equal(0, exitCode);
            Assert.Contains("Files inside these projects have changed", output);
            Assert.Contains(projectName, output);
        }

        [Theory]
        [InlineData("")]
        [InlineData("changes")]
        public async Task Using_assume_changes_should_ignore_other_changes_print_and_exit_successfully(string command)
        {
            // Create a project
            var projectName = "InventoryManagement";
            using var directory = new TempWorkingDirectory();
            var projectPath = directory.MakePathForCsProj(projectName);

            CreateProject(projectPath, projectName)
                .Save();

            // Create a second project
            var otherName = "OtherProjectWhichHasChanges";
            var otherPath = directory.MakePathForCsProj(otherName);

            CreateProject(otherPath, otherName)
                .Save();

            // Fake changes for the second project
            SetupChanges(directory.Path, otherPath);

            var (output, exitCode) =
                await this.InvokeAsync($"{command} -p {directory.Path} --assume-changes {projectName}");

            Assert.Equal(0, exitCode);
            Assert.Contains("Files inside these projects have changed", output);
            Assert.Contains(projectName, output);
            Assert.DoesNotContain(otherName, output);
        }

        [Theory]
        [InlineData("")]
        [InlineData("changes")]
        public async Task Using_solution_when_has_changes_should_print_and_exit_successfully(string command)
        {
            // Create a project
            var projectName = "InventoryManagement";
            using var directory = new TempWorkingDirectory();
            var projectPath = directory.MakePathForCsProj(projectName);

            CreateProject(projectPath, projectName)
                .Save();

            // Fake changes on the project's file
            SetupChanges(directory.Path, projectPath);

            // Create a solution which includes the project
            var solutionPath = await directory.CreateSolutionFileForProjects("test-solution.sln", projectPath);

            var (output, exitCode) =
                await this.InvokeAsync($"{command} --solution-path {solutionPath}");

            Assert.Equal(0, exitCode);
            Assert.Contains("Files inside these projects have changed", output);
            Assert.Contains(projectName, output);
        }

        [Theory]
        [InlineData("")]
        [InlineData("changes")]
        public async Task Using_solution_should_ignore_changes_outside_solution_and_print_and_exit_successfully(
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

            // Create a second project
            var otherName = "OtherProjectWhichHasChanges";
            var otherPath = directory.MakePathForCsProj(otherName);

            CreateProject(otherPath, otherName)
                .Save();

            // Fake changes for the both projects
            SetupChanges(directory.Path, projectPath, otherPath);

            var (output, exitCode) =
                await this.InvokeAsync($"{command} --solution-path {solutionPath}");

            Assert.Equal(0, exitCode);
            Assert.Contains("Files inside these projects have changed", output);
            Assert.Contains(projectName, output);
            Assert.DoesNotContain(otherName, output);
        }
    }
}
