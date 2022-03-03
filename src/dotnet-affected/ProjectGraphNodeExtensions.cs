using Microsoft.Build.Graph;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Affected.Cli
{
    internal static class ProjectGraphNodeExtensions
    {
        internal static string GetProjectName(this ProjectGraphNode node)
        {
            return node.ProjectInstance.GetPropertyValue("ProjectName");
        }

        internal static bool ReferencesNuGetPackage(this ProjectGraphNode node, string nuGetPackageName)
        {
            return node.ProjectInstance
                .GetItemsByItemTypeAndEvaluatedInclude("PackageReference", nuGetPackageName)
                .Any();
        }
        
        internal static bool IsOptedOutFromCentrallyManagedNuGetPackageVersions(this ProjectGraphNode node)
        {
            return node.ProjectInstance.Properties
                .Any(x => x.Name == "ManagePackageVersionsCentrally"
                          && x.EvaluatedValue.Equals("false", StringComparison.InvariantCultureIgnoreCase));
        }

        internal static IEnumerable<ProjectGraphNode> Deduplicate(this IEnumerable<ProjectGraphNode> projectGraphNodes)
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
