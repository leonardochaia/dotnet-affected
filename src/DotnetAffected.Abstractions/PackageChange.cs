using System.Collections.Generic;

namespace DotnetAffected.Abstractions
{
    /// <summary>
    /// Changes about a package across two revisions.
    /// </summary>
    public class PackageChange
    {

        /// <summary>
        /// Gets or sets the package name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or set the old versions.
        /// </summary>
        public IReadOnlyCollection<string> OldVersions => _oldVersions;

        /// <summary>
        /// Gets or sets the new versions.
        /// </summary>
        public IReadOnlyCollection<string> NewVersions => _newVersions;

        /// <summary>
        /// A unique id for this change-set which includes the package name and all of the versions changed.
        /// </summary>
        public string UniqueId => _uniqueId ??= $"{Name};{string.Join('|', OldVersions)};{string.Join('|', NewVersions)}";

        private string? _uniqueId;
        private readonly HashSet<string> _newVersions = new();
        private readonly HashSet<string> _oldVersions = new();
 
        /// <summary>
        /// Initializes a new instance of <see cref="PackageChange"/>.
        /// </summary>
        /// <param name="name"></param>
        public PackageChange(string name)
        {
            Name = name;
        }

        /// <inheritdoc/> 
        public override bool Equals(object? obj) => obj is PackageChange pkg && UniqueId == pkg.UniqueId;

        /// <inheritdoc/>
        public override int GetHashCode() => UniqueId.GetHashCode();

        /// <summary>
        /// Add a new version to the list of changes
        /// </summary>
        /// <param name="version"></param>
        public void AddNewVersion(string version)
        {
            _uniqueId = null;
            _newVersions.Add(version);
        }
        
        /// <summary>
        /// Add an old version to the list of changes
        /// </summary>
        /// <param name="version"></param>
        public void AddOldVersion(string version)
        {
            _uniqueId = null;
            _oldVersions.Add(version);
        }
    }
}
