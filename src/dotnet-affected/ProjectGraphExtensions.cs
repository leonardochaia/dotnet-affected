using Microsoft.Build.Graph;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Affected.Cli
{
    internal static class ProjectGraphExtensions
    {
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

        private static ProjectGraphNode? FindNodeByName(
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
