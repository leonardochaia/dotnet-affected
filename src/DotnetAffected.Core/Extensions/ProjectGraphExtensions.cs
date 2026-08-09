using Microsoft.Build.Graph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DotnetAffected.Core
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
        /// Searches for the nodes matching an assumed change, which may be given as a path to a
        /// project file, as a project's ProjectName property, or as a project file name without its
        /// extension. A relative path is resolved against <paramref name="repositoryPath"/>.
        /// </summary>
        /// <remarks>
        /// A multi targeted project contributes an outer node plus one node per framework, all
        /// sharing a path. They collapse to a single node here, which is what mapping the project
        /// file through the predictors used to produce.
        /// </remarks>
        /// <param name="graph"></param>
        /// <param name="assumption"></param>
        /// <param name="repositoryPath"></param>
        /// <returns>Empty when the assumption matches no project in the graph.</returns>
        public static IEnumerable<ProjectGraphNode> FindNodesByAssumption(
            this ProjectGraph graph,
            string assumption,
            string repositoryPath)
        {
            var assumedPath = Path.GetFullPath(Path.IsPathRooted(assumption)
                ? assumption
                : Path.Combine(repositoryPath, assumption));

            return graph.ProjectNodes
                .Where(node => Matches(node, assumption, assumedPath))
                .Deduplicate();
        }

        private static bool Matches(ProjectGraphNode node, string assumption, string assumedPath)
        {
            var fullPath = node.GetFullPath();

            return fullPath.Equals(assumedPath, PathComparison)
                   || node.GetProjectName()
                       .Equals(assumption, StringComparison.OrdinalIgnoreCase)
                   || Path.GetFileNameWithoutExtension(fullPath)
                       .Equals(assumption, StringComparison.OrdinalIgnoreCase);
        }

        private static StringComparison PathComparison => GitChangesProvider.IsWindows
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
    }
}
