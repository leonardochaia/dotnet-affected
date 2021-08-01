using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Affected.Cli.Tests
{
    public class AffectedDetectionTests
        : BaseDotnetAffectedCommandTest
    {
        public AffectedDetectionTests(ITestOutputHelper helper) : base(helper)
        {
        }

        [Fact]
        public void When_changes_are_made_to_a_project_dependant_projects_should_be_affected()
        {
            // Create a project
            var projectName = "InventoryManagement";
            using var directory = new TempWorkingDirectory();
            var projectPath = directory.MakePathForCsProj(projectName);

            CreateProject(projectPath, projectName)
                .Save();

            // Create another project that depends on the first one
            var dependantProjectName = "InventoryManagement.Tests";
            var dependantProjectPath = directory.MakePathForCsProj(dependantProjectName);

            CreateProject(dependantProjectPath, dependantProjectName)
                .AddDependency(projectPath)
                .Save();

            // Fake changes to first project's csproj file.
            SetupChanges(directory.Path, projectPath);

            var context = CreateCommandExecutionContext(directory.Path);

            Assert.Single(context.ChangedProjects);
            Assert.Single(context.AffectedProjects);

            var changedProject = context.ChangedProjects.Single();
            Assert.Equal(projectName, changedProject.Name);
            Assert.Equal(projectPath, changedProject.FilePath);

            var affectedProject = context.AffectedProjects.Single();
            Assert.Equal(dependantProjectName, affectedProject.Name);
            Assert.Equal(dependantProjectPath, affectedProject.FilePath);
        }

        [Fact]
        public void When_recursively_changes_are_made_to_a_project_dependant_projects_should_be_affected()
        {
            using var directory = new TempWorkingDirectory();

            // Create a shared project
            var sharedProjectName = "InventoryManagement.Domain";
            var sharedProjectPath = directory.MakePathForCsProj(sharedProjectName);

            CreateProject(sharedProjectPath, sharedProjectName)
                .Save();

            // Create a project
            var projectName = "InventoryManagement";
            var projectPath = directory.MakePathForCsProj(projectName);

            CreateProject(projectPath, projectName)
                .AddDependency(sharedProjectPath)
                .Save();

            // Create another project that depends on the first one
            var dependantProjectName = "InventoryManagement.Tests";
            var dependantProjectPath = directory.MakePathForCsProj(dependantProjectName);

            CreateProject(dependantProjectPath, dependantProjectName)
                .AddDependency(projectPath)
                .Save();

            // Fake changes to first project's csproj file.
            SetupChanges(directory.Path, sharedProjectPath);

            var context = CreateCommandExecutionContext(directory.Path);

            Assert.Single(context.ChangedProjects);
            Assert.Equal(2, context.AffectedProjects.Count());

            var changedProject = context.ChangedProjects.Single();
            Assert.Equal(sharedProjectName, changedProject.Name);
            Assert.Equal(sharedProjectPath, changedProject.FilePath);

            var firstAffectedProject = context.AffectedProjects.First();
            Assert.Equal(projectName, firstAffectedProject.Name);
            Assert.Equal(projectPath, firstAffectedProject.FilePath);

            var secondAffectedProject = context.AffectedProjects.ElementAt(1);
            Assert.Equal(dependantProjectName, secondAffectedProject.Name);
            Assert.Equal(dependantProjectPath, secondAffectedProject.FilePath);
        }
    }
}
