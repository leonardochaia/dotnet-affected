using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Affected.Cli.Tests
{
    /// <summary>
    /// Tests for detecting changed projects when using a SolutionFile to filter.
    /// This should cover all tests where filtering should be applied
    /// </summary>
    public class ChangeDetectionUsingSolutionTests
        : BaseDotnetAffectedCommandTest
    {
        public ChangeDetectionUsingSolutionTests(ITestOutputHelper helper) : base(helper)
        {
        }

        [Fact]
        public async Task Using_solution_when_has_changes_project_should_have_changed()
        {
            // Create a project
            var projectName = "InventoryManagement";
            using var directory = new TempWorkingDirectory();
            var projectPath = directory.MakePathForCsProj(projectName);

            CreateProject(projectPath, projectName)
                .Save();

            // Fake changes on the project's file
            SetupFileChanges(directory.Path, projectPath);

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
            SetupFileChanges(directory.Path, projectPath, otherPath);

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
        public async Task
            Using_solution_when_projects_outside_solution_has_changed_should_throw_nothing_changed()
        {
            // Create a project
            var projectName = "InventoryManagement";
            using var directory = new TempWorkingDirectory();
            var projectPath = directory.MakePathForCsProj(projectName);

            CreateProject(projectPath, projectName)
                .Save();

            // Create a solution which includes the project
            var solutionPath = await directory.CreateSolutionFileForProjects("test-solution.sln", projectPath);

            // Create a project that is outside the solution
            var outsiderName = "OutsiderProject";
            var outsiderPath = directory.MakePathForCsProj(outsiderName);

            CreateProject(outsiderPath, outsiderName)
                .Save();

            // Fake changes for the outsider
            SetupFileChanges(directory.Path, outsiderPath);

            var context = CreateCommandExecutionContext(
                directory.Path,
                solutionPath: solutionPath);

            Assert.Throws<NoChangesException>(() => context.ChangedProjects);
        }
    }
}
