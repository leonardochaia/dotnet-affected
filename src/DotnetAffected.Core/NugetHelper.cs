using DotnetAffected.Abstractions;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using System.Collections.Generic;
using System.Linq;

namespace DotnetAffected.Core
{
    internal static class NugetHelper
    {
        public static IEnumerable<(string Package, string Version)> ParseDirectoryPackageProps(Project? project)
        {
            if (project == null) return Enumerable.Empty<(string Package, string Version)>();

            // A project contains the combination of multiple project files (e.g. DirectoryPackageProps can Import others)
            // So we might have multiple identical package versions for the same package/condition combination.
            // We only allow multiple package versions if the condition is different.
            // For a duplicate set in a project the last value is always the one resolved so we will use a dictionary to
            // reflect that logic.
            var packages = new Dictionary<string, (string Package, string Version)>();
            foreach (var item in project.ItemsIgnoringCondition)
            {
                if (item.ItemType == "PackageVersion")
                {
                    // we create a unique Id to the combination of all condition expressions from the item up to all relevant parents
                    var conditions = item.Xml.AllParents
                        .OfType<ProjectItemGroupElement>()
                        .Where(e => !string.IsNullOrWhiteSpace(e.Condition))
                        .Select(e => e.Condition);

                    if (!string.IsNullOrWhiteSpace(item.Xml.Condition))
                        conditions = new[] { item.Xml.Condition }.Concat(conditions);

                    packages[$"{item.EvaluatedInclude} {string.Join(";", conditions)}"] = (item.EvaluatedInclude, item.Metadata.SingleOrDefault(m => m.Name == "Version")!.EvaluatedValue);
                }
            }

            return packages.Values;
        }

        public static IEnumerable<PackageChange> DiffPackageDictionaries(
            IEnumerable<(string Package, string Version)> fromPackages,
            IEnumerable<(string Package, string Version)> toPackages)
        {
            var output = new Dictionary<string, PackageChange>();
            foreach (var (key, currentVersion) in fromPackages)
            {
                var otherVersions = toPackages
                    .Where(p => p.Package == key)
                    .ToList();

                PackageChange change;
                change = !output.ContainsKey(key) ? new PackageChange(key) : output[key];

                if (otherVersions.Any())
                {
                    if (otherVersions.Any(p => p.Version == currentVersion))
                    {
                        continue;
                    }

                    // Updated packages
                    foreach (var other in otherVersions)
                    {
                        change.OldVersions.Add(other.Version);
                    }

                    change.NewVersions.Add(currentVersion);
                    output[key] = change;
                }
                else
                {
                    // New packages
                    change.NewVersions.Add(currentVersion);
                    output[key] = change;
                }
            }

            // Deleted packages
            foreach (var package in toPackages.Except(fromPackages))
            {
                if (!output.ContainsKey(package.Package))
                {
                    output.Add(package.Package, new PackageChange(package.Package));
                }

                var change = output[package.Package];
                change.OldVersions.Add(package.Version);
            }

            return output.Values;
        }
    }
}
