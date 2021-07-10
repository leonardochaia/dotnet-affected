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
        private readonly Lazy<IEnumerable<ProjectGraphNode>> _changedProjects;
        private readonly Lazy<ProjectGraph> _graph;
        private readonly Lazy<string> _repositoryPath;

        public CommandExecutionContext(
            CommandExecutionData executionData,
            IConsole console,
            IChangesProvider changesProvider)
        {
            _executionData = executionData;
            _console = console;
            _changesProvider = changesProvider;

            // Figure out the repository path
            _repositoryPath = new Lazy<string>(() =>
            {
                // the argument takes precedence.
                if (!string.IsNullOrWhiteSpace(_executionData.RepositoryPath))
                {
                    return _executionData.RepositoryPath;
                }

                // if no arguments, then use current directory
                if (string.IsNullOrWhiteSpace(_executionData.SolutionPath))
                {
                    return Environment.CurrentDirectory;
                }

                // When using solution, and no path specified, assume the solution's directory
                var solutionDirectory = Path.GetDirectoryName(_executionData.SolutionPath);
                if (string.IsNullOrWhiteSpace(solutionDirectory))
                {
                    throw new InvalidOperationException(
                        $"Failed to determine directory from solution path {_executionData.SolutionPath}");
                }

                return solutionDirectory;
            });

            // Discovering projects, and finding affected may throw
            // For error handling to be managed properly at the handler level,
            // we use Lazies so that its done on demand when its actually needed
            // instead of happening here on the constructor
            _graph = new Lazy<ProjectGraph>(BuildProjectGraph);

            _changedProjects = new Lazy<IEnumerable<ProjectGraphNode>>(() =>
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

        public IEnumerable<ProjectGraphNode> ChangedProjects => _changedProjects.Value;

        public IEnumerable<ProjectGraphNode> AffectedProjects => _graph.Value.FindNodesThatDependOn(ChangedProjects);

        /// <summary>
        /// Builds a <see cref="ProjectGraph"/> from all discovered projects.
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
                .Select(x => x.AbsolutePath)
                .ToArray();
        }

        private IEnumerable<string> FindProjectsInDirectory()
        {
            WriteLine($"Finding all csproj at {_repositoryPath.Value}");

            // TODO: Find *.*proj ?
            return Directory.GetFiles(_repositoryPath.Value, "*.csproj", SearchOption.AllDirectories)
                .ToArray();
        }

        private IEnumerable<ProjectGraphNode> FindNodesThatChangedUsingGit()
        {
            var filesWithChanges = this._changesProvider
                .GetChangedFiles(_repositoryPath.Value,
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
