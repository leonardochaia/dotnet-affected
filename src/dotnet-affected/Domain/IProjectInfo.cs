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
    }
}
