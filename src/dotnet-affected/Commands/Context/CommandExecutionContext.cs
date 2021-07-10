using Microsoft.Build.Graph;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.IO;
using System.Linq;

namespace Affected.Cli.Commands
{
    internal class CommandExecutionContext : ICommandExecutionContext
    {
        private readonly CommandExecutionData _executionData;
        private readonly IConsole _console;
        private readonly IChangesProvider _changesProvider;
        private readonly Lazy<IEnumerable<ProjectGraphNode>> _changedProjects;
        private readonly Lazy<IEnumerable<ProjectGraphNode>> _affectedProjects;
        private readonly Lazy<ProjectGraph> _graph;

        public CommandExecutionContext(
            CommandExecutionData executionData,
            IConsole console,
            IChangesProvider changesProvider)
        {
            _executionData = executionData;
            _console = console;
            _changesProvider = changesProvider;

            // Discovering projects, and finding affected may throw
            // For error handling to be managed properly at the handler level,
            // we use Lazies so that its done on demand when its actually needed
            // instead of happening here on the constructor
            _graph = new Lazy<ProjectGraph>(BuildProjectGraph);
            _changedProjects = new Lazy<IEnumerable<ProjectGraphNode>>(DetermineChangedProjects);
            _affectedProjects = new Lazy<IEnumerable<ProjectGraphNode>>(
                () => _graph.Value.FindNodesThatDependOn(ChangedProjects));
        }

        public IEnumerable<ProjectGraphNode> ChangedProjects => _changedProjects.Value;

        public IEnumerable<ProjectGraphNode> AffectedProjects => _affectedProjects.Value;

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

        private IEnumerable<ProjectGraphNode> DetermineChangedProjects()
        {
            if (!_executionData.AssumeChanges.Any())
            {
                return FindNodesThatChangedUsingChangesProvider();
            }

            WriteLine($"Assuming hypothetical project changes, won't use Git diff");

            return _graph.Value
                .FindNodesByName(_executionData.AssumeChanges);
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

        private IEnumerable<ProjectGraphNode> FindNodesThatChangedUsingChangesProvider()
        {
            // Get all files that have changed
            var filesWithChanges = this._changesProvider
                .GetChangedFiles(
                    _executionData.RepositoryPath,
                    _executionData.From,
                    _executionData.To)
                .ToList();

            // Match which files belong to which of our known projects
            var output = _graph.Value
                .FindNodesContainingFiles(filesWithChanges)
                .ToList();

            WriteLine($"Found {filesWithChanges.Count} changed files" +
                      $" inside {output.Count} projects.");

            return output;
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
