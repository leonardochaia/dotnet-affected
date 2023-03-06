using Affected.Cli.Formatters;
using DotnetAffected.Testing.Utils;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Affected.Cli.Tests.Formatters
{
    public class JsonFormatterTests : BaseMSBuildTest
    {
        [Fact]
        public async Task Using_single_project_should_contain_project()
        {
            var formatter = new JsonOutputFormatter();

            var projectPath = "/home/dev/test/test.csproj";
            var projects = new[]
            {
                new ProjectInfo("TestProject", projectPath)
            };

            var output = await formatter.Format(projects);

            Assert.Equal(JsonSerializer.Serialize(projects, JsonOutputFormatter.SerializerOptions), output);
        }

        [Fact]
        public async Task Using_multiple_projects_should_contain_them_all()
        {
            var formatter = new JsonOutputFormatter();

            var firstProjectPath = "/home/dev/test/test.csproj";
            var secondProjectPath = "/home/dev/other-test/other-test.csproj";
            var projects = new[]
            {
                new ProjectInfo("TestProject", firstProjectPath), new ProjectInfo("OtherTest", secondProjectPath)
            };

            var output = await formatter.Format(projects);

            Assert.Equal(JsonSerializer.Serialize(projects, JsonOutputFormatter.SerializerOptions), output);
        }
    }
}
