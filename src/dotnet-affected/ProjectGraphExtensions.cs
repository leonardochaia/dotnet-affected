using Microsoft.Build.Graph;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Affected.Cli
{
    internal static class ProjectGraphExtensions
    {
        public static IEnumerable<ProjectGraphNode> FindNodesThatDependOn(
            this ProjectGraph graph,
            IEnumerable<ProjectGraphNode> targetNodes)
        {
            var added = new HashSet<string>();
            foreach (var node in targetNodes)
            {
                foreach (var affected in graph.FindNodesThatDependOn(node).ToList())
                {
                    if (!added.Contains(affected.ProjectInstance.FullPath))
                    {
                        added.Add(affected.ProjectInstance.FullPath);
                        yield return affected;
                    }
                }
            }
        }

        public static IEnumerable<ProjectGraphNode> FindNodesThatDependOn(
            this ProjectGraph graph,
            ProjectGraphNode targetNode)
        {
            var targetNodeIsKnown = false;
            foreach (var currentNode in graph.ProjectNodes)
            {
                // ignore the target node
                if (currentNode == targetNode)
                {
                    targetNodeIsKnown = true;
                    continue;
                }

                foreach (var dependency in currentNode.ProjectReferences)
                {
                    // current project depends on targetNode
                    if (dependency == targetNode)
                    {
                        // if targetNode changes, currentNode will be affected.
                        yield return currentNode;

                        // since currentNode depends on targetNode,
                        // we also need to check which projects the currentNode depends on
                        // since they could be affected by the changes made on targetNode
                        foreach (var childDep in graph.FindNodesThatDependOn(currentNode))
                        {
                            yield return childDep;
                        }
                    }
                }
            }

            if (!targetNodeIsKnown)
            {
                throw new InvalidOperationException($"Requested to find {targetNode.ProjectInstance.FullPath} but its not present in known projects");
            }
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
                .FirstOrDefault(n => n.GetProjectName().Equals(name, StringComparison.OrdinalIgnoreCase));
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
