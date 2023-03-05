using DotnetAffected.Abstractions;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace DotnetAffected.Core
{
    internal static class NugetHelper
    {
        internal class PackageRef
        {
            /// <summary>
            /// Gets or sets the package name.
            /// </summary>
            public string Name { get; }

            public string Version { get; }

            public string UniqueId { get; }

            private PackageRef(string name, string version, string condition)
            {
                Name = name;
                Version = version;
                UniqueId = $"{name} {string.Join(";", condition)}";
            }

            public static PackageRef Create(ProjectItem item, string? versionOverride = null)
            {
                // we create a unique Id to the combination of all condition expressions from the item up to all relevant parents
                var conditions = item.Xml.AllParents
                    .OfType<ProjectItemGroupElement>()
                    .Where(e => !string.IsNullOrWhiteSpace(e.Condition))
                    .Select(e => e.Condition);

                if (!string.IsNullOrWhiteSpace(item.Xml.Condition))
                {
                    conditions = new[]
                    {
                        item.Xml.Condition
                    }.Concat(conditions);
                }

                var version = versionOverride ?? item.Metadata.Single(m => m.Name == "Version")
                    .EvaluatedValue;

                return new PackageRef(
                    item.EvaluatedInclude,
                    version,
                    string.Join(";", conditions)
                );
            }

            public override bool Equals(object? obj)
                => obj is PackageRef pkg && UniqueId == pkg.UniqueId && Version == pkg.Version;

            public override int GetHashCode() => (UniqueId, Version).GetHashCode();
        }

        public static IEnumerable<PackageRef> ParseDirectoryPackageProps(Project? project)
        {
            if (project == null) return Enumerable.Empty<PackageRef>();

            var centralPackageManagement = !project.MatchPropertyFlag("ManagePackageVersionsCentrally", false);
            var enablePackageVersionOverride = centralPackageManagement &&
                                               !project.MatchPropertyFlag("EnablePackageVersionOverride", false);
            var itemType = centralPackageManagement ? "PackageVersion" : "PackageReference";

            // A project contains the combination of multiple project files (e.g. DirectoryPackageProps can Import others)
            // So we might have multiple identical package versions for the same package/condition combination.
            // We only allow multiple package versions if the condition is different.
            // For a duplicate set in a project the last value is always the one resolved so we will use a dictionary to
            // reflect that logic.
            var packages = new Dictionary<string, PackageRef>();
            foreach (var item in project.ItemsIgnoringCondition)
            {
                if (item.ItemType == itemType)
                {
                    // TODO: Add support for OverrideVersion
                    if (item.MatchMetadataFlag("IsImplicitlyDefined", true))
                        continue;

                    var pkg = enablePackageVersionOverride &&
                              item.TryGetMetadataValue("VersionOverride", out var versionOverride)
                        ? PackageRef.Create(item, versionOverride)
                        : PackageRef.Create(item);

                    packages[pkg.UniqueId] = pkg;
                }
            }

            return packages.Values;
        }

        public static bool TryFindDiffPackageDictionaries(IEnumerable<PackageRef> fromPackages,
            IEnumerable<PackageRef> toPackages,
            [NotNullWhen(true)] out IEnumerable<PackageChange>? changedPackages)
        {
            var output = new Dictionary<string, PackageChange>();
            foreach (var pkg in fromPackages)
            {
                var otherVersions = toPackages
                    .Where(p => p.Name == pkg.Name)
                    .ToList();

                PackageChange change;
                change = !output.ContainsKey(pkg.Name) ? new PackageChange(pkg.Name) : output[pkg.Name];

                if (otherVersions.Any())
                {
                    if (otherVersions.Any(p => p.Version == pkg.Version))
                    {
                        continue;
                    }

                    // Updated packages
                    foreach (var other in otherVersions)
                    {
                        change.AddOldVersion(other.Version);
                    }

                    change.AddNewVersion(pkg.Version);
                    output[pkg.Name] = change;
                }
                else
                {
                    // New packages
                    change.AddNewVersion(pkg.Version);
                    output[pkg.Name] = change;
                }
            }

            // Deleted packages
            foreach (var package in toPackages.Except(fromPackages))
            {
                if (!output.ContainsKey(package.Name))
                    output.Add(package.Name, new PackageChange(package.Name));

                var change = output[package.Name];
                change.AddOldVersion(package.Version);
            }

            changedPackages = output.Any() ? output.Values : null;
            return changedPackages != null;
        }
    }
}
