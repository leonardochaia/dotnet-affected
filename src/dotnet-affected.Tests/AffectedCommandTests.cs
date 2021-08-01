using System.IO;
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
            SetupChanges(directory.Path, projectPath);

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
            SetupChanges(directory.Path, projectPath);

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
            SetupChanges(directory.Path, projectPath);

            var (output, exitCode) =
                await this.InvokeAsync($"-p {directory.Path} --dry-run --output-name to-build");

            Assert.Equal(0, exitCode);

            Assert.Contains($"WRITE {directory.Path}/to-build.proj", output);
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
            SetupChanges(directory.Path, projectPath);

            var (output, exitCode) =
                await this.InvokeAsync($"-p {directory.Path} --dry-run --output-name to-build.whatever");

            Assert.Equal(0, exitCode);

            Assert.Contains($"WRITE {directory.Path}/to-build.whatever.proj", output);
        }
    }
}
