using System.Collections.Generic;

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
        public PackageChange(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets or sets the package name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or set the old version.
        /// </summary>
        public ICollection<string> OldVersions { get; } = new HashSet<string>();

        /// <summary>
        /// Gets or sets the new version.
        /// </summary>
        public ICollection<string> NewVersions { get; } = new HashSet<string>();
    }
}
