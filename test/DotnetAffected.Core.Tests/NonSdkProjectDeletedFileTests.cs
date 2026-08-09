using DotnetAffected.Testing.Utils;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace DotnetAffected.Core.Tests
{
    /// <summary>
    /// Deleted files in projects that have no default globs.
    ///
    /// SDK style projects glob their whole directory, so almost any deleted file is still an
    /// item and gets attributed. Non SDK projects declare their inputs one by one, which makes
    /// them the clearest way to pin down what attribution does and does not cover.
    /// </summary>
    public class NonSdkProjectDeletedFileTests : BaseDotnetAffectedTest
    {
        private const string ProjectName = "LegacyInventory";

        /// <summary>
        /// A declared item keeps its place in the evaluated list whether or not the file is on
        /// disk, so the project is attributed the deletion.
        /// </summary>
        [Fact]
        public async Task When_a_declared_file_is_deleted_project_should_have_changes()
        {
            var project = Repository.CreateNonSdkMsBuildProject(ProjectName, ".csproj");

            await Repository.CreateTextFileAsync(Path.Combine(ProjectName, "file.cs"), "// content");
            Repository.StageAndCommit();

            Repository.DeleteFile(Path.Combine(ProjectName, "file.cs"));

            Assert.Single(AffectedSummary.FilesThatChanged);

            var projectInfo = Assert.Single(AffectedSummary.ProjectsWithChangedFiles);
            Assert.Equal(project.FullPath, projectInfo.GetFullPath());
        }

        /// <summary>
        /// A file no item ever referenced is not a build input, so deleting it cannot change
        /// what the project produces and the project is deliberately left alone.
        ///
        /// Attribution follows what the project declares. Merely sitting in the project's
        /// directory does not make a file an input, so nothing is inferred from its location.
        /// </summary>
        [Fact]
        public async Task When_an_undeclared_file_is_deleted_project_should_not_have_changes()
        {
            Repository.CreateNonSdkMsBuildProject(ProjectName, ".csproj");

            await Repository.CreateTextFileAsync(Path.Combine(ProjectName, "file.cs"), "// content");
            await Repository.CreateTextFileAsync(Path.Combine(ProjectName, "notes.txt"), "notes");
            Repository.StageAndCommit();

            Repository.DeleteFile(Path.Combine(ProjectName, "notes.txt"));

            Assert.Single(AffectedSummary.FilesThatChanged);
            Assert.Empty(AffectedSummary.ProjectsWithChangedFiles);
            Assert.Empty(AffectedSummary.AffectedProjects);
        }
    }
}
