using LibGit2Sharp;
using System;

namespace DotnetAffected.Testing.Utils
{
    /// <summary>
    /// A linked git worktree of a <see cref="TemporaryRepository"/>.
    ///
    /// In a worktree the working directory and the git directory live apart: the checkout has a
    /// <c>.git</c> file pointing at an admin directory inside the original repository. Anything
    /// deriving paths from one while assuming the other silently misbehaves, so the scenario is
    /// worth exercising rather than assuming.
    /// </summary>
    public sealed class TemporaryWorktree : IDisposable
    {
        private readonly TempWorkingDirectory _directory;

        /// <summary>
        /// Adds a worktree of <paramref name="repository"/>, checked out on a new branch.
        /// </summary>
        /// <param name="repository">Repository to link the worktree to.</param>
        /// <param name="branchName">Branch created for the worktree.</param>
        public TemporaryWorktree(TemporaryRepository repository, string branchName = "worktree")
        {
            _directory = new TempWorkingDirectory();

            // REMARKS: the worktree deliberately lives outside the repository. Nesting it would
            // put a second copy of every project underneath the repository root, which project
            // discovery would then find alongside the originals.
            var path = System.IO.Path.Combine(_directory.Path, branchName);

            repository.Repository.Worktrees.Add(branchName, path, false);

            // REMARKS: LibGit2Sharp writes the worktree's administrative files but leaves the
            // working directory empty, and its overload that takes a committish checks out in the
            // original repository instead, which fails with "cannot set HEAD to reference ... as it
            // is the current HEAD of a linked repository". Resetting hard from inside the worktree
            // populates it from its own HEAD without moving any reference.
            string workingDirectory;
            using (var worktreeRepository = new Repository(path))
            {
                worktreeRepository.Reset(ResetMode.Hard);
                workingDirectory = worktreeRepository.Info.WorkingDirectory;
            }

            // Taken from git rather than from the path we composed, for the same reason
            // TemporaryRepository does it: on OSX the temp directory is reached through a symlink,
            // so "/var/x" and the "/private/var/x" git reports are the same place spelled two ways.
            // Handing tests the resolved spelling keeps them about worktrees, leaving the unresolved
            // spelling to SymlinkedRepositoryPathTests.
            Path = System.IO.Path.TrimEndingDirectorySeparator(workingDirectory);
        }

        /// <summary>
        /// Gets the root of the worktree's checkout.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Stages and commits everything in the worktree, onto the worktree's own branch.
        /// </summary>
        public Commit StageAndCommit(string message = null)
        {
            using var repository = new Repository(Path);

            Commands.Stage(repository, "*");

            var author = new Signature("Leo", "lchaia@outlook.com", DateTime.Now);
            return repository.Commit(message ?? Guid.NewGuid()
                .ToString("N"), author, author);
        }

        public void Dispose()
        {
            _directory.Dispose();
        }
    }
}
