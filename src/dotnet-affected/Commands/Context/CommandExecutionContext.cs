using Microsoft.Build.Graph;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.IO;
using System.Linq;

namespace Affected.Cli.Commands
{
    /// <summary>
    /// Calculates contextual information used across commands
    /// </summary>
    internal class CommandExecutionContext : ICommandExecutionContext
    {
        private readonly CommandExecutionData _executionData;
        private readonly IConsole _console;
        private readonly Lazy<IEnumerable<ProjectGraphNode>> _changedProjects;
        private readonly Lazy<IEnumerable<ProjectGraphNode>> _affectedProjects;
        private readonly IChangesProviderRef _changesProvider;
        private readonly IProjectGraphRef _graph;

        public CommandExecutionContext(
            CommandExecutionData executionData,
            IConsole console,
            IChangesProviderRef changesProvider,
            IProjectGraphRef graph)
        {
            _executionData = executionData;
            _console = console;
            _changesProvider = changesProvider;
            _graph = graph;

            // Discovering projects, and finding affected may throw
            // For error handling to be managed properly at the handler level,
            // we use Lazies so that its done on demand when its actually needed
            // instead of happening here on the constructor
            _changedProjects = new Lazy<IEnumerable<ProjectGraphNode>>(DetermineChangedProjects);
            _affectedProjects = new Lazy<IEnumerable<ProjectGraphNode>>(DetermineAffectedProjects);
        }

        public IEnumerable<IProjectInfo> ChangedProjects => _changedProjects.Value
            .Select(p => new ProjectInfo(p)).ToList();

        public IEnumerable<IProjectInfo> AffectedProjects => _affectedProjects.Value
            .Select(p => new ProjectInfo(p)).ToList();

        private IEnumerable<ProjectGraphNode> DetermineChangedProjects()
        {
            var output = FindChangedNodesUsingChangesProvider().ToList();

            if (!output.Any())
            {
                throw new NoChangesException();
            }

            return output;
        }

        private IEnumerable<ProjectGraphNode> DetermineAffectedProjects()
        {
            return _graph.Value.FindNodesThatDependOn(_changedProjects.Value);
        }

        private IEnumerable<ProjectGraphNode> FindChangedNodesUsingChangesProvider()
        {
            // Get all files that have changed
            var filesWithChanges = this._changesProvider.Value
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
