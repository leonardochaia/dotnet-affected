using DotnetAffected.Abstractions;
using Microsoft.Build.Graph;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DotnetAffected.Core
{
    /// <summary>
    /// The result of building a <see cref="ProjectGraph"/>.
    /// </summary>
    public class ProjectGraphBuildResult
    {
        /// <summary>
        /// Gets the built project graph.
        /// </summary>
        public ProjectGraph Graph { get; }

        /// <summary>
        /// Gets the paths of projects that were excluded during graph construction.
        /// </summary>
        public string[] ExcludedProjectPaths { get; }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public ProjectGraphBuildResult(ProjectGraph graph, string[] excludedProjectPaths)
        {
            Graph = graph;
            ExcludedProjectPaths = excludedProjectPaths;
        }
    }

    /// <summary>
    /// Resolves the <see cref="ProjectGraph"/> for the directory provided in user input.
    /// </summary>
    public class ProjectGraphFactory
    {
        private readonly IDiscoveryOptions _options;

        /// <summary>
        /// Creates an instance of the factory.
        /// </summary>
        /// <param name="options"></param>
        public ProjectGraphFactory(IDiscoveryOptions options)
        {
            _options = options;
        }

        /// <summary>
        /// Builds a <see cref="ProjectGraph"/> from all discovered projects,
        /// applying exclusion filters before graph construction.
        /// </summary>
        /// <returns>A build result containing the graph and any excluded project paths.</returns>
        public ProjectGraphBuildResult BuildProjectGraph()
        {
            // Discover all projects and build the graph
            var allProjects = new ProjectDiscoveryManager()
                .DiscoverProjects(_options);

            // Apply exclusion filter before building the graph
            var excludedPaths = new List<string>();
            var pattern = _options.ExclusionRegex;
            if (!string.IsNullOrEmpty(pattern))
            {
                var regex = new Regex(pattern);
                var included = new List<string>();
                foreach (var project in allProjects)
                {
                    if (regex.IsMatch(project))
                        excludedPaths.Add(project);
                    else
                        included.Add(project);
                }

                allProjects = included;
            }

            WriteLine($"Building Dependency Graph");

            var graph = new ProjectGraph(allProjects);

            WriteLine(
                $"Built Graph with {graph.ConstructionMetrics.NodeCount} Projects " +
                $"in {graph.ConstructionMetrics.ConstructionTime:s\\.ff}s");

            return new ProjectGraphBuildResult(graph, excludedPaths.ToArray());
        }

        private void WriteLine(string? message = null)
        {
            // TODO: Logging
        }
    }
}
