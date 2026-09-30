using System;
using System.Diagnostics;
using System.IO;

namespace DotnetAffected.Testing.Utils
{
    /// <summary>
    /// A linked git worktree of a <see cref="TemporaryRepository"/>, created by the git CLI with
    /// <c>--relative-paths</c> (git 2.48 and later).
    ///
    /// Linking a worktree with relative paths makes git write <c>extensions.relativeWorktrees</c>
    /// into the repository's config, and libgit2 refuses to open a repository with an extension it
    /// does not know about. That affects the original checkout as much as the worktree.
    ///
    /// Everything here goes through the git CLI rather than LibGit2Sharp: the point is to produce
    /// the repository exactly as git leaves it, and to open it only from the code under test.
    /// </summary>
    public sealed class TemporaryRelativeWorktree : IDisposable
    {
        private readonly TempWorkingDirectory _directory;

        /// <summary>
        /// Adds a worktree of <paramref name="repository"/>, checked out on a new branch.
        /// </summary>
        /// <param name="repository">Repository to link the worktree to.</param>
        /// <param name="branchName">Branch created for the worktree.</param>
        public TemporaryRelativeWorktree(TemporaryRepository repository, string branchName = "worktree")
        {
            _directory = new TempWorkingDirectory();

            // REMARKS: outside the repository for the same reason as TemporaryWorktree.
            var path = System.IO.Path.Combine(_directory.Path, branchName);

            Git(repository.Path, "worktree", "add", "--relative-paths", "-b", branchName, path);

            // Taken from git rather than from the path we composed. See TemporaryWorktree.
            Path = System.IO.Path.GetFullPath(Git(path, "rev-parse", "--show-toplevel").Trim());

            var gitFile = File.ReadAllText(System.IO.Path.Combine(Path, ".git")).Trim();
            if (System.IO.Path.IsPathRooted(gitFile.Substring("gitdir:".Length).Trim()))
                throw new InvalidOperationException($"Expected a relative worktree link, got '{gitFile}'");
        }

        /// <summary>
        /// Gets the root of the worktree's checkout.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Stages and commits everything in the worktree, onto the worktree's own branch.
        /// </summary>
        /// <returns>The sha of the new commit.</returns>
        public string StageAndCommit(string message = null)
        {
            Git(Path, "add", "--all");
            Git(Path, "-c", "user.name=Leo", "-c", "user.email=lchaia@outlook.com",
                "commit", "--quiet", "-m", message ?? Guid.NewGuid().ToString("N"));

            return Git(Path, "rev-parse", "HEAD").Trim();
        }

        public void Dispose()
        {
            _directory.Dispose();
        }

        private static string Git(string workingDirectory, params string[] arguments)
        {
            var startInfo = new ProcessStartInfo("git")
            {
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            foreach (var argument in arguments)
                startInfo.ArgumentList.Add(argument);

            using var process = Process.Start(startInfo)!;
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
                throw new InvalidOperationException(
                    $"git {string.Join(" ", arguments)} exited with {process.ExitCode}: {error}");

            return output;
        }
    }
}
