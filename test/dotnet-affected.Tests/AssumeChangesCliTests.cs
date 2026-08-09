using DotnetAffected.Testing.Utils;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Affected.Cli.Tests
{
    /// <summary>
    /// --assume-changes accepts a project name or a path to a project file, resolved against the
    /// graph rather than against the file system.
    /// </summary>
    public class AssumeChangesCliTests : BaseInvocationTest
    {
        public AssumeChangesCliTests(ITestOutputHelper helper)
            : base(helper)
        {
        }

        [Fact]
        public async Task When_assuming_changes_by_name_project_and_dependants_should_be_reported()
        {
            var project = this.Repository.CreateCsProject("InventoryManagement");
            var dependant = this.Repository.CreateCsProject(
                "InventoryManagement.Tests",
                p => p.AddProjectDependency(project.FullPath));

            // Commit so nothing actually changed and only the assumption drives the output.
            this.Repository.StageAndCommit();

            var (output, exitCode) = await this.InvokeAsync(
                $"-p {Repository.Path} --dry-run -f text --assume-changes InventoryManagement");

            Assert.Equal(0, exitCode);
            Assert.Contains(project.FullPath, output);
            Assert.Contains(dependant.FullPath, output);
        }

        /// <summary>
        /// A name is not enough when two projects share one, so a path has to work too.
        /// </summary>
        [Fact]
        public async Task When_assuming_changes_by_relative_path_project_should_be_reported()
        {
            var project = this.Repository.CreateCsProject("InventoryManagement");
            var other = this.Repository.CreateCsProject("PurchasingManagement");

            this.Repository.StageAndCommit();

            var relativePath = Path.Combine("InventoryManagement", "InventoryManagement.csproj");

            var (output, exitCode) = await this.InvokeAsync(
                $"-p {Repository.Path} --dry-run -f text --assume-changes {relativePath}");

            Assert.Equal(0, exitCode);
            Assert.Contains(project.FullPath, output);
            Assert.DoesNotContain(other.FullPath, output);
        }

        [Fact]
        public async Task When_assuming_changes_by_absolute_path_project_should_be_reported()
        {
            var project = this.Repository.CreateCsProject("InventoryManagement");

            this.Repository.StageAndCommit();

            var (output, exitCode) = await this.InvokeAsync(
                $"-p {Repository.Path} --dry-run -f text --assume-changes {project.FullPath}");

            Assert.Equal(0, exitCode);
            Assert.Contains(project.FullPath, output);
        }

        /// <summary>
        /// A typo must not be reported as a correct "nothing is affected" answer.
        /// </summary>
        [Fact]
        public async Task When_assuming_changes_for_an_unknown_project_should_fail()
        {
            this.Repository.CreateCsProject("InventoryManagement");
            this.Repository.StageAndCommit();

            var (_, exitCode) = await this.InvokeAsync(
                $"-p {Repository.Path} --dry-run -f text --assume-changes NoSuchProject");

            Assert.NotEqual(0, exitCode);
            Assert.Contains("NoSuchProject", Terminal.Error.ToString());
        }
    }
}
