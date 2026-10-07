using DotnetAffected.Core.FileSystem;
using DotnetAffected.Testing.Utils;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DotnetAffected.Core.Tests
{
    /// <summary>
    /// MSBuild expands globs in parallel, so the git file system is queried from several
    /// threads at once while a single project is being evaluated.
    /// </summary>
    public class GitIgnoredPathsConcurrencyTests : BaseDotnetAffectedTest
    {
        [Fact]
        public async Task When_queried_concurrently_ignored_path_detection_should_not_throw()
        {
            await Repository.CreateTextFileAsync(".gitignore", "ignored/\n");
            await Repository.CreateTextFileAsync("tracked.txt", "content");
            Repository.StageAndCommit();

            var fileSystem = new MsBuildGitFileSystem(Repository.Repository, Repository.Repository.Head.Tip);

            var paths = Enumerable.Range(0, 20_000)
                .Select(i => Path.Combine(Repository.Path, i % 2 == 0 ? "ignored" : "untracked", $"file{i}.props"))
                .ToArray();

            Parallel.ForEach(paths, new ParallelOptions { MaxDegreeOfParallelism = 16 }, path => fileSystem.FileExists(path));
        }
    }
}
