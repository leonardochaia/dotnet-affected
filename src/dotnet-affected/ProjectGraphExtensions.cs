using Microsoft.Build.Graph;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Affected.Cli
{
    internal static class ProjectGraphExtensions
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
                s => FindReferencingProjectsImpl(targetNode)
                    .ToList());
        }

        internal static IEnumerable<ProjectGraphNode> FindNodesContainingFiles(
            this ProjectGraph graph,
            IEnumerable<string> files)
        {
            var hasReturned = new HashSet<string>();
            foreach (var file in files)
            {
                // TODO: Find a better way of doing this.
                var nodes = graph.ProjectNodes
                    .Where(n => file.IsSubPathOf(n.ProjectInstance.Directory));

                foreach (var node in nodes)
                {
                    if (hasReturned.Add(node.ProjectInstance.FullPath))
                    {
                        yield return node;
                    }
                }
            }
        }

        internal static IEnumerable<ProjectGraphNode> FindNodesReferencingNuGetPackages(
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

        internal static ProjectGraphNode? FindNodeByPath(
            this ProjectGraph graph,
            string projectPath)
        {
            return graph.ProjectNodes
                .FirstOrDefault(n => n.ProjectInstance.FullPath == projectPath);
        }

        internal static ProjectGraphNode? FindNodeByName(
            this ProjectGraph graph,
            string name)
        {
            return graph.ProjectNodes
                .FirstOrDefault(n => n.GetProjectName()
                    .Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        internal static IEnumerable<ProjectGraphNode> FindNodesByName(
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
    }
}
