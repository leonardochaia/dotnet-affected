using DotnetAffected.Testing.Utils;
using System.Threading.Tasks;
using Xunit;

namespace DotnetAffected.Core.Tests
{
    /// <summary>
    /// Tests for paths that live inside the working directory but are not repository content.
    ///
    /// The git file system decided what belongs to a commit by path prefix alone, so anything
    /// sitting inside the working directory without being tracked resolved to "missing".
    /// The SDK is regularly installed there on CI runners, and MSBuild guards its own imports
    /// with Exists(), so those imports were silently skipped and SDK properties went undefined.
    /// </summary>
    public class GitIgnoredPathsDetectionTests : BaseDotnetAffectedTest
    {
        /// <summary>
        /// Regression test for https://github.com/leonardochaia/dotnet-affected/issues/154
        ///
        /// Mirrors how the SDK fails without needing a real SDK inside the repository: an
        /// ignored directory holding a props file, imported under Exists(), whose property is
        /// then used by a property function that only works when the import actually happened.
        /// That is exactly the shape of `[MSBuild]::Add($(NETCoreAppMaximumVersion), 1)` in
        /// Microsoft.NET.SupportedTargetFrameworks.props.
        /// </summary>
        [Fact]
        public async Task When_an_ignored_directory_holds_an_imported_props_packages_should_still_be_diffed()
        {
            await Repository.CreateTextFileAsync(".gitignore", "localsdk/\n");

            // Ignored, so it never reaches the commit and can only be served from disk.
            await Repository.CreateTextFileAsync("localsdk/Versions.props", @"
<Project>
    <PropertyGroup>
        <LocalSdkVersion>10</LocalSdkVersion>
    </PropertyGroup>
</Project>
");

            await Repository.CreateTextFileAsync("Directory.Build.props", @"
<Project>
    <Import Project=""$(MSBuildThisFileDirectory)localsdk/Versions.props""
            Condition=""Exists('$(MSBuildThisFileDirectory)localsdk/Versions.props')"" />
    <PropertyGroup>
        <NextLocalSdkVersion>$([MSBuild]::Add($(LocalSdkVersion), 1))</NextLocalSdkVersion>
    </PropertyGroup>
</Project>
");

            var packageName = "Some.Library";
            Repository.CreateDirectoryPackageProps(
                b => b.AddPackageVersion(packageName, "1.0.0"));

            var msBuildProject = Repository.CreateCsProject(
                "Lib",
                b => b.AddNuGetDependency(packageName));

            Repository.StageAndCommit();

            Repository.UpdateDirectoryPackageProps(
                b => b.UpdatePackageVersion(packageName, "2.0.0"));

            // Without the import, LocalSdkVersion is empty and [MSBuild]::Add throws
            // "Method '[MSBuild]::Add' not found" while evaluating the project from the commit.
            Assert.Single(AffectedSummary.ChangedPackages);

            var projectInfo = Assert.Single(AffectedSummary.AffectedProjects);
            Assert.Equal(msBuildProject.FullPath, projectInfo.GetFullPath());
        }
    }
}
