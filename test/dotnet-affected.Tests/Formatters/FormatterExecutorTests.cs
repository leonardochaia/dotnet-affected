using DotnetAffected.Testing.Utils;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Linq;
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
        
        [Fact]
        public async Task Should_write_deduplicated_projects()
        {
            // Arrange
            const string formatterType = "text";
            const string projectName = "InventoryManagement";
            const string outputFileName = "affected";
            var msBuildProject = this.Repository.CreateCsProject(projectName);
            var executor = this.ServiceProvider.GetRequiredService<IOutputFormatterExecutor>();
            var projects = new[]
            {
                new ProjectInfo("DuplicatedTestProject", msBuildProject.FullPath),
                new ProjectInfo("DuplicatedTestProject", msBuildProject.FullPath),
            };

            // Act
            await executor.Execute(projects, new[]
            {
                formatterType
            }, Repository.Path, outputFileName, false, true);

            // Assert
            var outputPath = Path.Combine(Repository.Path, $"{outputFileName}.txt");
            var outputContents = await File.ReadAllLinesAsync(outputPath);
            Assert.Single(outputContents);
            Assert.Equal(msBuildProject.FullPath, outputContents.First());
        }
    }
}
