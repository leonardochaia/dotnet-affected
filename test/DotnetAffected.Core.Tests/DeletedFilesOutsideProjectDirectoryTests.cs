using DotnetAffected.Testing.Utils;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace DotnetAffected.Core.Tests
{
    /// <summary>
    /// Deleted files that a project references from outside its own directory.
    ///
    /// #157 attributes a deleted file to the project whose directory contained it, which cannot
    /// help here. The overlay file system closes most of the remainder by putting deleted files
    /// back for evaluation, so globs match and items resolve as they did before the deletion.
    /// See https://github.com/leonardochaia/dotnet-affected/issues/84
    ///
    /// Each case creates a file under Shared/ referenced by a project in a sibling directory,
    /// commits, deletes only that file, and asserts the project is reported as changed.
    /// </summary>
    public class DeletedFilesOutsideProjectDirectoryTests : BaseDotnetAffectedTest
    {
        private const string ProjectName = "InventoryManagement";

        /// <summary>Control: the case #157 fixed. Expected to pass.</summary>
        [Fact]
        public async Task Case1_deleted_file_inside_project_directory()
        {
            var project = Repository.CreateCsProject(ProjectName);
            var file = Path.Combine(ProjectName, "file.cs");
            await Repository.CreateTextFileAsync(file, "// content");
            Repository.StageAndCommit();

            Repository.DeleteFile(file);

            AssertProjectChanged(project.FullPath);
        }

        /// <summary>
        /// Explicit Include of a path outside the project directory. An explicitly included item
        /// should survive deletion in the evaluated item list, so predictions may still claim it.
        /// </summary>
        [Fact]
        public async Task Case2_deleted_file_outside_project_directory_explicit_include()
        {
            var project = Repository.CreateCsProject(
                ProjectName,
                p => p.AddItem("Compile", "../Shared/Explicit.cs"));

            await Repository.CreateTextFileAsync(Path.Combine("Shared", "Explicit.cs"), "// content");
            Repository.StageAndCommit();

            Repository.DeleteFile(Path.Combine("Shared", "Explicit.cs"));

            AssertProjectChanged(project.FullPath);
        }

        /// <summary>
        /// Glob Include reaching outside the project directory. A deleted file cannot match a
        /// glob, and containment cannot reach it either.
        /// </summary>
        [Fact]
        public async Task Case3_deleted_file_outside_project_directory_via_glob()
        {
            var project = Repository.CreateCsProject(
                ProjectName,
                p => p.AddItem("Compile", @"../Shared/**/*.cs"));

            await Repository.CreateTextFileAsync(Path.Combine("Shared", "Globbed.cs"), "// content");
            Repository.StageAndCommit();

            Repository.DeleteFile(Path.Combine("Shared", "Globbed.cs"));

            AssertProjectChanged(project.FullPath);
        }

        /// <summary>
        /// Unconditional Import of a props outside the project directory.
        ///
        /// Without the deleted import registered up front, MSBuild cannot evaluate the project
        /// at all and graph construction throws. Nothing asks whether the file exists here, so
        /// the registration has to happen eagerly rather than on a FileExists call.
        /// </summary>
        [Fact]
        public async Task Case4_deleted_props_imported_unconditionally()
        {
            await Repository.CreateTextFileAsync(Path.Combine("Shared", "Common.props"),
                "<Project><PropertyGroup><FromShared>yes</FromShared></PropertyGroup></Project>");

            var project = Repository.CreateCsProject(
                ProjectName,
                p => p.AddImport("../Shared/Common.props"));

            Repository.StageAndCommit();

            Repository.DeleteFile(Path.Combine("Shared", "Common.props"));

            AssertProjectChanged(project.FullPath);
        }

        /// <summary>
        /// Import guarded by Exists(). Without the overlay MSBuild skips it silently once the
        /// file is gone, so nothing in the evaluated project records that it ever existed and
        /// the project is never marked as changed.
        /// </summary>
        [Fact]
        public async Task Case5_deleted_props_imported_conditionally()
        {
            await Repository.CreateTextFileAsync(Path.Combine("Shared", "Optional.props"),
                "<Project><PropertyGroup><FromShared>yes</FromShared></PropertyGroup></Project>");

            var project = Repository.CreateCsProject(
                ProjectName,
                p =>
                {
                    var import = p.AddImport("../Shared/Optional.props");
                    import.Condition = "Exists('../Shared/Optional.props')";
                });

            Repository.StageAndCommit();

            Repository.DeleteFile(Path.Combine("Shared", "Optional.props"));

            AssertProjectChanged(project.FullPath);
        }

        /// <summary>Linked content file outside the project directory.</summary>
        [Fact]
        public async Task Case6_deleted_linked_content_outside_project_directory()
        {
            var project = Repository.CreateCsProject(
                ProjectName,
                p =>
                {
                    var item = p.AddItem("None", "../Shared/data.json");
                    item.AddMetadata("Link", "data.json");
                });

            await Repository.CreateTextFileAsync(Path.Combine("Shared", "data.json"), "{}");
            Repository.StageAndCommit();

            Repository.DeleteFile(Path.Combine("Shared", "data.json"));

            AssertProjectChanged(project.FullPath);
        }

        private void AssertProjectChanged(string expectedProjectPath)
        {
            Assert.Single(AffectedSummary.FilesThatChanged);

            var projectInfo = Assert.Single(AffectedSummary.ProjectsWithChangedFiles);
            Assert.Equal(expectedProjectPath, projectInfo.GetFullPath());
        }
    }
}
