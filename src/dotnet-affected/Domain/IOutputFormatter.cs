using System.Collections.Generic;
using System.Threading.Tasks;

namespace Affected.Cli
{
    /// <summary>
    /// Receives the list of projects to output and
    /// returns them in a particular format.
    /// </summary>
    public interface IOutputFormatter
    {
        /// <summary>
        /// Gets the formatter unique type.
        /// </summary>
        string Type { get; }

        /// <summary>
        /// Gets the format to use when storing a file
        /// with the contents outputted by this formatter.
        /// </summary>
        string NewFileExtension { get; }

        /// <summary>
        /// Formats the list of <paramref name="projects"/>
        /// into an specific output.
        /// </summary>
        /// <param name="projects">List of projects to format.</param>
        /// <returns>The formatted output.</returns>
        Task<string> Format(IEnumerable<IProjectInfo> projects);
    }
}
