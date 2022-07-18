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
        public void FindNodesThatDependOn_ShouldFindFirstLevelDependency()
        {
            // Create a project
            var dep1Project = Repository.CreateCsProject("dep1");
            var dep2Project = Repository.CreateCsProject("dep2");

            // child depends on dep1 and dep2
            var dependantMsBuildProject = Repository.CreateCsProject(
                "child",
                b => b
                    .AddProjectDependency(dep1Project.FullPath)
                    .AddProjectDependency(dep2Project.FullPath));
            var graph = new ProjectGraph(dependantMsBuildProject.FullPath);

            var dep1Node = graph.FindNodeByPath(dep1Project.FullPath);

            // Act
            var affected = graph.FindNodesThatDependOn(dep1Node)
                .ToList();

            // Assert
            Assert.Single(affected);
            Assert.Contains(affected, k => k.ProjectInstance.FullPath == dependantMsBuildProject.FullPath);
        }

        [Fact]
        public void FindNodesThatDependOn_ShouldFindAnyLevelDependency()
        {
            // Arrange
            var dep2Project = Repository.CreateCsProject("dep2");

            // dep1 depends on dep2
            var dep1Project = Repository.CreateCsProject("dep1", b => b
                .AddProjectDependency(dep2Project.FullPath));

            // root depends on dep1
            var projectThatChanged = Repository.CreateCsProject("projectThatChanged", b => b
                .AddProjectDependency(dep1Project.FullPath));

            var graph = new ProjectGraph(projectThatChanged.FullPath);

            var dep2Node = graph.FindNodeByPath(dep2Project.FullPath);

            // Act
            var affected = graph.FindNodesThatDependOn(dep2Node)
                .ToList();

            // Assert
            Assert.Equal(2, affected.Count);

            var dep1Node = graph.FindNodeByPath(dep1Project.FullPath);
            var projectThatChangedNode = graph.FindNodeByPath(projectThatChanged.FullPath);

            Assert.Collection(affected,
                k => Assert.Equal(dep1Node, k),
                k => Assert.Equal(projectThatChangedNode, k)
            );
        }

        [Fact]
        public void FindNodesThatDependOn_ShouldFindDependenciesForAllProjects()
        {
            // Arrange
            var dep2Project = Repository.CreateCsProject("dep2");

            // dep1 depends on dep2
            var dep1Project = Repository.CreateCsProject("dep1", b => b
                .AddProjectDependency(dep2Project.FullPath));

            // root depends on dep1
            var projectThatChanged = Repository.CreateCsProject("projectThatChanged", b => b
                .AddProjectDependency(dep1Project.FullPath));

            // other dep
            var dep3Project = Repository.CreateCsProject("dep3");

            // other root depends on other dep
            var otherProjectThatChanged = Repository.CreateCsProject("otherProjectThatChanged", b => b
                .AddProjectDependency(dep3Project.FullPath));

            var graph = new ProjectGraph(new string[]
            {
                projectThatChanged.FullPath, otherProjectThatChanged.FullPath
            });

            var dep2Node = graph.FindNodeByPath(dep2Project.FullPath);
            var dep3Node = graph.FindNodeByPath(dep3Project.FullPath);

            // Act
            var affected = graph.FindNodesThatDependOn(new[]
                {
                    dep2Node, dep3Node,
                })
                .ToList();

            // Assert
            Assert.Equal(3, affected.Count);

            var dep1Node = graph.FindNodeByPath(dep1Project.FullPath);
            var projectThatChangedNode = graph.FindNodeByPath(projectThatChanged.FullPath);
            var otherProjectThatChangedNode = graph.FindNodeByPath(otherProjectThatChanged.FullPath);

            Assert.Collection(affected,
                k => Assert.Equal(dep1Node, k),
                k => Assert.Equal(projectThatChangedNode, k),
                k => Assert.Equal(otherProjectThatChangedNode, k)
            );
        }

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
