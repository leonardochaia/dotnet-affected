﻿using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        public void When_package_is_updated_dependant_projects_should_be_affected()
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

            Assert.Single(Context.ChangedFiles);
            Assert.Single(Context.ChangedNuGetPackages);
            Assert.Single(Context.AffectedProjects);

            var projectInfo = Context.AffectedProjects.Single();
            Assert.Equal(projectName, projectInfo.Name);
            Assert.Equal(msBuildProject.FullPath, projectInfo.FilePath);
        }

        [Fact]
        public void When_packages_changes_dependencies_of_affected_projects_should_also_be_affected()
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

            // Create a project that depends on the first project
            var dependantProjectName = "InventoryManagement.Tests";
            var dependantMsBuildProject = Repository.CreateCsProject(
                dependantProjectName,
                b => b.AddProjectDependency(msBuildProject.FullPath));

            // Commit so there are no changes
            Repository.StageAndCommit();

            // Update package to newer version
            Repository.UpdateDirectoryPackageProps(
                b => b.UpdatePackageVersion(packageName, "v2.0.0"));

            Assert.Single(Context.ChangedFiles);
            Assert.Single(Context.ChangedNuGetPackages);
            Assert.Equal(2, Context.AffectedProjects.Count());

            var projectInfo = Context.AffectedProjects.FirstOrDefault();
            Assert.Equal(dependantProjectName, projectInfo.Name);
            Assert.Equal(dependantMsBuildProject.FullPath, projectInfo.FilePath);

            var dependantProjectInfo = Context.AffectedProjects.ElementAt(1);
            Assert.Equal(projectName, dependantProjectInfo.Name);
            Assert.Equal(msBuildProject.FullPath, dependantProjectInfo.FilePath);
        }

        [Fact]
        public void When_package_is_deleted_dependant_projects_should_be_affected()
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

            // Delete package
            Repository.UpdateDirectoryPackageProps(
                b => b.UpdatePackageVersion(packageName, null));

            Assert.Single(Context.ChangedFiles);
            Assert.Single(Context.ChangedNuGetPackages);
            Assert.Single(Context.AffectedProjects);

            var projectInfo = Context.AffectedProjects.Single();
            Assert.Equal(projectName, projectInfo.Name);
            Assert.Equal(msBuildProject.FullPath, projectInfo.FilePath);
        }

        [Fact]
        public void When_directory_packages_props_changes_without_dependant_projects_should_throw()
        {
            // Create a Directory.Package.props
            var packageName = "Some.Library";
            Repository.CreateDirectoryPackageProps(
                b => b
                    .AddPackageVersion(packageName, "1.0.0"));

            // Create a project with a nuget dependency to package 1
            var projectName = "InventoryManagement";
            Repository.CreateCsProject(
                projectName,
                b => b.AddNuGetDependency(packageName));

            // Commit so there are no changes
            Repository.StageAndCommit();

            // Update package 2 to newer version
            var otherPackageName = "Other.Library";
            Repository.UpdateDirectoryPackageProps(
                b => b.AddPackageVersion(otherPackageName, "v2.0.0"));

            Assert.Single(Context.ChangedNuGetPackages);
            Assert.Throws<NoChangesException>(() => Context.AffectedProjects);
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

            Assert.Throws<NoChangesException>(() => Context.AffectedProjects);
        }

        [Fact]
        public async Task With_conditional_props_file_projects_should_still_be_affected()
        {
            // Create a Directory.Packages.props file with conditions
            var propsFile = @"
<Project>
    <ItemGroup Condition=""'$(TargetFramework)' == 'net5.0'"">
        <PackageVersion Include=""Some.Library"" Version=""5.0.0"" />
    </ItemGroup>

    <ItemGroup Condition=""'$(TargetFramework)' == 'net6.0'"">
        <PackageVersion Include=""Other.Library"" Version=""6.0.0"" />
    </ItemGroup>
</Project>
";
            var propsPath = Path.Combine(Repository.Path, "Directory.Packages.props");
            await Repository.CreateTextFileAsync(propsPath, propsFile);

            // Create a project with a nuget dependency
            var projectName = "InventoryManagement";
            Repository.CreateCsProject(
                projectName,
                b => b.AddNuGetDependency("Some.Library"));

            // Commit all so there are no changes
            Repository.StageAndCommit();

            // Update versions in props file
            propsFile = @"
<Project>
    <ItemGroup Condition=""'$(TargetFramework)' == 'net5.0'"">
        <PackageVersion Include=""Some.Library"" Version=""5.1.0"" />
    </ItemGroup>

    <ItemGroup Condition=""'$(TargetFramework)' == 'net6.0'"">
        <PackageVersion Include=""Other.Library"" Version=""6.1.0"" />
    </ItemGroup>
</Project>
";
            await Repository.CreateTextFileAsync(propsPath, propsFile);

            Assert.Equal(2, Context.ChangedNuGetPackages.Count());
            Assert.Single(Context.AffectedProjects);
        }
        
        [Fact]
        public void When_directory_packages_props_is_added_dependant_projects_should_be_affected()
        {
            var packageName = "Some.Library";
            
            // Create a project with a nuget dependency
            var projectName = "InventoryManagement";
            var msBuildProject = Repository.CreateCsProject(
                projectName,
                b => b.AddNuGetDependency(packageName));

            // Commit so there are no changes
            Repository.StageAndCommit();

            // Create a Directory.Package.props
            Repository.CreateDirectoryPackageProps(
                b => b.AddPackageVersion(packageName, "1.0.0"));

            Assert.Single(Context.ChangedFiles);
            Assert.Single(Context.ChangedNuGetPackages);
            Assert.Single(Context.AffectedProjects);

            var projectInfo = Context.AffectedProjects.Single();
            Assert.Equal(projectName, projectInfo.Name);
            Assert.Equal(msBuildProject.FullPath, projectInfo.FilePath);
        }
        
        [Fact]
        public void When_directory_packages_props_is_removed_dependant_projects_should_be_affected()
        {
            var packageName = "Some.Library";
            // Create a Directory.Package.props
            Repository.CreateDirectoryPackageProps(
                b => b.AddPackageVersion(packageName, "1.0.0"));
            
            // Create a project with a nuget dependency
            var projectName = "InventoryManagement";
            var msBuildProject = Repository.CreateCsProject(
                projectName,
                b => b.AddNuGetDependency(packageName));

            // Commit so there are no changes
            Repository.StageAndCommit();

            Repository.RemoveDirectoryPackageProps();
            
            Assert.Single(Context.ChangedFiles);
            Assert.Single(Context.ChangedNuGetPackages);
            Assert.Single(Context.AffectedProjects);

            var projectInfo = Context.AffectedProjects.Single();
            Assert.Equal(projectName, projectInfo.Name);
            Assert.Equal(msBuildProject.FullPath, projectInfo.FilePath);
        }
    }
}
