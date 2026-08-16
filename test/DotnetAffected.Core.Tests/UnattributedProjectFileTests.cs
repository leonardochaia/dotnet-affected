using DotnetAffected.Abstractions;
using DotnetAffected.Testing.Utils;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DotnetAffected.Core.Tests
{
    /// <summary>
    /// A project file that changed while belonging to no project in the graph used to be counted
    /// among the files that changed and then dropped, leaving the count and the results
    /// disagreeing with a successful exit code. These cover it being reported.
    /// </summary>
    public class UnattributedProjectFileTests : BaseRepositoryTest
    {
        private const string SolutionPath = "test-solution.sln";

        private AffectedSummary Execute(AffectedOptions options)
            => new AffectedExecutor(options, changesProvider: new GitChangesProvider()).Execute();

        [Fact]
        public async Task When_changed_project_is_outside_the_solution_should_warn()
        {
            var inSolution = Repository.CreateCsProject("InSolution");
            await this.Repository.CreateSolutionAsync(SolutionPath, inSolution.FullPath);

            Repository.StageAndCommit();

            // Added to the repository but never added to the solution, so discovery never sees it.
            var outsideSolution = Repository.CreateCsProject("OutsideSolution");

            var summary = Execute(new AffectedOptions(
                Repository.Path,
                Path.Combine(Repository.Path, SolutionPath)));

            Assert.Contains(outsideSolution.FullPath, summary.FilesThatChanged);
            Assert.DoesNotContain(summary.ProjectsWithChangedFiles,
                p => p.GetFullPath() == outsideSolution.FullPath);

            var diagnostic = Assert.Single(summary.Diagnostics);
            Assert.Equal(AffectedDiagnosticSeverity.Warning, diagnostic.Severity);
            Assert.Contains(outsideSolution.FullPath, diagnostic.Message);
            Assert.Contains(SolutionPath, diagnostic.Message);
        }

        [Fact]
        public void When_changed_project_matches_exclude_discovery_should_warn()
        {
            Repository.CreateCsProject("Kept");

            Repository.StageAndCommit();

            var excluded = Repository.CreateCsProject("Excluded");

            var summary = Execute(new AffectedOptions(
                Repository.Path,
                excludeDiscoveryRegex: "Excluded"));

            var diagnostic = Assert.Single(summary.Diagnostics);
            Assert.Equal(AffectedDiagnosticSeverity.Warning, diagnostic.Severity);
            Assert.Contains(excluded.FullPath, diagnostic.Message);
            Assert.Contains("--exclude-discovery", diagnostic.Message);
        }

        /// <summary>
        /// A deleted project is absent from the graph because it is gone, which is the correct
        /// answer rather than an omission worth interrupting anyone over.
        /// </summary>
        [Fact]
        public void When_changed_project_was_deleted_should_not_warn()
        {
            Repository.CreateCsProject("Kept");
            var deleted = Repository.CreateCsProject("Deleted");

            Repository.StageAndCommit();

            File.Delete(deleted.FullPath);

            var summary = Execute(new AffectedOptions(Repository.Path));

            Assert.Contains(deleted.FullPath, summary.FilesThatChanged);
            Assert.Empty(summary.Diagnostics);
        }

        [Fact]
        public void When_every_changed_project_is_in_the_graph_should_not_warn()
        {
            Repository.CreateCsProject("InventoryManagement");

            var summary = Execute(new AffectedOptions(Repository.Path));

            Assert.Single(summary.ProjectsWithChangedFiles);
            Assert.Empty(summary.Diagnostics);
        }
    }
}
