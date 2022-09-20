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
        /// Gets the list of files the changed for the provided directory.
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        IEnumerable<string> GetChangedFiles(string directory, string from, string to);

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

        Project? LoadDirectoryPackagePropsProject(string directory, string pathToFile, string? commitRef, bool fallbackToHead);
    }
}
