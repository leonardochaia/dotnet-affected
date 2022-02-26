using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Affected.Cli.Tests
{
    /// <summary>
    /// Tests for detecting when a set of projects have changed.
    /// This should cover all possible change detection scenarios.
    /// </summary>
    public class ChangeDetectionTests
        : BaseDotnetAffectedCommandTest
    {
        public ChangeDetectionTests(ITestOutputHelper helper) : base(helper)
        {
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
            SetupFileChanges(directory.Path, projectPath);

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
            SetupFileChanges(directory.Path, Path.Combine(projectDirectory!, "some/random/file.cs"));

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
            SetupFileChanges(directory.Path, otherPath);

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
        public void When_nothing_has_changed_should_throw_nothing_has_changed()
        {
            var projectName = "InventoryManagement";
            using var directory = CreateSingleProject(projectName);

            var context = CreateCommandExecutionContext(
                directory.Path);

            Assert.Throws<NoChangesException>(() => context.ChangedProjects);
        }
    }
}
