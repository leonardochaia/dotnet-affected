using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Affected.Cli.Tests
{
    /// <summary>
    /// Tests for checking output files are generated at the proper path
    /// </summary>
    public class OutputGenerationTests : BaseDotnetAffectedCommandTest
    {
        public OutputGenerationTests(ITestOutputHelper helper) : base(helper)
        {
        }

        [Fact]
        public async Task Should_create_file_at_repo_root()
        {
            // Create a project
            var projectName = "InventoryManagement";
            using var directory = new TempWorkingDirectory();
            var projectPath = directory.MakePathForCsProj(projectName);

            CreateProject(projectPath, projectName)
                .Save();

            // Fake changes to it's project's csproj file.
            SetupFileChanges(directory.Path, projectPath);

            var (output, exitCode) =
                await this.InvokeAsync($"-p {directory.Path} -f text");

            var destination = Path.Combine(directory.Path, "affected.txt");
            var outputContents = await File.ReadAllTextAsync(destination);

            Assert.Equal(0, exitCode);

            Assert.Contains($"WRITE: {destination}", output);
            Assert.Contains(projectPath, outputContents);
        }
        
        [Fact]
        public async Task Using_relative_output_dir_should_create_file_inside_relative_dir()
        {
            // Create a project
            var projectName = "InventoryManagement";
            using var directory = new TempWorkingDirectory();
            var projectPath = directory.MakePathForCsProj(projectName);

            CreateProject(projectPath, projectName)
                .Save();

            // Fake changes to it's project's csproj file.
            SetupFileChanges(directory.Path, projectPath);

            var (output, exitCode) =
                await this.InvokeAsync($"-p {directory.Path} -f text --output-dir relative/");

            var destination = Path.Combine(directory.Path, "relative/affected.txt");

            Assert.Equal(0, exitCode);

            Assert.Contains($"WRITE: {destination}", output);

            Assert.True(File.Exists(destination));
            var outputContents = await File.ReadAllTextAsync(destination);
            Assert.Contains(projectPath, outputContents);
        }
        
        [Fact]
        public async Task When_using_output_name_should_create_file()
        {
            // Create a project
            var projectName = "InventoryManagement";
            using var directory = new TempWorkingDirectory();
            var projectPath = directory.MakePathForCsProj(projectName);

            CreateProject(projectPath, projectName)
                .Save();

            // Fake changes to it's project's csproj file.
            SetupFileChanges(directory.Path, projectPath);

            var (output, exitCode) =
                await this.InvokeAsync($"-p {directory.Path} --dry-run --output-name to-build");

            Assert.Equal(0, exitCode);

            Assert.Contains($"WRITE {Path.Combine(directory.Path, "to-build.proj")}", output);
        }
        
        [Fact]
        public async Task When_using_output_name_should_create_file_with_extension()
        {
            // Create a project
            var projectName = "InventoryManagement";
            using var directory = new TempWorkingDirectory();
            var projectPath = directory.MakePathForCsProj(projectName);

            CreateProject(projectPath, projectName)
                .Save();

            // Fake changes to it's project's csproj file.
            SetupFileChanges(directory.Path, projectPath);

            var (output, exitCode) =
                await this.InvokeAsync($"-p {directory.Path} --dry-run --output-name to-build.whatever");

            Assert.Equal(0, exitCode);

            Assert.Contains($"WRITE {Path.Combine(directory.Path, "to-build.whatever.proj")}", output);
        }
    }
}
