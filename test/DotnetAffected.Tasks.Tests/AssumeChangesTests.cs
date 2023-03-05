using DotnetAffected.Tasks.Tests.Resources;
using DotnetAffected.Testing.Utils;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace DotnetAffected.Tasks.Tests
{
    public class AssumeChangesTests : BaseAffectedTaskBuildTest
    {
        public AssumeChangesTests(ITestOutputHelper helper)
            : base(helper)
        {
        }

        [Fact]
        public async Task When_DotnetAffectedAssumeChanges_the_projects_defined_has_changes()
        {
            Repository.CreateCsProject("project-1");
            Repository.CreateCsProject("project-2");
            Repository.CreateCsProject("project3");

            await Repository.PrepareTaskInfra(TestProjectScenarios.AssumeChanges);

            // Commit so there are no changes
            Repository.StageAndCommit();

            ExecuteCommandAndCollectResults();

            //Assert
            Assert.True(ExitSuccess);
            Assert.True(HasProjects);
            Assert.Equal(3, Projects.Count());
        }
    }
}
