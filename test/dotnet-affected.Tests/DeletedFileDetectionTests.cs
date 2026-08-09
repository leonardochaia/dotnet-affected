using DotnetAffected.Testing.Utils;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Affected.Cli.Tests
{
    /// <summary>
    /// Deleted files, exercised through the CLI rather than through the library.
    ///
    /// The overlay that restores deleted files before evaluation lives behind
    /// AffectedProcessorContext's lazy graph, and the CLI supplies a pre-built graph, so the
    /// library tests for this cover a path no frontend takes. See issue #84.
    /// </summary>
    public class DeletedFileDetectionTests : BaseInvocationTest
    {
        public DeletedFileDetectionTests(ITestOutputHelper helper)
            : base(helper)
        {
        }

        [Fact]
        public async Task When_only_change_is_a_deleted_file_project_should_be_reported()
        {
            var projectName = "InventoryManagement";
            var msBuildProject = this.Repository.CreateCsProject(projectName);

            // Keep.cs stays behind so the project still has a compile item after the deletion.
            await this.Repository.CreateTextFileAsync(
                Path.Combine(projectName, "Class1.cs"), "public class Class1 {}");
            await this.Repository.CreateTextFileAsync(
                Path.Combine(projectName, "Keep.cs"), "public class Keep {}");

            this.Repository.StageAndCommit();

            this.Repository.DeleteFile(Path.Combine(projectName, "Class1.cs"));

            var (output, exitCode) =
                await this.InvokeAsync($"-p {Repository.Path} --dry-run --verbose -f text");

            Assert.Equal(0, exitCode);
            Assert.Contains(msBuildProject.FullPath, output);
        }
    }
}
