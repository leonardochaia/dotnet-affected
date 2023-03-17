using DotnetAffected.Core.Extensions;
using Microsoft.Build.Graph;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotnetAffected.Core.Extensions
{
    /// <summary>
    /// Extension methods over <see cref="ProjectGraphNode"/>.
    /// </summary>
    public static class ProjectGraphExtensions
    {
        /// <summary>
        /// Recursively finds the list of nodes that reference the provided <paramref name="nuGetPackageNames"/>.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="nuGetPackageNames"></param>
        /// <returns></returns>
        public static IEnumerable<ProjectGraphNode> FindNodesReferencingNuGetPackages(
            this ProjectGraph graph,
            IEnumerable<string> nuGetPackageNames)
        {
            var hasReturned = new HashSet<string>();
            foreach (var nuget in nuGetPackageNames)
            {
                var nodes = graph.ProjectNodes
                    .Where(n => !n.IsOptedOutFromCentrallyManagedNuGetPackageVersions()
                                && n.ReferencesNuGetPackage(nuget));

                foreach (var node in nodes)
                {
                    if (hasReturned.Add(node.ProjectInstance.FullPath))
                    {
                        yield return node;
                    }
                }
            }
        }

        /// <summary>
        /// Searches for the node that matches the given <paramref name="projectPath"/>.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="projectPath"></param>
        /// <returns></returns>
        public static ProjectGraphNode? FindNodeByPath(
            this ProjectGraph graph,
            string projectPath)
        {
            return graph.ProjectNodes
                .FirstOrDefault(n => n.ProjectInstance.FullPath == projectPath);
        }

        /// <summary>
        /// Searches for the node that matches the given <paramref name="name"/>.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ProjectGraphNode? FindNodeByName(
            this ProjectGraph graph,
            string name)
        {
            return graph.ProjectNodes
                .FirstOrDefault(n => n.GetProjectName()
                    .Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Searches for a list of nodes where the names matches the provided ones.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="names"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IEnumerable<ProjectGraphNode> FindNodesByName(
            this ProjectGraph graph,
            IEnumerable<string> names)
        {
            foreach (var projName in names)
            {
                var node = graph.FindNodeByName(projName);
                if (node is null)
                {
                    throw new InvalidOperationException($"Couldn't find project with name {projName}");
                }

                yield return node;
            }
        }

        /// <summary>
        /// In this method all filters on the ProjectsGraphNodes are applied. Yet only the regex filter is implemented, but this method could grow with more filter functionality.
        /// Ideas for the future: Filter by projects size? Exclude specific paths? etc.
        /// </summary>
        /// <param name="projectGraphNodes">The projectGraphNodes objects to filter</param>
        /// <param name="options">The affected options which transport the filters</param>
        /// <returns></returns>
        public static IEnumerable<ProjectGraphNode> ExcludeProjects(
            this IEnumerable<ProjectGraphNode> projectGraphNodes,
            AffectedOptions options)
        {
            if (options.RegexExcludeFilter != null)
            {
                projectGraphNodes = projectGraphNodes.Exclude(nodes => nodes.GetProjectName(),
                    options.RegexExcludeFilter);
            }

            return projectGraphNodes;
        }
    }
}
