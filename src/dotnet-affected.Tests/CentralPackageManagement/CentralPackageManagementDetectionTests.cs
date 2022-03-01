using System.Linq;
using Xunit;

namespace Affected.Cli.Tests
{
    /// <summary>
    /// Tests for detecting affected projects when central package management changes
    /// </summary>
    public class CentralPackageManagementDetectionTests
        : BaseServiceProviderRepositoryTest
    {
        [Fact]
        public void When_directory_packages_props_changes_dependant_projects_should_be_affected()
        {
            // Create a Directory.Package.props
            var packageName = "Some.Library";
            Repository.CreateDirectoryPackageProps(
                b => b.AddPackageVersion(packageName, "1.0.0"));

            // Create a project with a nuget dependency
            var projectName = "InventoryManagement";
            var msBuildProject = Repository.CreateCsProject(
                projectName,
                b => b.AddNuGetDependency(packageName));

            // Commit so there are no changes
            Repository.StageAndCommit();

            // Update package to newer version
            Repository.UpdateDirectoryPackageProps(
                b => b.UpdatePackageVersion(packageName, "v2.0.0"));

            Assert.Single(Context.ChangedProjects);
            Assert.Empty(Context.AffectedProjects);

            var projectInfo = Context.ChangedProjects.Single();
            Assert.Equal(projectName, projectInfo.Name);
            Assert.Equal(msBuildProject.FullPath, projectInfo.FilePath);
        }

        [Fact]
        public void When_directory_packages_props_changes_without_dependant_projects_should_throw()
        {
            // Create a Directory.Package.props
            var packageName = "Some.Library";
            var otherPackageName = "Some.Other.Library";
            Repository.CreateDirectoryPackageProps(
                b => b
                    .AddPackageVersion(packageName, "1.0.0")
                    .AddPackageVersion(otherPackageName, "v1.0.0"));

            // Create a project with a nuget dependency to package 1
            var projectName = "InventoryManagement";
            Repository.CreateCsProject(
                projectName,
                b => b.AddNuGetDependency(packageName));

            // Commit so there are no changes
            Repository.StageAndCommit();

            // Update package 2 to newer version
            Repository.UpdateDirectoryPackageProps(
                b => b.UpdatePackageVersion(otherPackageName, "v2.0.0"));

            Assert.Throws<NoChangesException>(() => Context.ChangedProjects);
        }

        [Fact]
        public void Should_ignore_projects_opted_out_of_central_package_management()
        {
            // Create a Directory.Package.props
            var packageName = "Some.Library";
            Repository.CreateDirectoryPackageProps(
                b => b.AddPackageVersion(packageName, "1.0.0"));

            // Create a project with a nuget dependency but no central package management
            var projectName = "InventoryManagement";
            Repository.CreateCsProject(
                projectName,
                b => b
                    .AddNuGetDependency(packageName, "v2.0.0")
                    .OptOutFromCentrallyManagedNuGetPackageVersions());

            // Commit so there are no changes
            Repository.StageAndCommit();

            // Update package to newer version
            Repository.UpdateDirectoryPackageProps(
                b => b.UpdatePackageVersion(packageName, "v2.0.0"));

            Assert.Throws<NoChangesException>(() => Context.ChangedProjects);
        }
    }
}
