using LibGit2Sharp;
using Microsoft.Build.Graph;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.IO;
using System.IO;
using System.Linq;
using Microsoft.Build.Construction;

namespace Affected.Cli.Commands
{
    internal class CommandExecutionContext : ICommandExecutionContext
    {
        private readonly CommandExecutionData _executionData;
        private readonly IConsole _console;
        private readonly IChangesProvider _changesProvider;
        private readonly Lazy<IEnumerable<ProjectGraphNode>> _nodesWithChanges;
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
            // so that error handling is managed properly at the handler level,
            // we use Lazy so that its done on demand when its actually needed.
            _graph = new Lazy<ProjectGraph>(BuildProjectGraph);

            _nodesWithChanges = new Lazy<IEnumerable<ProjectGraphNode>>(() =>
            {
                if (!_executionData.AssumeChanges.Any())
                {
                    return FindNodesThatChangedUsingGit();
                }

                WriteLine($"Assuming hypothetical project changes, won't use Git diff");

                return _graph.Value
                    .FindNodesByName(_executionData.AssumeChanges);
            });
        }

        public IEnumerable<ProjectGraphNode> NodesWithChanges => _nodesWithChanges.Value;

        public IEnumerable<ProjectGraphNode> FindAffectedProjects()
        {
            return _graph.Value.FindNodesThatDependOn(NodesWithChanges);
        }

        /// <summary>
        /// Builds a <see cref="ProjectGraph"/> from all found project files
        /// inside the <see cref="CommandExecutionData.SolutionPath"/> or <see cref="CommandExecutionData.RepositoryPath"/>.
        /// </summary>
        /// <returns>A new Project Graph.</returns>
        private ProjectGraph BuildProjectGraph()
        {
            // Find all csproj and build the dependency tree
            var allProjects = !string.IsNullOrWhiteSpace(_executionData.SolutionPath)
                ? FindProjectsInSolution()
                : FindProjectsInDirectory();

            WriteLine($"Building Dependency Graph");

            var output = new ProjectGraph(allProjects);

            WriteLine($"Found {output.ConstructionMetrics.NodeCount} projects");

            return output;
        }

        private IEnumerable<string> FindProjectsInSolution()
        {
            WriteLine($"Finding all projects from Solution {_executionData.SolutionPath}");

            var solution = SolutionFile.Parse(_executionData.SolutionPath);

            return solution.ProjectsInOrder
                .Where(x => x.ProjectType != SolutionProjectType.SolutionFolder)
                .Select(x => x.AbsolutePath);
        }

        private IEnumerable<string> FindProjectsInDirectory()
        {
            WriteLine($"Finding all csproj at {_executionData.RepositoryPath}");

            // TODO: Find *.*proj ?
            return Directory.GetFiles(
                _executionData.RepositoryPath,
                "*.csproj",
                SearchOption.AllDirectories);
        }

        private IEnumerable<ProjectGraphNode> FindNodesThatChangedUsingGit()
        {
            var filesWithChanges = this._changesProvider
                .GetChangedFiles(this._executionData.RepositoryPath,
                    this._executionData.From,
                    this._executionData.To)
                .ToList();

            var output = _graph.Value
                .FindNodesContainingFiles(filesWithChanges)
                .ToList();

            WriteLine($"Found {filesWithChanges.Count()} changed files" +
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
