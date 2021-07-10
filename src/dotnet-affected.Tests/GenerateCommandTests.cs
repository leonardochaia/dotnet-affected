using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Affected.Cli.Tests
{
    public class GenerateCommandTests
        : BaseDotnetAffectedCommandTest
    {
        public GenerateCommandTests(ITestOutputHelper helper) : base(helper)
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
                await this.InvokeAsync($"generate -p {directory.Path}");

            Assert.Equal(0, exitCode);

            Assert.Contains($"Include=\"{projectPath}\"", output);
        }

        [Fact]
        public async Task When_any_affected_should_output_traversal_sdk()
        {
            // Create a project
            var projectName = "InventoryManagement";
            using var directory = new TempWorkingDirectory();
            var projectPath = directory.MakePathForCsProj(projectName);

            CreateProject(projectPath, projectName)
                .Save();

            // Fake changes on the project
            SetupChanges(directory.Path, new[]
            {
                projectPath
            });

            // Create another project that depends on the first one
            var dependantProjectName = "InventoryManagement.Tests";
            var dependantProjectPath = directory.MakePathForCsProj(dependantProjectName);

            CreateProject(dependantProjectPath, dependantProjectName)
                .AddDependency(projectPath)
                .Save();

            var (output, exitCode) =
                await this.InvokeAsync($"generate -p {directory.Path}");

            Assert.Equal(0, exitCode);

            Assert.Contains("Microsoft.Build.Traversal/3.0.3", output);
            Assert.Contains($"Include=\"{dependantProjectPath}\"", output);
            Assert.Contains($"Include=\"{projectPath}\"", output);
        }

        [Fact]
        public async Task When_affected_and_output_should_generate_traversal_sdk_at_destination()
        {
            // Create a project
            var projectName = "InventoryManagement";
            using var directory = new TempWorkingDirectory();
            var projectPath = directory.MakePathForCsProj(projectName);

            CreateProject(projectPath, projectName)
                .Save();

            // Fake changes on the project
            SetupChanges(directory.Path, projectPath);

            // Create another project that depends on the first one
            var dependantProjectName = "InventoryManagement.Tests";
            var dependantProjectPath = directory.MakePathForCsProj(dependantProjectName);

            CreateProject(dependantProjectPath, dependantProjectName)
                .AddDependency(projectPath)
                .Save();

            // Generate a path for the traversal sdk destination file
            var destination = directory.MakePathFor("generate-output.txt");

            var (output, exitCode) =
                await this.InvokeAsync($"generate -p {directory.Path} --output {destination}");

            Assert.Equal(0, exitCode);
            Assert.Contains(destination, output);

            // Read the output file for assertions
            var outputContents = await File.ReadAllTextAsync(destination);

            Helper.WriteLine($"TestInfra: Contents of generated output file at {destination}");
            Helper.WriteLine(outputContents);

            Assert.Contains("Microsoft.Build.Traversal/3.0.3", outputContents);
            Assert.Contains($"Include=\"{dependantProjectPath}\"", outputContents);
            Assert.Contains($"Include=\"{projectPath}\"", outputContents);
        }
    }
}
