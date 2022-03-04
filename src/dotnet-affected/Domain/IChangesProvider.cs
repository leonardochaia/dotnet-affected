using System.Collections.Generic;

namespace Affected.Cli
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
        /// Uses the underlying changes provider to get
        /// the text contents of a file at <paramref name="from"/> and at <paramref name="to"/>.
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="pathToFile"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        (string FromText, string ToText) GetTextFileContents(string directory, string pathToFile, string from, string to);
    }
}
