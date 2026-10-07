using LibGit2Sharp;
using System.Linq;

namespace DotnetAffected.Core
{
    /// <summary>
    /// Opens repositories with LibGit2Sharp. Every repository dotnet-affected reads is opened
    /// through here.
    /// </summary>
    internal static class GitRepository
    {
        /// <summary>
        /// Repository extensions git may write that the bundled libgit2 does not know about, but
        /// that do not change anything it reads.
        ///
        /// <c>relativeworktrees</c>: git 2.48 and later write it whenever a worktree is linked with
        /// relative paths (<c>git worktree add --relative-paths</c>, or
        /// <c>worktree.useRelativePaths</c>). libgit2 already resolves the relative links of such a
        /// worktree (its <c>.git</c> file, and the <c>gitdir</c> and <c>commondir</c> files in its
        /// administrative directory); the extension only tells older gits not to use the
        /// worktree, as they would misread those links. libgit2 refuses to open a repository with an
        /// extension it does not recognise, and only started recognising this one in 1.9.4
        /// (libgit2/libgit2#7254), which no LibGit2Sharp release bundles yet.
        /// </summary>
        private static readonly string[] SupportedExtensions = { "relativeworktrees" };

        /// <summary>
        /// Registered once, and only when a repository is first opened: this is a call into
        /// libgit2, and the MSBuild task points LibGit2Sharp at its native library before that.
        /// </summary>
        static GitRepository()
        {
            // SetExtensions replaces the extensions registered by callers, so keep any that are
            // already there. Registering one libgit2 already knows is harmless.
            GlobalSettings.SetExtensions(GlobalSettings.GetExtensions()
                .Union(SupportedExtensions)
                .ToArray());
        }

        /// <summary>
        /// Opens the repository at <paramref name="path"/>.
        /// </summary>
        public static Repository Open(string path) => new Repository(path);
    }
}
