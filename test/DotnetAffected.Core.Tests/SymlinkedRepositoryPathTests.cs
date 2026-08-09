using DotnetAffected.Testing.Utils;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DotnetAffected.Core.Tests
{
    /// <summary>
    /// Detection when the repository is reached through a symlink, so that the path given to the
    /// tool and the path git reports are the same place spelled differently.
    ///
    /// Anything comparing the two spellings, or deriving a repository relative path from the wrong
    /// one, silently finds nothing: no blob matches, so a deleted file is never restored and a file
    /// inside the repository looks like it sits outside of it. Silence is the whole problem, since
    /// reporting nothing as affected is indistinguishable from a correct answer.
    ///
    /// This is the normal state of affairs for a temp directory on macOS, where /var is a symlink to
    /// /private/var, which is how these bugs first showed up.
    /// </summary>
    public class SymlinkedRepositoryPathTests : BaseRepositoryTest
    {
        private const string ProjectName = "InventoryManagement";

        /// <summary>
        /// Creating a symlink needs a privilege that is not granted by default on Windows, so the
        /// test yields rather than failing for an unrelated reason.
        /// </summary>
        private static string TryCreateLinkTo(string target)
        {
            var link = Path.Combine(Path.GetTempPath(), $"Symlink{Guid.NewGuid():N}");

            try
            {
                Directory.CreateSymbolicLink(link, target);
                return link;
            }
            catch (IOException)
            {
                return null;
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }
        }

        [Fact]
        public async Task When_repository_path_is_a_symlink_deleted_file_should_be_attributed()
        {
            this.Repository.CreateCsProject(ProjectName);
            await this.Repository.CreateTextFileAsync(
                Path.Combine(ProjectName, "Gone.cs"), "public class Gone {}");
            await this.Repository.CreateTextFileAsync(
                Path.Combine(ProjectName, "Keep.cs"), "public class Keep {}");
            this.Repository.StageAndCommit();

            this.Repository.DeleteFile(Path.Combine(ProjectName, "Gone.cs"));

            var link = TryCreateLinkTo(this.Repository.Path);
            if (link is null) return;

            try
            {
                var summary = new AffectedExecutor(new AffectedOptions(link)).Execute();

                Assert.Single(summary.FilesThatChanged);

                var changed = Assert.Single(summary.ProjectsWithChangedFiles);
                Assert.Equal(
                    Path.Combine(link, ProjectName, $"{ProjectName}.csproj"),
                    changed.GetFullPath());
            }
            finally
            {
                Directory.Delete(link);
            }
        }

        /// <summary>
        /// Reading a file out of a commit goes through a different path than restoring a deleted
        /// one, and decides whether a path belongs to the repository at all, so it is covered
        /// separately.
        /// </summary>
        [Fact]
        public void When_repository_path_is_a_symlink_package_changes_should_be_detected()
        {
            var packageName = "Some.Library";
            this.Repository.CreateDirectoryPackageProps(
                b => b.AddPackageVersion(packageName, "1.0.0"));

            this.Repository.CreateCsProject(ProjectName, b => b.AddNuGetDependency(packageName));

            this.Repository.StageAndCommit();

            this.Repository.UpdateDirectoryPackageProps(
                b => b.UpdatePackageVersion(packageName, "2.0.0"));

            var link = TryCreateLinkTo(this.Repository.Path);
            if (link is null) return;

            try
            {
                var summary = new AffectedExecutor(new AffectedOptions(link)).Execute();

                Assert.Single(summary.ChangedPackages);

                var affected = Assert.Single(summary.AffectedProjects);
                Assert.Equal(
                    Path.Combine(link, ProjectName, $"{ProjectName}.csproj"),
                    affected.GetFullPath());
            }
            finally
            {
                Directory.Delete(link);
            }
        }
    }
}
