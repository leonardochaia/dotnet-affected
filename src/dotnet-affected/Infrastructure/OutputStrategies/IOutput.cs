using System.Collections.Generic;

namespace Affected.Cli
{
    /// <summary>
    /// Represents a single output unit that contains a set of projects.
    /// </summary>
    public interface IOutput
    {
        /// <summary>
        /// The name of the output unit, typically a file name.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// The directory where the output unit is stored.
        /// </summary>
        string Directory { get; }
        /// <summary>
        /// The projects contained in the output unit.
        /// </summary>
        IEnumerable<IProjectInfo> Projects { get; }
    }
}
