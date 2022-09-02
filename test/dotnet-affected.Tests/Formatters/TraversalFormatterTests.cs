using Affected.Cli.Formatters;
using DotnetAffected.Testing.Utils;
using System.Threading.Tasks;
using Xunit;

namespace Affected.Cli.Tests.Formatters
{
    public class TraversalFormatterTests : BaseMSBuildTest
    {
        [Fact]
        public async Task Using_single_project_should_contain_project()
        {
            var formatter = new TraversalProjectOutputFormatter();

            var projectPath = "/home/dev/test/";
            var projects = new[]
            {
                new ProjectInfo("TestProject", projectPath)
            };

            var output = await formatter.Format(projects);

            CustomAssertions.LineSequenceEquals(output,
                l => Assert.Contains("Microsoft.Build.Traversal/3.0.3", l),
                l => Assert.Contains("ItemGroup", l),
                l => Assert.Contains(projectPath, l),
                l => Assert.Contains("ItemGroup", l),
                l => Assert.Contains("Project", l)
            );
        }

        [Fact]
        public async Task Using_multiple_projects_should_contain_them_all()
        {
            var formatter = new TraversalProjectOutputFormatter();

            var firstProjectPath = "/home/dev/test/";
            var secondProjectPath = "/home/dev/other-test/";
            var projects = new[]
            {
                new ProjectInfo("TestProject", firstProjectPath), new ProjectInfo("OtherTest", secondProjectPath)
            };

            var output = await formatter.Format(projects);

            CustomAssertions.LineSequenceEquals(output,
                l => Assert.Contains("Microsoft.Build.Traversal/3.0.3", l),
                l => Assert.Contains("ItemGroup", l),
                l => Assert.Contains(secondProjectPath, l),
                l => Assert.Contains(firstProjectPath, l),
                l => Assert.Contains("ItemGroup", l),
                l => Assert.Contains("Project", l)
            );
        }
    }
}
