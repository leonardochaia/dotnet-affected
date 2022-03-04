using Microsoft.Build.Evaluation;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Affected.Cli
{
    internal static class NugetHelper
    {
        public static IDictionary<string, string> ParseDirectoryPackageProps(string propsFile)
        {
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

            var packageReferences = project.GetItems("PackageVersion");

            return packageReferences
                .ToDictionary(p => p.EvaluatedInclude,
                    p => p.Metadata.SingleOrDefault(m => m.Name == "Version")!.EvaluatedValue);
        }

        public static IEnumerable<PackageChange> DiffPackageDictionaries(
            IDictionary<string, string> fromPackages,
            IDictionary<string, string> toPackages)
        {
            var output = new List<PackageChange>();
            foreach (var (key, currentVersion) in fromPackages)
            {
                if (toPackages.ContainsKey(key))
                {
                    var otherVersion = toPackages[key];
                    if (otherVersion != currentVersion)
                    {
                        // Updated packages
                        output.Add(new PackageChange(key, otherVersion, currentVersion));
                    }
                }
                else
                {
                    // New packages
                    output.Add(new PackageChange(key, null, currentVersion));
                }
            }

            // Deleted packages
            foreach (var package in toPackages.Keys.Except(fromPackages.Keys))
            {
                output.Add(new PackageChange(package, toPackages[package], null));
            }

            return output;
        }
    }
}
