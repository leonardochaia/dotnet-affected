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
            var affected = dep1Node.FindReferencingProjects()
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
            var affected = dep2Node.FindReferencingProjects()
                .OrderBy(x => x.ProjectInstance.FullPath)
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
            var affected = new[]
                {
                    dep2Node, dep3Node,
                }.FindReferencingProjects()
                .OrderBy(x => x.ProjectInstance.FullPath)
                .ToList();

            // Assert
            Assert.Equal(3, affected.Count);

            var dep1Node = graph.FindNodeByPath(dep1Project.FullPath);
            var projectThatChangedNode = graph.FindNodeByPath(projectThatChanged.FullPath);
            var otherProjectThatChangedNode = graph.FindNodeByPath(otherProjectThatChanged.FullPath);

            Assert.Collection(affected,
                k => Assert.Equal(dep1Node, k),
                k => Assert.Equal(otherProjectThatChangedNode, k),
                k => Assert.Equal(projectThatChangedNode, k)
            );
        }
    }
}
