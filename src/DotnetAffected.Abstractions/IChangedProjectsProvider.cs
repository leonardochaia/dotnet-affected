using Microsoft.Build.Graph;
using System.Collections.Generic;

namespace DotnetAffected.Abstractions
{
    /// <summary>
    /// Finds projects based on a list of files they own.
    /// </summary>
    public interface IChangedProjectsProvider
    {
        /// <summary>
        /// Get which projects references the provided files
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        IEnumerable<ProjectGraphNode> GetReferencingProjects(
            IEnumerable<string> files);
    }
}
