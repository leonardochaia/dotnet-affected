using Microsoft.Build.Graph;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.IO;
using System.IO;
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
        private readonly Lazy<IEnumerable<string>> _changedFiles;
        private readonly Lazy<IEnumerable<ProjectGraphNode>> _changedProjects;
        private readonly Lazy<IEnumerable<ProjectGraphNode>> _affectedProjects;
        private readonly Lazy<IEnumerable<string>> _changedNugetPackages;
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
            _changedFiles = new Lazy<IEnumerable<string>>(DetermineChangedFiles);
            _changedProjects = new Lazy<IEnumerable<ProjectGraphNode>>(DetermineChangedProjects);
            _affectedProjects = new Lazy<IEnumerable<ProjectGraphNode>>(DetermineAffectedProjects);
            _changedNugetPackages = new Lazy<IEnumerable<string>>(DetermineChangedNugetPackages);
        }

        public IEnumerable<string> ChangedFiles => _changedFiles.Value;

        public IEnumerable<string> ChangedNuGetPackages => _changedNugetPackages.Value;

        public IEnumerable<IProjectInfo> ChangedProjects => _changedProjects.Value
            .Select(p => new ProjectInfo(p)).ToList();

        public IEnumerable<IProjectInfo> AffectedProjects => _affectedProjects.Value
            .Select(p => new ProjectInfo(p)).ToList();

        private IEnumerable<string> DetermineChangedFiles()
        {
            // Get all files that have changed
            return this._changesProvider.Value
                .GetChangedFiles(
                    _executionData.RepositoryPath,
                    _executionData.From,
                    _executionData.To)
                .ToList();
        }

        private IEnumerable<ProjectGraphNode> DetermineChangedProjects()
        {
            var filesWithChanges = _changedFiles.Value;

            // Match which files belong to which of our known projects
            var output = _graph.Value
                .FindNodesContainingFiles(filesWithChanges)
                .ToList();

            WriteLine($"Found {filesWithChanges.Count()} changed files" +
                      $" inside {output.Count} projects.");

            return output;
        }

        private IEnumerable<ProjectGraphNode> DetermineAffectedProjects()
        {
            // Find projects referencing NuGet packages that changed
            var nodesReferencingNuGets = _graph.Value
                .FindNodesReferencingNuGetPackages(_changedNugetPackages.Value);
            
            // Find projects that depend on the changed projects + projects affected by nuget
            var dependantProjects = _graph.Value
                .FindNodesThatDependOn(_changedProjects.Value.Concat(nodesReferencingNuGets));

            WriteLine($"Found {dependantProjects.Count()} affected by changed projects");
            WriteLine($"Found {nodesReferencingNuGets.Count()} affected by changed NuGet packages");

            var output = dependantProjects
                .Concat(nodesReferencingNuGets)
                .Deduplicate()
                .ToArray();

            if (!_changedProjects.Value.Any() && !output.Any())
            {
                throw new NoChangesException();
            }

            return output;
        }

        private IEnumerable<string> DetermineChangedNugetPackages()
        {
            var packagePropsPath = _changedFiles.Value
                .SingleOrDefault(f => f.EndsWith("Directory.Packages.props"));

            if (packagePropsPath is null)
            {
                return Enumerable.Empty<string>();
            }

            packagePropsPath = Path.GetRelativePath(_executionData.RepositoryPath, packagePropsPath);

            // Get all centrally managed NuGet packages that have changed
            var lineChanges = _changesProvider.Value.GetChangedLinesForFile(
                _executionData.RepositoryPath,
                packagePropsPath,
                _executionData.From,
                _executionData.To);

            var changedNugetPackages = NugetHelper.ParseNugetPackagesFromLines(lineChanges)
                .ToList();

            WriteLine($"Found {changedNugetPackages.Count()} changed NuGet packages");

            return changedNugetPackages;
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
