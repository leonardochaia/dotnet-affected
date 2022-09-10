using DotnetAffected.Testing.Utils;
using System.Linq;
using Xunit;

namespace DotnetAffected.Core.Tests
{
    
    /// <summary>
    /// Tests for detecting affected projects when central package management changes with nested package files
    /// </summary>
    public class CentralPackageManagementDetectionNestedTests : BaseDotnetAffectedTest
    {

        /// <summary>
        /// This test demonstrate the main difference when nest project evaluation is supported in DPP. <br/>
        ///
        /// <example>
        /// <list type="bullet">
        ///   <listheader><b>Initial FileSystem state (committed to git):</b></listheader>
        ///   <item>Directory.Packages.props<br/>
        ///     - Other.Library @ 2.5.0
        ///   </item>
        ///   <item>InventoryManagement/Directory.Packages.props<br/>
        ///     - Some.Library  @ 3.0.0
        ///   </item>
        ///   <item>InventoryManagement/InventoryManagement.csproj</item>
        /// </list>
        /// 
        /// <list type="bullet">
        ///   <listheader><b>Updating files (working directory):</b></listheader>
        ///   <item>Directory.Packages.props<br/>
        ///     - Some.Library  @ 2.0.0
        ///   </item>
        ///   <item>InventoryManagement/Directory.Packages.props<br/>
        ///     - Other.Library @ 3.5.0
        ///   </item>
        /// </list>
        ///  
        ///  In words, we've swapped the packages between the <b>Directory.Packages.props</b> files and changed the versions. <para/>
        ///
        /// <list type="bullet">
        ///   <listheader><b>Real analysis (i.e. with project evaluation):</b></listheader>
        ///   <item>Some.Library   PREV: 3.0.0 <b>CURR</b>: 2.0.0</item>
        ///   <item>Other.Library  PREV: 2.5.0 <b>CURR</b>: 3.5.0</item>
        /// </list>
        /// 
        /// <list type="bullet">
        ///   <listheader><b>Old analysis (i.e. NO project evaluation):</b></listheader>
        ///   <item>Some.Library   PREV: 3.0.0 <b>CURR</b>: 2.0.0</item>
        ///   <item>Other.Library  PREV: NONE <b>CURR</b>: 3.5.0</item>
        /// </list>
        /// This is what we get when project evaluation is supported.<para/>
        ///
        /// Putting the history mismatches aside, the actual current package resolution fails! <br/>
        /// It appears to bring the right values, but no! <br/>
        /// It will first load the file `InventoryManagement/Directory.Packages.props` from the commit, with the right
        /// value for `Other.Library` @ 3.50 however, for `Some.Library` it will just load the file from the file system,
        /// not the commit. By luck it's the right value but if we we're to find the changeset between 2 commits and not between the working dir and HEAD, it would
        /// have given us the wrong value!
        /// </example>
        /// </summary>
        [Fact]
        public void When_directory_packages_props_updates_dependant_projects_should_be_affected()
        {
            // TODO: Extend this test to find the changeset between 2 commits and not WorkDir <-> HEAD

            // Create a Directory.Package.props
            var packageName = "Some.Library";
            var otherPackageName = "Other.Library";
            Repository
                .CreateDirectoryPackageProps(b => b.AddPackageVersion(otherPackageName, "2.5.0"));

            // Create a project with a nuget dependency
            var projectName = "InventoryManagement";
            var msBuildProject = Repository.CreateCsProject(
                projectName,
                b => b.AddNuGetDependency(packageName));

            msBuildProject
                .CreateDirectoryPackageProps(true, b => b.AddPackageVersion(packageName, "3.0.0"));

            // Commit so there are no changes
            Repository.StageAndCommit();

            msBuildProject.RemoveDirectoryPackageProps();
            msBuildProject
                .CreateDirectoryPackageProps(true, b => b.AddPackageVersion(otherPackageName, "3.5.0"));
            
            Repository.RemoveDirectoryPackageProps();
            Repository.CreateDirectoryPackageProps(b => b.AddPackageVersion(packageName, "2.0.0"));

            Assert.Equal(2, AffectedSummary.ChangedPackages.Length);
            Assert.Single(AffectedSummary.AffectedProjects);
            
            var someLibChanges = AffectedSummary.ChangedPackages.Single(c => c.Name == packageName);
            var otherLibChanges = AffectedSummary.ChangedPackages.Single(c => c.Name == otherPackageName);
            
#if (NET5_0_OR_GREATER)
            Assert.Equal("3.0.0", someLibChanges.OldVersions.Single());
            Assert.Equal("2.5.0", otherLibChanges.OldVersions.Single());

            Assert.Equal("2.0.0", someLibChanges.NewVersions.Single());
            Assert.Equal("3.5.0", otherLibChanges.NewVersions.Single());
#else
            Assert.Equal("3.0.0", someLibChanges.OldVersions.Single());
            Assert.Empty(otherLibChanges.OldVersions);

            Assert.Equal("2.0.0", someLibChanges.NewVersions.Single());
            Assert.Equal("3.5.0", otherLibChanges.NewVersions.Single());
#endif
        }

#if (NET5_0_OR_GREATER)

