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
        private readonly Lazy<ProjectDiscoveryResult> _projectDiscoveryResult;
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
            _projectDiscoveryResult = new Lazy<ProjectDiscoveryResult>(DiscoverProjects);
            _graph = new Lazy<ProjectGraph>(BuildProjectGraph);
            _changedProjects = new Lazy<IEnumerable<ProjectGraphNode>>(DetermineChangedProjects);
            _affectedProjects = new Lazy<IEnumerable<ProjectGraphNode>>(
                () => _graph.Value.FindNodesThatDependOn(_changedProjects.Value));
        }

        public IEnumerable<IProjectInfo> ChangedProjects => _changedProjects.Value
            .Select(p => new ProjectInfo(p)).ToList();

        public IEnumerable<IProjectInfo> AffectedProjects => _affectedProjects.Value
            .Select(p => new ProjectInfo(p)).ToList();

        /// <summary>
        /// Builds a <see cref="ProjectGraph"/> from all discovered projects.
        /// </summary>
        /// <returns>A new Project Graph.</returns>
        private ProjectGraph BuildProjectGraph()
        {
            // Build project graph based on all discovered projects
            WriteLine($"Building Dependency Graph");
            var output = new ProjectGraph(_projectDiscoveryResult.Value.Projects);

            WriteLine(
                $"Built Graph with {output.ConstructionMetrics.NodeCount} Projects " +
                $"in {output.ConstructionMetrics.ConstructionTime:s\\.ff}s");

            return output;
        }

        private IEnumerable<ProjectGraphNode> DetermineChangedProjects()
        {
            IEnumerable<ProjectGraphNode> output;
            if (!_executionData.AssumeChanges.Any())
            {
                output = FindNodesThatChangedUsingChangesProvider();
            }
            else
            {
                WriteLine($"Assuming hypothetical project changes, won't use Git diff");
                output = _graph.Value
                    .FindNodesByName(_executionData.AssumeChanges);
            }

            if (!output.Any())
            {
                throw new NoChangesException();
            }

            return output;
        }

        /// <summary>
        /// Discovers projects 
        /// </summary>
        /// <returns>A discovery result containing projects and path to the Directory.Packages.props file if exists</returns>
        private ProjectDiscoveryResult DiscoverProjects()
        {
            var projectDiscoveryResult = BuildProjectDiscoverer()
                .DiscoverProjects(_executionData);
            return projectDiscoveryResult;
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
            var nodesContainingFiles = _graph.Value.FindNodesContainingFiles(filesWithChanges);

            // Get all centrally managed NuGet packages that have changed
            var changedNuGets = this._changesProvider
                .GetChangedCentrallyManagedNuGetPackages(
                    _executionData.RepositoryPath,
                    _projectDiscoveryResult.Value.DirectoryPackagesPropsFile,
                    _executionData.From,
                    _executionData.To)
                .ToList();
            
            // Find the projects referencing those NuGet packages
            var nodesReferencingNuGets = _graph.Value.FindNodesReferencingNuGetPackages(changedNuGets);
            
            // Prepare the output
            var output = nodesContainingFiles
                .Concat(nodesReferencingNuGets)
                .Deduplicate()
                .ToList();

            WriteLine($"Found {filesWithChanges.Count} changed files" +
                      $" and {changedNuGets.Count} changed NuGet packages" +
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
