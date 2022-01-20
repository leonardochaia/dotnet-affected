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
        /// Gets the list of names of changed centrally managed NuGet packages 
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="directoryPackagesPropsPath">The Directory.Packages.props file that contains the versions of the centrally managed NuGet packages</param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        IEnumerable<string> GetChangedCentrallyManagedNuGetPackages(string directory, string directoryPackagesPropsPath, string from, string to);
    }
}
