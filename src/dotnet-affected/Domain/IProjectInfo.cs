using System.Collections;
using System.Collections.Generic;

namespace Affected.Cli
{
    /// <summary>
    /// Keeps information about a .NET project
    /// </summary>
    public interface IProjectInfo
    {
        /// <summary>
        /// Gets the project name, without extension.
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Gets the full path to the project's file.
        /// </summary>
        string FilePath { get; }

        /// <summary>
        /// Gets the additional properties and values from the project's file if they exist.
        /// </summary>
        IDictionary<string, string> AdditionalProperties { get; }
    }
}
