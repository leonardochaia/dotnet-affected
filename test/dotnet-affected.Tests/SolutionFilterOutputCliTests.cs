using DotnetAffected.Core;
using DotnetAffected.Testing.Utils;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Affected.Cli.Tests
{
    /// <summary>
    /// Tests for generating Solution Filter files from the CLI.
    /// </summary>
    public class SolutionFilterOutputCliTests : BaseInvocationTest
    {
        public SolutionFilterOutputCliTests(ITestOutputHelper helper)
            : base(helper)
        {
        }

        [Fact]
        public async Task Should_create_a_filter_referencing_the_solution()
        {
            var msBuildProject = this.Repository.CreateCsProject("InventoryManagement");
            var solutionName = "test-solution.slnx";
            await this.Repository.CreateXmlSolutionAsync(solutionName, msBuildProject.FullPath);

            var solutionPath = Path.Combine(Repository.Path, solutionName);

            var (_, exitCode) = await this.InvokeAsync(
                $"-p {Repository.Path} --filter-file-path {solutionPath} -f slnf");

            Assert.Equal(0, exitCode);

            // Load it back through the same model the CLI consumes, so the
            // test proves the output is usable as input.
            var filterPath = Path.Combine(Repository.Path, "affected.slnf");
            var filter = SolutionFilter.LoadFromFile(filterPath);

            Assert.Equal(solutionPath, filter.SolutionPath);
            Assert.Equal(msBuildProject.FullPath, filter.ProjectPaths.Single());
        }

        [Fact]
        public async Task Should_reference_the_solution_relative_to_the_output_dir()
        {
            var msBuildProject = this.Repository.CreateCsProject("InventoryManagement");
            var solutionName = "test-solution.slnx";
            await this.Repository.CreateXmlSolutionAsync(solutionName, msBuildProject.FullPath);

            var solutionPath = Path.Combine(Repository.Path, solutionName);

            var (_, exitCode) = await this.InvokeAsync(
                $"-p {Repository.Path} --filter-file-path {solutionPath} -f slnf --output-dir artifacts");

            Assert.Equal(0, exitCode);

            var filterPath = Path.Combine(Repository.Path, "artifacts", "affected.slnf");
            var contents = await File.ReadAllTextAsync(filterPath);

            // Relative, so that the generated filter can be committed or moved.
            Assert.DoesNotContain(Repository.Path, contents);
            Assert.Contains("..", contents);

            // and still resolves back to the real solution.
            Assert.Equal(solutionPath, SolutionFilter.LoadFromFile(filterPath).SolutionPath);
        }

        /// <summary>
        /// The format cannot work without a Solution, and must say so
        /// instead of failing with a stack trace after building the graph.
        /// </summary>
        [Fact]
        public async Task Without_a_filter_file_should_fail_at_parse_time()
        {
            this.Repository.CreateCsProject("InventoryManagement");

            var (_, exitCode) = await this.InvokeAsync($"-p {Repository.Path} --dry-run -f slnf");

            Assert.Equal(AffectedExitCodes.Failure, exitCode);

            var error = Terminal.Error.ToString();
            Assert.Contains("--filter-file-path", error);
            Assert.DoesNotContain("   at ", error);
        }

        [Fact]
        public async Task Using_a_traversal_project_as_filter_should_fail_at_parse_time()
        {
            var msBuildProject = this.Repository.CreateCsProject("InventoryManagement");
            await this.Repository.CreateTraversalProjectAsync("dirs.proj", msBuildProject.FullPath);

            var traversalPath = Path.Combine(Repository.Path, "dirs.proj");

            var (_, exitCode) = await this.InvokeAsync(
                $"-p {Repository.Path} --dry-run --filter-file-path {traversalPath} -f slnf");

            Assert.Equal(AffectedExitCodes.Failure, exitCode);
            Assert.DoesNotContain("   at ", Terminal.Error.ToString());
        }

        /// <summary>
        /// --solution-path is the deprecated alias, the validator has to honor it
        /// or it would reject a perfectly valid invocation.
        /// </summary>
        [Fact]
        public async Task Using_the_obsolete_solution_path_option_should_be_accepted()
        {
            var msBuildProject = this.Repository.CreateCsProject("InventoryManagement");
            var solutionName = "test-solution.slnx";
            await this.Repository.CreateXmlSolutionAsync(solutionName, msBuildProject.FullPath);

            var solutionPath = Path.Combine(Repository.Path, solutionName);

            var (_, exitCode) = await this.InvokeAsync(
                $"-p {Repository.Path} --solution-path {solutionPath} -f slnf");

            Assert.Equal(0, exitCode);
            Assert.Equal(solutionPath,
                SolutionFilter.LoadFromFile(Path.Combine(Repository.Path, "affected.slnf")).SolutionPath);
        }
    }
}
