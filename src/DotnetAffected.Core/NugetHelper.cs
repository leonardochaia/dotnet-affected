using DotnetAffected.Abstractions;
using Microsoft.Build.Evaluation;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace DotnetAffected.Core
{
    internal static class NugetHelper
    {
        public static IEnumerable<(string Package, string Version)> ParseDirectoryPackageProps(string? propsFile)
        {
            if (propsFile == null) return Enumerable.Empty<(string, string)>();

            Stream GenerateStreamFromString(string s)
            {
                var stream = new MemoryStream();
                var writer = new StreamWriter(stream);
                writer.Write(s);
                writer.Flush();
                stream.Position = 0;
                return stream;
            }

            using var reader = new XmlTextReader(GenerateStreamFromString(propsFile));
            var project = new Project(reader);

            return project.ItemsIgnoringCondition
                .Where(i => i.ItemType == "PackageVersion")
                .Select(i => (i.EvaluatedInclude,
                    i.Metadata.SingleOrDefault(m => m.Name == "Version")!.EvaluatedValue));
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
