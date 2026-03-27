using Affected.Cli.Formatters;
using DotnetAffected.Testing.Utils;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Affected.Cli.Tests.Formatters
{
    public class SolutionFilterFileFormatterTests : BaseMSBuildTest
    {
        [Fact]
        public async Task Using_no_solution_should_fail()
        {
            var formatter = new SolutionFilterFileOutputFormatter();
            var projects = new[]
            {
                  new ProjectInfo("TestProject", "/home/dev/test/test.csproj")
            };

            await Assert.ThrowsAsync<ArgumentException>(async () => await formatter.Format(projects, null));
        }

        [Fact]
        public async Task Using_single_project_should_contain_it()
        {
            var formatter = new SolutionFilterFileOutputFormatter();

            var solutionFile = "/home/dev/Test.slnx";
            var firstProjectPath = "/home/dev/test/test.csproj";
            var projects = new[]
            {
              new ProjectInfo("TestProject", firstProjectPath)
          };

            var output = await formatter.Format(projects, solutionFile);

            var solution = new
            {
                solution = new
                {
                    path = solutionFile,
                    projects = new[] { Path.GetRelativePath("/home/dev", firstProjectPath) }
                }
            };

            Assert.Equal(JsonSerializer.Serialize(solution, SolutionFilterFileOutputFormatter.SerializerOptions), output);

        }

        [Fact]
        public async Task Using_multiple_project_should_contain_them_all()
        {
            var formatter = new SolutionFilterFileOutputFormatter();

            var solutionFile = "/home/AltTest.slnx";
            var firstProjectPath = "/home/dev/test/proj.csproj";
            var secondProjectPath = "/home/dev/other-test/other-proj.csproj";
            var projects = new[]
            {
              new ProjectInfo("TestProject", firstProjectPath), new ProjectInfo("OtherTest", secondProjectPath)
          };

            var output = await formatter.Format(projects, solutionFile);

            var solution = new
            {
                solution = new
                {
                    path = solutionFile,
                    projects = new[] { Path.GetRelativePath("/home/", firstProjectPath), Path.GetRelativePath("/home/", secondProjectPath) }
                }
            };

            Assert.Equal(JsonSerializer.Serialize(solution, SolutionFilterFileOutputFormatter.SerializerOptions), output);
        }
    }
}
