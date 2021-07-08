using Microsoft.Build.Construction;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using Xunit;
using Xunit.Abstractions;

namespace Affected.Cli.Tests
{
    public class PublicApiTests
        : BaseMSBuildTest
    {
        private readonly ITestOutputHelper _helper;

        public PublicApiTests(ITestOutputHelper helper)
        {
            _helper = helper;
        }

        private (string Output, int ExitCode) Invoke(string args)
        {
            var parser = CommandLineBuilderUtils
                .CreateCommandLineBuilder()
                .Build();

            var console = new TestConsole();
            var exitCode = parser.Invoke(args, console);
            var output = console.Out.ToString();

            this._helper.WriteLine(output);

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

        [Fact]
        public void RootCommand_WithAffected_ShouldExitCode_AndPrint()
        {
            // Create a project
            var projectName = "InventoryManagement";
            using var directory = new FileUtilities.TempWorkingDirectory();
            var imPath = FileUtilities.GetTemporaryFile(directory.Path, ".csproj");

            var imProject = CreateProject(imPath, projectName);
            imProject.Save();

            // Test project for our project
            var testProjectName = "InventoryManagement.Tests";
            var imTestPath = FileUtilities.GetTemporaryFile(directory.Path, ".csproj");

            var imTestProject = CreateProject(imTestPath, testProjectName);
            imTestProject.AddItemGroup().AddItem("ProjectReference", imPath);
            imProject.Save();

            var (output, exitCode) = this.Invoke($"-v --assume-changes {projectName} -p {directory.Path}");

            Assert.Equal(0, exitCode);
            Assert.Contains("These projects are affected by those changes", output);
            Assert.Contains(testProjectName, output);
        }

        [Fact]
        public void RootCommand_WithoutAffected_ShouldExitCode_AndPrint()
        {
            var projectName = "InventoryManagement";
            using var directory = CreateSingleProject(projectName);

            var (output, exitCode) = this.Invoke($"-v --assume-changes {projectName} -p {directory.Path}");

            Assert.Equal(AffectedExitCodes.NothingAffected, exitCode);

            Assert.Contains("No affected projects where found for the current changes", output);
        }

        [Fact]
        public void ChangesCommand_ShouldPrint_ProjectsWithChanges()
        {
            var projectName = "InventoryManagement";
            using var directory = CreateSingleProject(projectName);

            var (output, exitCode) = this.Invoke($"changes -v --assume-changes {projectName} -p {directory.Path}");

            Assert.Equal(0, exitCode);
            Assert.Contains("Files inside these projects have changed", output);
            Assert.Contains(projectName, output);
        }
    }
}