        [Fact]
        public void When_directory_packages_props_changes_without_dependant_projects_nothing_should_be_affected()
        {
            // Create a Directory.Package.props
            var packageName = "Some.Library";
            Repository
                .CreateDirectoryPackageProps(b => b.AddPackageVersion(packageName, "2.0.0"));

            // Create a project with a nuget dependency
            var projectName = "InventoryManagement";
            var msBuildProject = Repository.CreateCsProject(
                projectName,
                b => b.AddNuGetDependency(packageName));

            msBuildProject
                .CreateDirectoryPackageProps(true, b => b.AddPackageVersion(packageName, "1.0.0"));

            // Commit so there are no changes
            Repository.StageAndCommit();

            var otherPackageName = "Other.Library";
            Repository.UpdateDirectoryPackageProps(
                b => b.AddPackageVersion(otherPackageName, "2.0.0"));

            Assert.Single(AffectedSummary.ChangedPackages);
            Assert.Empty(AffectedSummary.AffectedProjects);
        }

        [Fact]
        public void With_nested_conditional_props_file_projects_should_still_be_affected()
        {
            var packageName = "Some.Library";
            Repository
                .CreateDirectoryPackageProps(b =>
                {
                    var itemGroup = b.AddItemGroup();
                    itemGroup.Condition = "'$(TargetFramework)' == 'net5.0'";
                    var item = itemGroup.AddItem("PackageVersion", packageName);
                    item.AddMetadata("Version", "5.0.0", expressAsAttribute: true);
                    
                    itemGroup = b.AddItemGroup();
                    itemGroup.Condition = "'$(TargetFramework)' == 'net6.0'";
                    item = itemGroup.AddItem("PackageVersion", packageName);
                    item.AddMetadata("Version", "6.0.0", expressAsAttribute: true);
                });
            
            // Create a project with a nuget dependency
            var projectName = "InventoryManagement";
            var msBuildProject = Repository.CreateCsProject(
                projectName,
                b => b.AddNuGetDependency(packageName));

            // Commit all so there are no changes
            Repository.StageAndCommit();

            msBuildProject
                .CreateDirectoryPackageProps(true, b =>
                {
                    var itemGroup = b.AddItemGroup();
                    itemGroup.Condition = "'$(TargetFramework)' == 'net5.0'";
                    var item = itemGroup.AddItem("PackageVersion", packageName);
                    item.AddMetadata("Version", "5.1.0", expressAsAttribute: true);
                    
                    itemGroup = b.AddItemGroup();
                    itemGroup.Condition = "'$(TargetFramework)' == 'net6.0'";
                    item = itemGroup.AddItem("PackageVersion", packageName);
                    item.AddMetadata("Version", "6.1.0", expressAsAttribute: true);
                });


            Assert.Single(AffectedSummary.ChangedPackages);
            Assert.Single(AffectedSummary.AffectedProjects);

            var changedPackage = AffectedSummary.ChangedPackages.Single();
            Assert.Equal(changedPackage.Name, packageName);
            Assert.Equal(2, changedPackage.OldVersions.Count);
            Assert.Equal(2, changedPackage.NewVersions.Count);
            Assert.Equal(changedPackage.OldVersions, new []{"5.0.0", "6.0.0"});
            Assert.Equal(changedPackage.NewVersions, new []{"5.1.0", "6.1.0"});            
        }

