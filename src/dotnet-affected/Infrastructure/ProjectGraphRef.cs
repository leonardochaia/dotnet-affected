using Affected.Cli.Commands;
using Microsoft.Build.Graph;
using System;
using System.CommandLine;
using System.CommandLine.IO;

namespace Affected.Cli
{
    /// <summary>
    /// Resolves the <see cref="ProjectGraph"/> for the directory provided in user input.
    /// </summary>
    internal class ProjectGraphRef : IProjectGraphRef
    {
        private readonly Lazy<ProjectGraph> _graph;
        private readonly CommandExecutionData _executionData;
        private readonly IConsole _console;

        public ProjectGraphRef(
            CommandExecutionData executionData,
            IConsole console)
        {
            _executionData = executionData;
            _console = console;

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
                .DiscoverProjects(_executionData);

            WriteLine($"Building Dependency Graph");

            var output = new ProjectGraph(allProjects);

            WriteLine(
                $"Built Graph with {output.ConstructionMetrics.NodeCount} Projects " +
                $"in {output.ConstructionMetrics.ConstructionTime:s\\.ff}s");

            return output;
        }

        private IProjectDiscoverer BuildProjectDiscoverer()
        {
            if (string.IsNullOrWhiteSpace(_executionData.SolutionPath))
            {
                WriteLine($"Discovering projects from {_executionData.RepositoryPath}");
                return new DirectoryProjectDiscoverer();
            }

            WriteLine($"Discovering projects from Solution {_executionData.SolutionPath}");
            return new SolutionFileProjectDiscoverer();
        }

        private void WriteLine(string? message = null)
        {
            if (!_executionData.Verbose)
            {
                return;
            }

            if (message == null)
            {
                _console.Out.WriteLine();
            }
            else
            {
                _console.Out.WriteLine(message);
            }
        }
    }
}
