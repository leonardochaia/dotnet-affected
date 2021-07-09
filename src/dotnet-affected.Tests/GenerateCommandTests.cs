using System;
using System.IO;
using System.Linq;
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
        public async Task When_nothing_has_changed_should_exit_with_NothingChanged_status_code()
        {
            var projectName = "InventoryManagement";
            using var directory = CreateSingleProject(projectName);

            // Fake No changes
            SetupChanges(directory.Path, Enumerable.Empty<string>());

            var (output, exitCode) = await this.InvokeAsync($"generate -p {directory.Path}");

            Assert.Equal(AffectedExitCodes.NothingChanged, exitCode);

            RenderingAssertions.LineSequenceEquals(output,
                l => Assert.Contains("No affected projects where found for the current changes", l));
        }

        [Fact]
        public async Task When_any_changes_should_generate_traversal_sdk()
        {
            var projectName = "InventoryManagement";
            using var directory = new FileUtilities.TempWorkingDirectory();
            var projectPath = directory.CreateTemporaryCsProjFile();

            CreateProject(projectPath, projectName)
                .Save();

            // Fake changes on the project
            SetupChanges(directory.Path, new[]
            {
                projectPath
            });

            var (output, exitCode) =
                await this.InvokeAsync($"generate -p {directory.Path}");

            Assert.Equal(0, exitCode);

            RenderingAssertions.LineSequenceEquals(output,
                l => Assert.Contains("Microsoft.Build.Traversal/3.0.3", l),
                l => Assert.Contains("ItemGroup", l),
                l => Assert.Contains("ProjectReference", l),
                l => Assert.Contains($"Include=\"{projectPath}\"", l),
                l => Assert.Contains("/>", l),
                l => Assert.Contains("/ItemGroup", l),
                l => Assert.Contains("/Project", l)
            );
        }

        [Fact]
        public async Task When_affected_should_generate_traversal_sdk()
        {
            var projectName = "InventoryManagement";
            using var directory = new FileUtilities.TempWorkingDirectory();
            var projectPath = directory.CreateTemporaryCsProjFile();

            CreateProject(projectPath, projectName)
                .Save();

            // Fake changes on the project
            SetupChanges(directory.Path, new[]
            {
                projectPath
            });

            // Create another project that depends on the first one
            var dependantProjectName = "InventoryManagement.Tests";
            var dependantProjectPath = directory.CreateTemporaryCsProjFile();

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
            var projectName = "InventoryManagement";
            using var directory = new FileUtilities.TempWorkingDirectory();
            var projectPath = directory.CreateTemporaryCsProjFile(projectName);

            var destination = Path.Combine(directory.Path, $"{Guid.NewGuid():N}-generate-output.txt");

            CreateProject(projectPath, projectName)
                .Save();

            // Fake changes on the project
            SetupChanges(directory.Path, new[]
            {
                projectPath
            });

            // Create another project that depends on the first one
            var dependantProjectName = "InventoryManagement.Tests";
            var dependantProjectPath = directory.CreateTemporaryCsProjFile(dependantProjectName);

            CreateProject(dependantProjectPath, dependantProjectName)
                .AddDependency(projectPath)
                .Save();

            var (output, exitCode) =
                await this.InvokeAsync($"generate -p {directory.Path} --output {destination}");

            Assert.Equal(0, exitCode);

            var outputContents = await File.ReadAllTextAsync(destination);

            Helper.WriteLine($"TestInfra: Contents of generated output file at {destination}");
            Helper.WriteLine(outputContents);

            Assert.Contains("Microsoft.Build.Traversal/3.0.3", outputContents);
            Assert.Contains($"Include=\"{dependantProjectPath}\"", outputContents);
            Assert.Contains($"Include=\"{projectPath}\"", outputContents);
        }
    }
}
