using DotnetAffected.Tasks.Tests.Resources;
using DotnetAffected.Testing.Utils;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace DotnetAffected.Tasks.Tests
{
    public class GitBetweenCommitsChangeDetectionTests : BaseAffectedTaskBuildTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public GitBetweenCommitsChangeDetectionTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task When_has_changes_project_should_have_changed()
        {
            // Create a project
            var projectName = "InventoryManagement";
            var msBuildProject = Repository.CreateCsProject(projectName);

            // Make a commit and keep track of the sha
            var fromCommit = Repository.StageAndCommit().Sha;

            // Create a file with some changes
            var targetFilePath = Path.Combine(projectName, "file.cs");
            await this.Repository.CreateTextFileAsync(targetFilePath, "// Initial content");

            // Commit the changes
            var toCommit = Repository.StageAndCommit().Sha;

            await Repository.PrepareTaskInfra(TestProjectScenarios.GitBetweenCommits.Replace("__FromRef__", fromCommit).Replace("__ToRef__", toCommit));

            ExecuteCommandAndCollectResults();

            Assert.True(ExitSuccess);
            Assert.True(HasProjects);
            Assert.Single(Projects);
            Assert.Equal(msBuildProject.FullPath, Projects.Single());

#if (NET5_0_OR_GREATER)
            fromCommit = toCommit;
            toCommit = Repository.StageAndCommit().Sha;
            
            await Repository.PrepareTaskInfra(TestProjectScenarios.GitBetweenCommits.Replace("__FromRef__", fromCommit).Replace("__ToRef__", toCommit));

            ExecuteCommandAndCollectResults();

            Assert.True(ExitSuccess);
            Assert.False(HasProjects);
            Assert.Empty(Projects);
#endif
        }
        
    }
}
