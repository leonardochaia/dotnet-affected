namespace Affected.Cli
{
    /// <summary>
    /// Changes about a package across two revisions.
    /// </summary>
    public class PackageChange
    {
        /// <summary>
        /// Initializes a new instance of <see cref="PackageChange"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="oldVersion"></param>
        /// <param name="newVersion"></param>
        public PackageChange(string name, string? oldVersion, string? newVersion)
        {
            Name = name;
            OldVersion = oldVersion;
            NewVersion = newVersion;
        }

        /// <summary>
        /// Gets or sets the package name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or set the old version.
        /// </summary>
        public string? OldVersion { get; }

        /// <summary>
        /// Gets or sets the new version.
        /// </summary>
        public string? NewVersion { get; }
    }
}
