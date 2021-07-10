using LibGit2Sharp;
using System.Collections.Generic;

namespace Affected.Cli
{
    internal class GitChangesProvider : IChangesProvider
    {
        public IEnumerable<string> GetChangedFiles(string directory, string from, string to)
        {
            using var repository = new Repository(directory);

            // Find the To Commit or use HEAD.
            var toCommit = GitUtils.GetCommitOrHead(repository, to);

            // No from: compare against working directory
            if (string.IsNullOrWhiteSpace(from))
            {
                // this.WriteLine($"Finding changes from working directory against {to}");

                return GitUtils.GetChangesAgainstWorkingDirectory(repository, toCommit.Tree);
            }

            var fromCommit = GitUtils.GetCommitOrThrow(repository, @from);
            // this.WriteLine($"Finding changes from {from} against {to}");

            // Compare the two commits.
            return GitUtils.GetChangesBetweenTrees(
                repository,
                fromCommit.Tree,
                toCommit.Tree);
        }
    }
}
