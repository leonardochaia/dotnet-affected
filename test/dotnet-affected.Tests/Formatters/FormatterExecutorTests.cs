using DotnetAffected.Testing.Utils;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Affected.Cli.Tests.Formatters
{
    public class FormatterExecutorTests
        : BaseServiceProviderCliTest
    {
        [Fact]
        public async Task Should_create_file_at_directory()
        {
            var formatterType = "text";

            // Create a project
            var projectName = "InventoryManagement";
            var msBuildProject = this.Repository.CreateCsProject(projectName);

            var executor = this.ServiceProvider.GetRequiredService<IOutputFormatterExecutor>();
            var projects = new[]
            {
                new ProjectInfo("TestProject", msBuildProject.FullPath)
            };

            await executor.Execute(projects, new[]
            {
                formatterType
            }, Repository.Path, "affected", false, true);

            var outputPath = Path.Combine(Repository.Path, "affected.txt");
            var outputContents = await File.ReadAllTextAsync(outputPath);

            Assert.True(File.Exists(outputPath));

            Assert.Contains(msBuildProject.FullPath, outputContents);
        }
    }
}
