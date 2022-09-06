using Affected.Cli.Formatters;
using System.Threading.Tasks;
using Xunit;

namespace Affected.Cli.Tests.Formatters
{
    public class TextFormatterTests
    {
        [Fact]
        public async Task Using_single_project_should_contain_project()
        {
            var formatter = new TextOutputFormatter();

            var projectPath = "/home/dev/test/";
            var projects = new[]
            {
                new ProjectInfo("TestProject", projectPath)
            };

            var output = await formatter.Format(projects);

            Assert.Contains(projectPath, output);
        }

        [Fact]
        public async Task Using_multiple_projects_should_contain_them_all()
        {
            var formatter = new TextOutputFormatter();

            var firstProjectPath = "/home/dev/test/";
            var secondProjectPath = "/home/dev/other-test/";
            var projects = new[]
            {
                new ProjectInfo("TestProject", firstProjectPath), new ProjectInfo("OtherTest", secondProjectPath)
            };

            var output = await formatter.Format(projects);

            Assert.Contains(firstProjectPath, output);
            Assert.Contains(secondProjectPath, output);
        }
    }
}