        [Fact]
        public void When_nested_package_is_added_dependant_projects_should_be_affected()
        {
            // Create a Directory.Package.props
            var packageName = "Some.Library";
            Repository
                .CreateDirectoryPackageProps(b => b.AddPackageVersion(packageName, "2.0.0"));

            // Create a project with a nuget dependency
            var projectName = "InventoryManagement";
            var msBuildProject = Repository.CreateCsProject(
                projectName,
                b => b.AddNuGetDependency(packageName));

            // Commit so there are no changes
            Repository.StageAndCommit();

            msBuildProject
                .CreateDirectoryPackageProps(true, b => b.AddPackageVersion(packageName, "1.0.0"));

            Assert.Single(AffectedSummary.FilesThatChanged);
            Assert.Single(AffectedSummary.ChangedPackages);
            Assert.Single(AffectedSummary.AffectedProjects);

            var projectInfo = AffectedSummary.AffectedProjects.Single();
            Assert.Equal(projectName, projectInfo.GetProjectName());
            Assert.Equal(msBuildProject.FullPath, projectInfo.GetFullPath());
            
            var changedPackage = AffectedSummary.ChangedPackages.Single();
            Assert.Equal(changedPackage.Name, packageName);
            Assert.Equal(changedPackage.OldVersions.Single(), "2.0.0");
            Assert.Equal(changedPackage.NewVersions.Single(), "1.0.0");
        }

        [Fact]
        public void When_directory_packages_props_is_removed_dependant_projects_should_be_affected()
        {
            // Create a Directory.Package.props
            var packageName = "Some.Library";
            Repository
                .CreateDirectoryPackageProps(b => b.AddPackageVersion(packageName, "1.0.0"));

            // Create a project with a nuget dependency
            var projectName = "InventoryManagement";
            var msBuildProject = Repository.CreateCsProject(
                projectName,
                b => b.AddNuGetDependency(packageName));

            msBuildProject
                .CreateDirectoryPackageProps(true, b => b.AddPackageVersion(packageName, "2.0.0"));

            // Commit so there are no changes
            Repository.StageAndCommit();

            msBuildProject.RemoveDirectoryPackageProps();

            Assert.Single(AffectedSummary.FilesThatChanged);
            Assert.Single(AffectedSummary.ChangedPackages);
            Assert.Single(AffectedSummary.AffectedProjects);

            var projectInfo = AffectedSummary.AffectedProjects.Single();
            Assert.Equal(projectName, projectInfo.GetProjectName());
            Assert.Equal(msBuildProject.FullPath, projectInfo.GetFullPath());

            var changedPackage = AffectedSummary.ChangedPackages.Single();
            Assert.Equal(changedPackage.Name, packageName);
            Assert.Equal(changedPackage.OldVersions.Single(), "2.0.0");
            Assert.Equal(changedPackage.NewVersions.Single(), "1.0.0");
        }
        
        [Fact]
        public void With_nested_conditional_props_file_projects_should_still_be_affected1()
        {
            // Create a Directory.Package.props
            var packageName = "Some.Library";
            Repository
                .CreateDirectoryPackageProps(b => b.AddPackageVersion(packageName, "2.0.0"));

            // Create a project with a nuget dependency
            var projectName = "InventoryManagement";
            var msBuildProject = Repository.CreateCsProject(
                projectName,
                b => b.AddNuGetDependency(packageName));

            msBuildProject
                .CreateDirectoryPackageProps(true, b => b.AddPackageVersion(packageName, "1.0.0"));
    
            // Commit so there are no changes
            Repository.StageAndCommit();

            Repository.UpdateDirectoryPackageProps(
                b => b.AddPackageVersion(packageName, "2.1.0"));

            msBuildProject
                .UpdateDirectoryPackageProps(b => b.AddPackageVersion(packageName, "1.1.0"));

            Assert.Equal(2, AffectedSummary.FilesThatChanged.Length);
            Assert.Single(AffectedSummary.ChangedPackages);
            Assert.Single(AffectedSummary.AffectedProjects);

            var projectInfo = AffectedSummary.AffectedProjects.Single();
            Assert.Equal(projectName, projectInfo.GetProjectName());
            Assert.Equal(msBuildProject.FullPath, projectInfo.GetFullPath());
            
            var changedPackage = AffectedSummary.ChangedPackages.Single();
            Assert.Equal(packageName, changedPackage.Name);
            Assert.Equal("1.0.0", changedPackage.OldVersions.Single());
            Assert.Equal("1.1.0", changedPackage.NewVersions.Single());      
        }

#endif // NET5_0_OR_GREATER

    }
}

