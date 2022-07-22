using Microsoft.Build.Graph;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Affected.Cli.Tests
{
    public class ProjectGraphExtensionsTests
        : BaseRepositoryTest
    {
        [Fact]
        public async Task FindProjectsForFilePaths_ShouldFindSingleProject()
        {
            // Arrange
            var project1 = Repository.CreateCsProject("context/Project1");

            var project2 = Repository.CreateCsProject("context/Project2");

            var targetFilePath = Path.Combine(project1.DirectoryPath, "file.cs");
            await this.Repository.CreateTextFileAsync(targetFilePath, "// Initial content");

            var graph = new ProjectGraph(new string[]
            {
                project1.FullPath, project2.FullPath
            });

            // Act
            var projects = graph.FindNodesContainingFiles(new[]
                {
                    targetFilePath,
                })
                .ToList();

            // Assert
            var project1Node = graph.FindNodeByPath(project1.FullPath);
            Assert.Single(projects);
            Assert.Equal(project1Node, projects.Single());
        }

        [Fact]
        public async Task FindProjectsForFilePaths_WithMultipleFiles_ShouldFindSingleProject()
        {
            // Arrange
            var project1 = Repository.CreateCsProject("context/Project1");

            var project2 = Repository.CreateCsProject("context/Project2");

            var targetFilePath = Path.Combine(project1.DirectoryPath, "file.cs");
            await this.Repository.CreateTextFileAsync(targetFilePath, "// Initial content");

            var targetFilePath2 = Path.Combine(project1.DirectoryPath, "file2.cs");
            await this.Repository.CreateTextFileAsync(targetFilePath2, "// Other content");

            var graph = new ProjectGraph(new string[]
            {
                project1.FullPath, project2.FullPath
            });

            // Act
            var projects = graph.FindNodesContainingFiles(new[]
                {
                    targetFilePath, targetFilePath2,
                })
                .ToList();

            // Assert
            var project1Node = graph.FindNodeByPath(project1.FullPath);
            Assert.Single(projects);
            Assert.Equal(project1Node, projects.Single());
        }

        [Fact]
        public async Task FindProjectsForFilePaths_ShouldFindMultipleProject()
        {
            // Arrange
            // Arrange
            var project1 = Repository.CreateCsProject("context/Project1");

            var project2 = Repository.CreateCsProject("context/Project2");

            var targetFilePath = Path.Combine(project1.DirectoryPath, "file.cs");
            await this.Repository.CreateTextFileAsync(targetFilePath, "// Initial content");

            var targetFilePath2 = Path.Combine(project2.DirectoryPath, "file.cs");
            await this.Repository.CreateTextFileAsync(targetFilePath2, "// Initial content");

            var graph = new ProjectGraph(new string[]
            {
                project1.FullPath, project2.FullPath
            });

            // Act
            var projects = graph.FindNodesContainingFiles(new[]
                {
                    targetFilePath, targetFilePath2,
                })
                .ToList();

            // Assert
            var project1Node = graph.FindNodeByPath(project1.FullPath);
            var project2Node = graph.FindNodeByPath(project2.FullPath);

            Assert.Collection(projects,
                p => Assert.Equal(project1Node, p),
                p => Assert.Equal(project2Node, p));
        }
    }
}
