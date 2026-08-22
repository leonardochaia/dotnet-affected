using DotnetAffected.Testing.Utils;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DotnetAffected.Core.Tests
{
    /// <summary>
    /// Moving a file from one project to another.
    ///
    /// Git reports a move as a single rename entry when rename detection is on, which carries
    /// only the new path. The project the file left keeps compiling a smaller set of files and
    /// nothing attributes the change to it, so it is never reported as changed and the old path
    /// never reaches the deleted files overlay either.
    /// </summary>
    public class MovedFileChangeDetectionTests
        : BaseDotnetAffectedTest
    {
        private const string SourceProject = "InventoryManagement";
        private const string TargetProject = "OrderManagement";

        private string _fromCommit;

        protected override AffectedOptions Options =>
            new AffectedOptions(Repository.Path, fromRef: _fromCommit);

        /// <summary>
        /// The move is committed, so the comparison is between two trees.
        /// </summary>
        [Fact]
        public async Task When_file_is_moved_between_projects_both_projects_should_have_changes()
        {
            var (source, target) = await CreateProjectsWithMovableFile();

            _fromCommit = Repository.StageAndCommit()
                .Sha;

            MoveFile();
            Repository.StageAndCommit();

            AssertBothProjectsChanged(source, target);
        }

        /// <summary>
        /// The move is staged but not committed, so the comparison is against the working tree.
        /// </summary>
        [Fact]
        public async Task When_staged_file_is_moved_between_projects_both_projects_should_have_changes()
        {
            var (source, target) = await CreateProjectsWithMovableFile();

            _fromCommit = Repository.StageAndCommit()
                .Sha;

            MoveFile();
            Repository.StageAll();

            AssertBothProjectsChanged(source, target);
        }

        private async Task<(string SourcePath, string TargetPath)> CreateProjectsWithMovableFile()
        {
            var source = Repository.CreateCsProject(SourceProject);
            var target = Repository.CreateCsProject(TargetProject);

            // Long enough for git to consider the two paths similar to each other.
            await Repository.CreateTextFileAsync(
                Path.Combine(SourceProject, "Moved.cs"),
                string.Join("\n", Enumerable.Range(0, 50).Select(i => $"// line {i}")));

            return (source.FullPath, target.FullPath);
        }

        private void MoveFile()
        {
            File.Move(
                Path.Combine(Repository.Path, SourceProject, "Moved.cs"),
                Path.Combine(Repository.Path, TargetProject, "Moved.cs"));
        }

        private void AssertBothProjectsChanged(string sourceProjectPath, string targetProjectPath)
        {
            // Both the path the file left and the path it arrived at are changes.
            Assert.Equal(2, AffectedSummary.FilesThatChanged.Count());

            var changedProjects = AffectedSummary.ProjectsWithChangedFiles
                .Select(p => p.GetFullPath())
                .OrderBy(p => p)
                .ToArray();

            Assert.Equal(
                new[] { sourceProjectPath, targetProjectPath }.OrderBy(p => p)
                    .ToArray(),
                changedProjects);
        }
    }
}
