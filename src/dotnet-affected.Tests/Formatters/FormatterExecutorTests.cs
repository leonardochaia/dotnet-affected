using Affected.Cli.Formatters;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Affected.Cli.Tests.Formatters
{
    public class FormatterExecutorTests
        : BaseDotnetAffectedCommandTest
    {
        public FormatterExecutorTests(ITestOutputHelper helper) : base(helper)
        {
        }

        [Fact]
        public async Task Should_create_file_at_directory()
        {
            var formatterType = "text";
            var formatter = new TextOutputFormatter();
            var executor = new OutputFormatterExecutor(new[]
            {
                formatter
            }, this.Terminal);


            using var directory = new TempWorkingDirectory();
            var projectPath = directory.MakePathForCsProj("test");
            var projects = new[]
            {
                new ProjectInfo("TestProject", projectPath)
            };

            await executor.Execute(projects, new[]
            {
                formatterType
            }, directory.Path, "affected", false, true);

            var outputPath = Path.Combine(directory.Path, "affected.txt");
            var outputContents = await File.ReadAllTextAsync(outputPath);

            Assert.True(File.Exists(outputPath));

            Assert.Contains(projectPath, outputContents);
        }
    }
}
