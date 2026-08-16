using Microsoft.Build.Evaluation;
using System.Collections.Generic;

namespace DotnetAffected.Abstractions
{
    /// <summary>
    /// Abstraction over the underlying repository implementation.
    /// </summary>
    public interface IChangesProvider
    {
        /// <summary>
        /// Gets the list of files that changed between <paramref name="from"/> and the working
        /// tree, which is the only revision whose projects can be analysed.
        /// </summary>
        /// <param name="directory">Root of the repository.</param>
        /// <param name="from">Branch or commit to compare against. Defaults to HEAD when empty.</param>
        /// <param name="uncommitted">What the working tree contributes on top of the commits.</param>
        /// <returns></returns>
        IEnumerable<string> GetChangedFiles(string directory, string from, UncommittedChanges uncommitted);

        /// <summary>
        /// Gets the commit the working tree is checked out at.
        /// </summary>
        /// <param name="directory">Root of the repository.</param>
        /// <returns>The commit's SHA, or null when the repository has no commits.</returns>
        string? GetWorkingTreeCommitSha(string directory);

        /// <summary>
        /// Resolves a branch name or commit-ish to the commit it points at.
        /// </summary>
        /// <param name="directory">Root of the repository.</param>
        /// <param name="commitRef">The branch or commit to resolve.</param>
        /// <returns>The commit's SHA.</returns>
        string ResolveCommitSha(string directory, string commitRef);

        /// <summary>
        /// Uses the underlying changes provider to load a <see cref="Project"/> file at <paramref name="commitRef"/>.
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="pathToFile"></param>
        /// <param name="commitRef"></param>
        /// <param name="fallbackToHead">When true, uses the HEAD as the default commit, otherwise uses the current working directory. <br/>
        /// Applicable only when <paramref name="commitRef"/> is null or empty.</param>
        /// <returns></returns>
        Project? LoadProject(string directory, string pathToFile, string? commitRef, bool fallbackToHead);

        /// <summary>
        /// Uses the underlying changes provider to load a <see cref="Project"/> for Directory.Packages.Prop
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="pathToFile"></param>
        /// <param name="commitRef"></param>
        /// <param name="fallbackToHead"></param>
        /// <returns></returns>
        Project? LoadDirectoryPackagePropsProject(string directory, string pathToFile, string? commitRef,
            bool fallbackToHead);

        /// <summary>
        /// Reads the contents of <paramref name="filePaths"/> as they were at <paramref name="commitRef"/>.
        ///
        /// This is the primitive the <c>LoadProject</c> methods are built on, exposed so that
        /// files removed by the diff can still be read back after they are gone from disk.
        /// </summary>
        /// <param name="directory">Root of the repository.</param>
        /// <param name="commitRef">Commit to read from. When null or empty, HEAD is used.</param>
        /// <param name="filePaths">Absolute paths of the files to read.</param>
        /// <returns>
        /// Contents keyed by absolute path. Paths that did not exist at
        /// <paramref name="commitRef"/> are absent from the result.
        /// </returns>
        IReadOnlyDictionary<string, byte[]> ReadFilesAt(
            string directory,
            string? commitRef,
            IReadOnlyCollection<string> filePaths);
    }
}
