using DotnetAffected.Testing.Utils;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DotnetAffected.Core.Tests
{
    /// <summary>
    /// Tests for the eager loading file system used when diffing NuGet packages.
    ///
    /// Loading a project from a commit goes through
    /// <see cref="DotnetAffected.Core.FileSystem.EagerCachingMsBuildGitFileSystem"/>, which
    /// eagerly loads every file MSBuild probes for. MSBuild probes for plenty of files that
    /// are not MSBuild projects, so the eager loader must not try to parse them.
    /// </summary>
    public class EagerCachingFileSystemTests : BaseDotnetAffectedTest
    {
        /// <summary>
        /// Regression test for https://github.com/leonardochaia/dotnet-affected/issues/155
        ///
        /// `GetDirectoryNameOfFileAbove` probes each directory above the project for the
        /// .slnx. The eager loader answers that probe by parsing the .slnx as an MSBuild
        /// project, which fails with "The element &lt;Solution&gt; is unrecognized".
        /// </summary>
        [Fact]
        public async Task When_directory_build_props_probes_for_a_slnx_packages_should_still_be_diffed()
        {
            var projectName = "Lib";

            await Repository.CreateXmlSolutionAsync("repro.slnx", $"{projectName}/{projectName}.csproj");

            await Repository.CreateTextFileAsync("Directory.Build.props", @"
<Project>
    <PropertyGroup>
        <SolutionRoot>$([MSBuild]::GetDirectoryNameOfFileAbove($(MSBuildProjectDirectory), 'repro.slnx'))</SolutionRoot>
    </PropertyGroup>
</Project>
");

            var packageName = "Some.Library";
            Repository.CreateDirectoryPackageProps(
                b => b.AddPackageVersion(packageName, "1.0.0"));

            var msBuildProject = Repository.CreateCsProject(
                projectName,
                b => b.AddNuGetDependency(packageName));

            Repository.StageAndCommit();

            Repository.UpdateDirectoryPackageProps(
                b => b.UpdatePackageVersion(packageName, "2.0.0"));

            // Must not throw InvalidProjectFileException.
            Assert.Single(AffectedSummary.ChangedPackages);

            var projectInfo = Assert.Single(AffectedSummary.AffectedProjects);
            Assert.Equal(msBuildProject.FullPath, projectInfo.GetFullPath());
        }

        /// <summary>
        /// Same defect as the .slnx case, but for a file that is not XML at all, which fails
        /// while parsing rather than on an unexpected root element.
        ///
        /// Note that a global.json is normally read while resolving the SDK, which happens
        /// outside the evaluation file system and so never reaches the eager loader. It only
        /// gets there when a project explicitly probes for it, as below.
        /// </summary>
        [Fact]
        public async Task When_directory_build_props_probes_for_a_global_json_packages_should_still_be_diffed()
        {
            await Repository.CreateTextFileAsync("global.json", @"{
    ""sdk"": {
        ""version"": ""10.0.201"",
        ""rollForward"": ""latestMinor""
    }
}");

            await Repository.CreateTextFileAsync("Directory.Build.props", @"
<Project>
    <PropertyGroup>
        <RepoRoot>$([MSBuild]::GetDirectoryNameOfFileAbove($(MSBuildProjectDirectory), 'global.json'))</RepoRoot>
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

            Assert.Single(AffectedSummary.ChangedPackages);

            var projectInfo = Assert.Single(AffectedSummary.AffectedProjects);
            Assert.Equal(msBuildProject.FullPath, projectInfo.GetFullPath());
        }
    }
}
