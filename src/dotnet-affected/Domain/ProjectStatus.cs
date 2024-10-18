namespace Affected.Cli
{
    /// <summary>
    /// Describes whether the project is affected, changed, or excluded.
    /// </summary>
    public enum ProjectStatus
    {
        /// <summary>
        /// Indicates that the project is indirectly affected by changed dependencies.
        /// </summary>
        Affected = 0,
        /// <summary>
        /// Indicates that the project has changed files.
        /// </summary>
        Changed = 1,
        /// <summary>
        /// Indicates that the project is excluded from the potentially affected or changed projects.
        /// </summary>
        Excluded = 2,
    }
}
