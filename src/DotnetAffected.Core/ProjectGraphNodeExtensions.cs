using Microsoft.Build.Graph;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Affected.Cli
{
    public static class ProjectGraphNodeExtensions
    {
        public static string GetProjectName(this ProjectGraphNode node)
        {
            return node.ProjectInstance.GetPropertyValue("ProjectName");
        }

        public static bool ReferencesNuGetPackage(this ProjectGraphNode node, string nuGetPackageName)
        {
            return node.ProjectInstance
                .GetItemsByItemTypeAndEvaluatedInclude("PackageReference", nuGetPackageName)
                .Any();
        }

        public static bool IsOptedOutFromCentrallyManagedNuGetPackageVersions(this ProjectGraphNode node)
        {
            return node.ProjectInstance.Properties
                .Any(x => x.Name == "ManagePackageVersionsCentrally"
                          && x.EvaluatedValue.Equals("false", StringComparison.InvariantCultureIgnoreCase));
        }

        public static IEnumerable<ProjectGraphNode> Deduplicate(this IEnumerable<ProjectGraphNode> projectGraphNodes)
        {
            var returned = new HashSet<string>();
            foreach (var node in projectGraphNodes)
            {
                if (returned.Add(node.ProjectInstance.FullPath))
                    yield return node;
            }
        }
    }
}
