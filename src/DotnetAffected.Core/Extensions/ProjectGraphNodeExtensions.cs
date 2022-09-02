using Microsoft.Build.Graph;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace DotnetAffected.Core
{
    /// <summary>
    /// Extensions methods for <see cref="ProjectGraphNode"/>
    /// </summary>
    public static class ProjectGraphNodeExtensions
    {
        private static readonly ConcurrentDictionary<string, IEnumerable<ProjectGraphNode>> Cache = new();

        /// <summary>
        /// Recursively searches for all <see cref="ProjectGraphNode.ReferencingProjects"/>
        /// in all provided projects.
        /// </summary>
        /// <param name="targetNodes"></param>
        /// <returns></returns>
        public static IEnumerable<ProjectGraphNode> FindReferencingProjects(
            this IEnumerable<ProjectGraphNode> targetNodes)
        {
            var added = new HashSet<string>();
            foreach (var node in targetNodes)
            {
                foreach (var affected in FindReferencingProjects(node))
                {
                    if (added.Add(affected.ProjectInstance.FullPath))
                        yield return affected;
                }
            }
        }

        /// <summary>
        /// Recursively searches for <see cref="ProjectGraphNode.ReferencingProjects"/>
        /// </summary>
        /// <param name="targetNode"></param>
        /// <returns></returns>
        public static IEnumerable<ProjectGraphNode> FindReferencingProjects(
            this ProjectGraphNode targetNode)
        {
            return Cache.GetOrAdd(
                targetNode.ProjectInstance.FullPath,
                _ => FindReferencingProjectsImpl(targetNode)
                    .ToList());
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

        private static IEnumerable<ProjectGraphNode> FindReferencingProjectsImpl(ProjectGraphNode node)
        {
            var added = new HashSet<string>();
            foreach (var referencingProject in node.ReferencingProjects)
            {
                // Return all referencing projects
                if (added.Add(referencingProject.ProjectInstance.FullPath))
                    yield return referencingProject;

                // Recurse each node's children
                foreach (var child in FindReferencingProjects(referencingProject))
                {
                    if (added.Add(child.ProjectInstance.FullPath))
                        yield return child;
                }
            }
        }
    }
}
