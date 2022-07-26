using Microsoft.Build.Graph;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Affected.Cli.Tests
{
    public class ChangedProjectsPredictorTests : BaseServiceProviderRepositoryTest
    {
        private IChangedProjectsProvider ChangedProjectsProvider =>
            this.ServiceProvider.GetRequiredService<IChangedProjectsProvider>();

        private ProjectGraph Graph =>
            this.ServiceProvider.GetRequiredService<IProjectGraphRef>()
                .Value;

        [Fact]
        public async Task FindProjectsForFilePaths_ShouldFindSingleProject()
        {
            // Arrange
            var project1 = Repository.CreateCsProject("context/Project1");

            Repository.CreateCsProject("context/Project2");

            var targetFilePath = Path.Combine(project1.DirectoryPath, "file.cs");
            await this.Repository.CreateTextFileAsync(targetFilePath, "// Initial content");

            // Act
            var projects = this.ChangedProjectsProvider.GetReferencingProjects(new[]
                {
                    targetFilePath,
                })
                .ToArray();

            // Assert
            var project1Node = Graph.FindNodeByPath(project1.FullPath);
            Assert.Single(projects);
            Assert.Equal(project1Node, projects.Single());
        }

        [Fact]
        public async Task FindProjectsForFilePaths_WithMultipleFiles_ShouldFindSingleProject()
        {
            // Arrange
            var project1 = Repository.CreateCsProject("context/Project1");

            Repository.CreateCsProject("context/Project2");

            var targetFilePath = Path.Combine(project1.DirectoryPath, "file.cs");
            await this.Repository.CreateTextFileAsync(targetFilePath, "// Initial content");

            var targetFilePath2 = Path.Combine(project1.DirectoryPath, "file2.cs");
            await this.Repository.CreateTextFileAsync(targetFilePath2, "// Other content");

            // Act
            var projects = this.ChangedProjectsProvider.GetReferencingProjects(new[]
                {
                    targetFilePath, targetFilePath2
                })
                .ToArray();

            // Assert
            var project1Node = Graph.FindNodeByPath(project1.FullPath);
            Assert.Single(projects);
            Assert.Equal(project1Node, projects.Single());
        }

        [Fact]
        public async Task FindProjectsForFilePaths_ShouldFindMultipleProject()
        {
            // Arrange
            var project1 = Repository.CreateCsProject("context/Project1");

            var project2 = Repository.CreateCsProject("context/Project2");

            var targetFilePath = Path.Combine(project1.DirectoryPath, "file.cs");
            await this.Repository.CreateTextFileAsync(targetFilePath, "// Initial content");

            var targetFilePath2 = Path.Combine(project2.DirectoryPath, "file.cs");
            await this.Repository.CreateTextFileAsync(targetFilePath2, "// Initial content");

            // Act
            var projects = this.ChangedProjectsProvider.GetReferencingProjects(new[]
                {
                    targetFilePath, targetFilePath2
                })
                .OrderBy(p => p.GetProjectName())
                .ToArray();

            // Assert
            var project1Node = Graph.FindNodeByPath(project1.FullPath);
            var project2Node = Graph.FindNodeByPath(project2.FullPath);

            Assert.Collection(projects,
                p => Assert.Equal(project1Node, p),
                p => Assert.Equal(project2Node, p));
        }
    }
}
