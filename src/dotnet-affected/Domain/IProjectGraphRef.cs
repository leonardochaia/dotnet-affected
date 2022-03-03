using Microsoft.Build.Graph;

namespace Affected.Cli
{
    /// <summary>
    /// Gets a reference to the current <see cref="ProjectGraph"/>
    /// </summary>
    public interface IProjectGraphRef
    {
        /// <summary>
        /// Gets the current <see cref="ProjectGraph"/>.
        /// </summary>
        ProjectGraph Value { get; }
    }
}
