using Microsoft.Build.Construction;
using System.CommandLine.Parsing;
using System.CommandLine.Rendering;
using Xunit;
using Xunit.Abstractions;
using System.Threading.Tasks;

namespace Affected.Cli.Tests
{
    public class PublicApiTests
        : BaseMSBuildTest
    {
        private readonly ITestOutputHelper _helper;

        private readonly ITerminal _terminal = new TestTerminal()
        {
            OutputMode = OutputMode.PlainText,
        };

        public PublicApiTests(ITestOutputHelper helper)
        {
            _helper = helper;
        }

        private async Task<(string Output, int ExitCode)> InvokeAsync(string args)
        {
            var parser = CommandLineBuilderUtils
                .CreateCommandLineBuilder()
                .Build();

            var exitCode = await parser.InvokeAsync("[output:PlainText] "+args, _terminal);
            var output = _terminal.Out.ToString();

            this._helper.WriteLine(output);
            this._helper.WriteLine(_terminal.Error.ToString());

            return (output, exitCode);
        }

        private FileUtilities.TempWorkingDirectory CreateSingleProject(string projectName)
        {
            var directory = new FileUtilities.TempWorkingDirectory();
            var csprojPath = FileUtilities.GetTemporaryFile(directory.Path, ".csproj");

            var projectRoot = CreateProject(csprojPath, projectName);
            projectRoot.Save();

            return directory;
        }

        private ProjectRootElement CreateProject(string csprojPath, string projectName)
        {
            ProjectRootElement projectRoot = ProjectRootElement.Create(csprojPath);
            projectRoot.AddProperty("ProjectName", projectName);
            return projectRoot;
        }
        //
        // [Fact]
        // public async Task RootCommand_WithAffected_ShouldExitCode_AndPrint()
        // {
        //     // Create a project
        //     var projectName = "InventoryManagement";
        //     using var directory = new FileUtilities.TempWorkingDirectory();
        //     var imPath = FileUtilities.GetTemporaryFile(directory.Path, ".csproj");
        //
        //     var imProject = CreateProject(imPath, projectName);
        //     imProject.Save();
        //
        //     // Test project for our project
        //     var testProjectName = "InventoryManagement.Tests";
        //     var imTestPath = FileUtilities.GetTemporaryFile(directory.Path, ".csproj");
        //
        //     var imTestProject = CreateProject(imTestPath, testProjectName);
        //     imTestProject.AddItemGroup().AddItem("ProjectReference", imPath);
        //     imProject.Save();
        //
        //     var (output, exitCode) = await this.InvokeAsync($"-v --assume-changes {projectName} -p {directory.Path}");
        //
        //     Assert.Equal(0, exitCode);
        //     Assert.Contains("These projects are affected by those changes", output);
        //     Assert.Contains(testProjectName, output);
        // }

        [Fact]
        public async Task RootCommand_WithoutAffected_ShouldExitCode_AndPrint()
        {
            var projectName = "InventoryManagement";
            using var directory = CreateSingleProject(projectName);

            var (output, exitCode) = await this.InvokeAsync($"-v --assume-changes {projectName} -p {directory.Path}");

            _helper.WriteLine("FINISHED");
            Assert.Equal(AffectedExitCodes.NothingAffected, exitCode);

            Assert.Contains("No affected projects where found for the current changes", output);
        }
        //
        // [Fact]
        // public async Task ChangesCommand_ShouldPrint_ProjectsWithChanges()
        // {
        //     var projectName = "InventoryManagement";
        //     using var directory = CreateSingleProject(projectName);
        //
        //     var (output, exitCode) = await this.InvokeAsync($"changes -v --assume-changes {projectName} -p {directory.Path}");
        //
        //     Assert.Equal(0, exitCode);
        //     Assert.Contains("Files inside these projects have changed", output);
        //     Assert.Contains(projectName, output);
        // }
    }
}
