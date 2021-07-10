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
    }
}
