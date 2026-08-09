using Microsoft.Build.Graph;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotnetAffected.Core
{
    /// <summary>
    /// Extensions methods for <see cref="ProjectGraphNode"/>
    /// </summary>
    public static class ProjectGraphNodeExtensions
    {
        /// <summary>
        /// Recursively searches for all <see cref="ProjectGraphNode.ReferencingProjects"/>
        /// in all provided projects.
        /// </summary>
        /// <param name="targetNodes"></param>
        /// <returns></returns>
        public static IEnumerable<ProjectGraphNode> FindReferencingProjects(
            this IEnumerable<ProjectGraphNode> targetNodes)
            => Traverse(targetNodes);

        /// <summary>
        /// Recursively searches for <see cref="ProjectGraphNode.ReferencingProjects"/>
        /// </summary>
        /// <param name="targetNode"></param>
        /// <returns></returns>
        public static IEnumerable<ProjectGraphNode> FindReferencingProjects(
            this ProjectGraphNode targetNode)
            => Traverse(new[] { targetNode });

        /// <summary>
        /// Walks the graph backwards from every starting node at once, sharing a single set of
        /// visited projects, so each project is expanded no more than once.
        ///
        /// Computing and keeping a closure per node instead makes the total work the sum of all
        /// closure sizes, which is quadratic in the size of the graph. Sharing the visited set
        /// keeps it proportional to the graph itself.
        /// </summary>
        private static IEnumerable<ProjectGraphNode> Traverse(IEnumerable<ProjectGraphNode> startingNodes)
        {
            // Keyed by path so that the inner builds of a multi targeted project collapse into
            // one entry, which is what callers have always been given.
            var starting = startingNodes as IReadOnlyCollection<ProjectGraphNode> ?? startingNodes.ToList();

            // A multi targeted project appears as an outer node plus one node per framework, all
            // sharing a path, and the outer node references the inner ones. Seeding the starting
            // paths keeps a project from being reported as referencing itself.
            var visited = new HashSet<string>(starting.Select(node => node.ProjectInstance.FullPath));
            var pending = new Queue<ProjectGraphNode>();

            foreach (var node in starting)
            {
                foreach (var referencing in node.ReferencingProjects)
                {
                    if (visited.Add(referencing.ProjectInstance.FullPath))
                        pending.Enqueue(referencing);
                }
            }

            while (pending.Count > 0)
            {
                var current = pending.Dequeue();
                yield return current;

                foreach (var referencing in current.ReferencingProjects)
                {
                    if (visited.Add(referencing.ProjectInstance.FullPath))
                        pending.Enqueue(referencing);
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="ProjectGraphNode.ProjectInstance"/>'s Name.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static string GetProjectName(this ProjectGraphNode node)
        {
            return node.ProjectInstance.GetPropertyValue("ProjectName");
        }

        /// <summary>
        /// Gets the <see cref="ProjectGraphNode.ProjectInstance"/>'s FullPath.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static string GetFullPath(this ProjectGraphNode node)
        {
            return node.ProjectInstance.FullPath;
        }

        /// <summary>
        /// Checks if the project references the nuget package.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="nuGetPackageName"></param>
        /// <returns></returns>
        public static bool ReferencesNuGetPackage(this ProjectGraphNode node, string nuGetPackageName)
        {
            return node.ProjectInstance
                .GetItemsByItemTypeAndEvaluatedInclude("PackageReference", nuGetPackageName)
                .Any();
        }

        /// <summary>
        /// Checks if a project is excluded from central package management by looking at the
        /// ManagePackageVersionsCentrally prop.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static bool IsOptedOutFromCentrallyManagedNuGetPackageVersions(this ProjectGraphNode node)
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
