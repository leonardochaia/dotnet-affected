using Microsoft.Build.Construction;
using Microsoft.Build.Graph;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Affected.Cli.Tests
{
    public class ProjectGraphExtensionsTests
        : BaseMSBuildTest
    {
        [Fact]
        public void FindNodesThatDependOn_ShouldFindFirstLevelDependency()
        {
            // Arrange
            using var dep1 = CreateProjectFile("dep1");
            using var dep2 = CreateProjectFile("dep2");

            // child depends on dep1 and dep2
            using var child = CreateProjectFile("child", dep1, dep2);
            var graph = new ProjectGraph(child.Path);

            var dep1Node = graph.FindNodeByPath(dep1.Path);

            // Act
            var affected = graph.FindNodesThatDependOn(dep1Node).ToList();

            // Assert
            Assert.Single(affected);
            Assert.Contains(affected, k => k.ProjectInstance.FullPath == child.Path);
        }

        [Fact]
        public void FindNodesThatDependOn_ShouldFindAnyLevelDependency()
        {
            // Arrange
            using var dep2 = CreateProjectFile("dep2");

            // dep1 depends on dep2
            using var dep1 = CreateProjectFile("dep1", dep2);

            // root depends on dep1
            using var projectThatChanged = CreateProjectFile("projectThatChanged", dep1);

            var graph = new ProjectGraph(projectThatChanged.Path);

            var dep2Node = graph.FindNodeByPath(dep2.Path);

            // Act
            var affected = graph.FindNodesThatDependOn(dep2Node).ToList();

            // Assert
            Assert.Equal(2, affected.Count);

            var dep1Node = graph.FindNodeByPath(dep1.Path);
            var projectThatChangedNode = graph.FindNodeByPath(projectThatChanged.Path);

            Assert.Collection(affected,
                k => Assert.Equal(dep1Node, k),
                k => Assert.Equal(projectThatChangedNode, k)
            );
        }

        [Fact]
        public void FindNodesThatDependOn_ShouldFindDependenciesForAllProjects()
        {
            // Arrange
            using var dep2 = CreateProjectFile("dep2");

            // dep1 depends on dep2
            using var dep1 = CreateProjectFile("dep1", dep2);

            // root depends on dep1
            using var projectThatChanged = CreateProjectFile("projectThatChanged", dep1);

            // other dep
            using var dep3 = CreateProjectFile("dep3");

            // other root depends on other dep
            using var otherProjectThatChanged = CreateProjectFile("otherProject", dep3);

            var graph = new ProjectGraph(new string[] {
                projectThatChanged.Path,
                otherProjectThatChanged.Path
            });

            var dep2Node = graph.FindNodeByPath(dep2.Path);
            var dep3Node = graph.FindNodeByPath(dep3.Path);

            // Act
            var affected = graph.FindNodesThatDependOn(new[] {
                dep2Node,
                dep3Node,
            }).ToList();

            // Assert
            Assert.Equal(3, affected.Count);

            var dep1Node = graph.FindNodeByPath(dep1.Path);
            var projectThatChangedNode = graph.FindNodeByPath(projectThatChanged.Path);
            var otherProjectThatChangedNode = graph.FindNodeByPath(otherProjectThatChanged.Path);

            Assert.Collection(affected,
                k => Assert.Equal(dep1Node, k),
                k => Assert.Equal(projectThatChangedNode, k),
                k => Assert.Equal(otherProjectThatChangedNode, k)
            );
        }

        [Fact]
        public void FindProjectsForFilePaths_ShouldFindSingleProject()
        {
            // Arrange
            using var project1 = CreateProjectFile(
                Path.Combine("somecontext", "project1", "project1")
            );

            using var project2 = CreateProjectFile(
                Path.Combine("somecontext", "project2", "project2")
            );

            var dummyFile = GetTemporaryFilePath(
               Path.Combine("somecontext", "project1", "Domain", "SomeEntity"),
               ".cs"
            );

            var graph = new ProjectGraph(new string[] { project1.Path, project2.Path });

            // Act
            var projects = graph.FindNodesContainingFiles(new[] {
                dummyFile,
            }).ToList();

            // Assert
            var project1Node = graph.FindNodeByPath(project1.Path);
            Assert.Single(projects);
            Assert.Equal(project1Node, projects.Single());
        }

        [Fact]
        public void FindProjectsForFilePaths_WithMultipleFiles_ShouldFindSingleProject()
        {
            // Arrange
            using var project1 = CreateProjectFile(
                Path.Combine("somecontext", "project1", "project1")
            );

            using var project2 = CreateProjectFile(
                Path.Combine("somecontext", "project2", "project2")
            );

            var dummyFile = GetTemporaryFilePath(
               Path.Combine("somecontext", "project1", "Domain", "SomeEntity"),
               ".cs"
            );

            var dummyFile2 = GetTemporaryFilePath(
               Path.Combine("somecontext", "project1", "Domain", "OtherEntity"),
               ".cs"
            );

            var graph = new ProjectGraph(new string[] { project1.Path, project2.Path });

            // Act
            var projects = graph.FindNodesContainingFiles(new[] {
                dummyFile,
                dummyFile2,
            }).ToList();

            // Assert
            var project1Node = graph.FindNodeByPath(project1.Path);
            Assert.Single(projects);
            Assert.Equal(project1Node, projects.Single());
        }

        [Fact]
        public void FindProjectsForFilePaths_ShouldFindMultipleProject()
        {
            // Arrange
            using var project1 = CreateProjectFile(
                Path.Combine("somecontext", "project1", "project1")
            );

            using var project2 = CreateProjectFile(
                Path.Combine("somecontext", "project2", "project2")
            );

            var dummyFile = GetTemporaryFilePath(
               Path.Combine("somecontext", "project1", "Domain", "SomeEntity"),
               ".cs"
            );

            var dummyFile2 = GetTemporaryFilePath(
               Path.Combine("somecontext", "project2", "OtherDomain", "OtherEntity"),
               ".cs"
            );

            var graph = new ProjectGraph(new string[] { project1.Path, project2.Path });

            // Act
            var projects = graph.FindNodesContainingFiles(new[] {
                dummyFile,
                dummyFile2,
            }).ToList();

            // Assert
            var project1Node = graph.FindNodeByPath(project1.Path);
            var project2Node = graph.FindNodeByPath(project2.Path);

            Assert.Collection(projects,
                p => Assert.Equal(project1Node, p),
                p => Assert.Equal(project2Node, p));
        }

        /// <summary>
        /// A path to a file that will be deleted once this class
        /// is disposed.
        /// </summary>
        private class TempFile : IDisposable
        {
            public TempFile(string path)
            {
                Path = path;
            }

            public string Path { get; }

            public void Dispose()
            {
                File.Delete(this.Path);
            }

            public override string ToString()
            {
                return this.Path;
            }
        }

        /// <summary>
        /// Gets a path to a temporary file, located in the
        /// <see cref="Path.GetTempPath"/>.
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        private static string GetTemporaryFilePath(string prefix, string extension)
        {
            var directory = Path.GetTempPath();
            Directory.CreateDirectory(directory);
            return Path.Combine(directory, $"{prefix}-{Guid.NewGuid():N}{extension}");
        }

        /// <summary>
        /// Creates an temporary MSBuild csproj file, which possible
        /// includes ProjectReferences to other projects.
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="projectReferences"></param>
        /// <returns></returns>
        private static TempFile CreateProjectFile(
            string prefix,
            params TempFile[] projectReferences)
        {
            projectReferences ??= Array.Empty<TempFile>();

            string filePath = GetTemporaryFilePath(prefix, ".csproj");

            ProjectRootElement xml = ProjectRootElement.Create();

            var itemGroup = xml.AddItemGroup();
            foreach (var projectReference in projectReferences)
            {
                itemGroup.AddItem("ProjectReference", projectReference.Path);
            }

            xml.Save(filePath);
            return new TempFile(filePath);
        }
    }
}
