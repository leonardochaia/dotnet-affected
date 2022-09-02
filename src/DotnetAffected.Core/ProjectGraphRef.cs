using Microsoft.Build.Graph;
using System;

namespace Affected.Cli
{
    /// <summary>
    /// Resolves the <see cref="ProjectGraph"/> for the directory provided in user input.
    /// </summary>
    public class ProjectGraphRef : IProjectGraphRef
    {
        private readonly IDiscoveryOptions _options;
        private readonly Lazy<ProjectGraph> _graph;

        public ProjectGraphRef(IDiscoveryOptions options)
        {
            _options = options;

            // Discovering projects, and finding affected may throw
            // For error handling to be managed properly at the handler level,
            // we use Lazies so that its done on demand when its actually needed
            // instead of happening here on the constructor
            _graph = new Lazy<ProjectGraph>(BuildProjectGraph);
        }

        public ProjectGraph Value => this._graph.Value;

        /// <summary>
        /// Builds a <see cref="ProjectGraph"/> from all discovered projects.
        /// </summary>
        /// <returns>A new Project Graph.</returns>
        private ProjectGraph BuildProjectGraph()
        {
            // Discover all projects and build the graph
            var allProjects = BuildProjectDiscoverer()
                .DiscoverProjects(_options);

            WriteLine($"Building Dependency Graph");

            var output = new ProjectGraph(allProjects);

            WriteLine(
                $"Built Graph with {output.ConstructionMetrics.NodeCount} Projects " +
                $"in {output.ConstructionMetrics.ConstructionTime:s\\.ff}s");

            return output;
        }

        private IProjectDiscoverer BuildProjectDiscoverer()
        {
            if (string.IsNullOrWhiteSpace(_options.SolutionPath))
            {
                WriteLine($"Discovering projects from {_options.RepositoryPath}");
                return new DirectoryProjectDiscoverer();
            }

            WriteLine($"Discovering projects from Solution {_options.SolutionPath}");
            return new SolutionFileProjectDiscoverer();
        }

        private void WriteLine(string? message = null)
        {
            // TODO: Logging
        }
    }
}
