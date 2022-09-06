using DotnetAffected.Abstractions;
using Microsoft.Build.Graph;

namespace DotnetAffected.Core
{
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
        /// Builds a <see cref="ProjectGraph"/> from all discovered projects.
        /// </summary>
        /// <returns>A new Project Graph.</returns>
        public ProjectGraph BuildProjectGraph()
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
