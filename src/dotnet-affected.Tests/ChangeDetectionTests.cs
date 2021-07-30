using Affected.Cli.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        private ICommandExecutionContext CreateCommandExecutionContext(
            string directoryPath,
            IEnumerable<string> assumeChanges = null,
            string solutionPath = null
        )
        {
            var data = new CommandExecutionData(directoryPath,
                solutionPath ?? string.Empty, String.Empty,
                String.Empty, true, assumeChanges);

            var context = new CommandExecutionContext(data, this.Terminal, this.ChangesProviderMock.Object);
            return context;
        }

        [Fact]
        public void Using_changes_provider_when_has_changes_project_should_have_changed()
        {
            // Create a project
            var projectName = "InventoryManagement";
            using var directory = new TempWorkingDirectory();
            var projectPath = directory.MakePathForCsProj(projectName);

            CreateProject(projectPath, projectName)
                .Save();

            // Fake changes on the project's file
            SetupChanges(directory.Path, projectPath);

            var context = CreateCommandExecutionContext(directory.Path);

            Assert.Single(context.ChangedProjects);
            Assert.Empty(context.AffectedProjects);

            var projectInfo = context.ChangedProjects.Single();
            Assert.Equal(projectName, projectInfo.Name);
            Assert.Equal(projectPath, projectInfo.FilePath);
        }

        [Fact]
        public void
            Using_change_provider_when_has_changes_to_file_inside_project_directory_project_should_have_changes()
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

            var context = CreateCommandExecutionContext(directory.Path);

            Assert.Single(context.ChangedProjects);
            Assert.Empty(context.AffectedProjects);

            var projectInfo = context.ChangedProjects.Single();
            Assert.Equal(projectName, projectInfo.Name);
            Assert.Equal(projectPath, projectInfo.FilePath);
        }

        [Fact]
        public void Using_assume_changes_when_has_changes_project_should_have_changes()
        {
            // Create a project
            var projectName = "InventoryManagement";
            using var directory = new TempWorkingDirectory();
            var projectPath = directory.MakePathForCsProj(projectName);

            CreateProject(projectPath, projectName)
                .Save();

            var context = CreateCommandExecutionContext(directory.Path,
                new[]
                {
                    projectName
                });

            Assert.Single(context.ChangedProjects);
            Assert.Empty(context.AffectedProjects);

            var projectInfo = context.ChangedProjects.Single();
            Assert.Equal(projectName, projectInfo.Name);
            Assert.Equal(projectPath, projectInfo.FilePath);
        }

        [Fact]
        public void Using_assume_changes_should_ignore_other_changes()
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

            var context = CreateCommandExecutionContext(directory.Path,
                new[]
                {
                    projectName
                });

            Assert.Single(context.ChangedProjects);
            Assert.Empty(context.AffectedProjects);

            var projectInfo = context.ChangedProjects.Single();
            Assert.Equal(projectName, projectInfo.Name);
            Assert.Equal(projectPath, projectInfo.FilePath);
        }

        [Fact]
        public async Task Using_solution_when_has_changes_should_print_and_exit_successfully()
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

            var context = CreateCommandExecutionContext(
                directory.Path,
                solutionPath: solutionPath);

            Assert.Single(context.ChangedProjects);
            Assert.Empty(context.AffectedProjects);

            var projectInfo = context.ChangedProjects.Single();
            Assert.Equal(projectName, projectInfo.Name);
            Assert.Equal(projectPath, projectInfo.FilePath);
        }

        [Fact]
        public async Task Using_solution_should_ignore_changes_outside_solution()
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

            var context = CreateCommandExecutionContext(
                directory.Path,
                solutionPath: solutionPath);

            Assert.Single(context.ChangedProjects);
            Assert.Empty(context.AffectedProjects);

            var projectInfo = context.ChangedProjects.Single();
            Assert.Equal(projectName, projectInfo.Name);
            Assert.Equal(projectPath, projectInfo.FilePath);
        }
    }
}
