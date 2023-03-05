using DotnetAffected.Tasks.Tests.Resources;
using DotnetAffected.Testing.Utils;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace DotnetAffected.Tasks.Tests
{
    public class AffectedDetectionTests : BaseAffectedTaskBuildTest
    {
        public AffectedDetectionTests(ITestOutputHelper helper)
            : base(helper)
        {
        }

        [Fact]
        public async Task When_changes_are_made_to_a_project_dependant_projects_should_be_affected()
        {
            var project1 = Repository.CreateCsProject("project-1");
            var project2 = Repository.CreateCsProject("project-2");

            await Repository.PrepareTaskInfra();

            // Commit so there are no changes
            Repository.StageAndCommit();

            await Repository.CreateTextFileAsync(project1, $"file-0.cs", $"// contents 0");

            ExecuteCommandAndCollectResults();

            //Assert
            Assert.True(ExitSuccess);
            Assert.True(HasProjects);
            Assert.Single(Projects);
            Assert.Equal(project1.FullPath, Projects.Single());
        }

        [Fact]
        public async Task When_task_filter_is_defined_only_changes_that_pass_the_filter_should_be_affected()
        {
            // Test A

            await Repository.PrepareTaskInfra(TestProjectScenarios.AffectedFilterClass);

            var project1 = Repository.CreateCsProject("project-1", p =>
            {
                p.AddProperty("IsClientLibrary", "false");
            });
            var project2 = Repository.CreateCsProject("project-2", p =>
            {
                p.AddProperty("IsClientLibrary", "false");
            });

            // Commit so there are no changes
            Repository.StageAndCommit();

            await Repository.CreateTextFileAsync(project1, $"file-0.cs", $"// contents 0");
            await Repository.CreateTextFileAsync(project2, $"file-0.cs", $"// contents 0");

            ExecuteCommandAndCollectResults();

            Assert.True(ExitSuccess);
            Assert.False(HasProjects);
            Assert.Empty(Projects);

            // Test B
            Repository.StageAndCommit();

            project2.AddOrUpdateProperty("IsClientLibrary", "true");
            project2.Save();

            await Repository.CreateTextFileAsync(project1, $"file-1.cs", $"// contents 1");
            await Repository.CreateTextFileAsync(project2, $"file-1.cs", $"// contents 1");

            ExecuteCommandAndCollectResults();

            // Commit so there are no changes
            Assert.True(ExitSuccess);
            Assert.True(HasProjects);
            Assert.Single(Projects);
            Assert.Equal(project2.FullPath, Projects.Single());

            // Test C
            Repository.StageAndCommit();

            project1.AddOrUpdateProperty("IsClientLibrary", "true");
            project1.Save();

            await Repository.CreateTextFileAsync(project1, $"file-2.cs", $"// contents 2");
            await Repository.CreateTextFileAsync(project2, $"file-2.cs", $"// contents 2");

            ExecuteCommandAndCollectResults();

            // Commit so there are no changes
            Assert.True(ExitSuccess);
            Assert.True(HasProjects);
            Assert.Equal(2, Projects.Count());
            Assert.Contains(project1.FullPath, Projects);
            Assert.Contains(project2.FullPath, Projects);
        }
    }
}
