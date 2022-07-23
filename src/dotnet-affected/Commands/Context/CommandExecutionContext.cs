using Microsoft.Build.Graph;
using System;
using System.Collections.Generic;
using System.CommandLine;
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
        private readonly Lazy<IEnumerable<string>> _changedFiles;
        private readonly Lazy<IEnumerable<ProjectGraphNode>> _changedProjects;
        private readonly Lazy<IEnumerable<ProjectGraphNode>> _affectedProjects;
        private readonly Lazy<IEnumerable<PackageChange>> _changedNugetPackages;
        private readonly IChangesProviderRef _changesProvider;
        private readonly IProjectGraphRef _graph;

        public CommandExecutionContext(
            CommandExecutionData executionData,
            IConsole console,
            IChangesProviderRef changesProvider,
            IProjectGraphRef graph)
        {
            _executionData = executionData;
            _changesProvider = changesProvider;
            _graph = graph;

            // Discovering projects, and finding affected may throw
            // For error handling to be managed properly at the handler level,
            // we use Lazies so that its done on demand when its actually needed
            // instead of happening here on the constructor
            _changedFiles = new Lazy<IEnumerable<string>>(DetermineChangedFiles);
            _changedProjects = new Lazy<IEnumerable<ProjectGraphNode>>(DetermineChangedProjects);
            _affectedProjects = new Lazy<IEnumerable<ProjectGraphNode>>(DetermineAffectedProjects);
            _changedNugetPackages = new Lazy<IEnumerable<PackageChange>>(DetermineChangedNugetPackages);
        }

        public IEnumerable<string> ChangedFiles => _changedFiles.Value;

        public IEnumerable<PackageChange> ChangedNuGetPackages => _changedNugetPackages.Value;

        public IEnumerable<IProjectInfo> ChangedProjects => _changedProjects.Value
            .Select(p => new ProjectInfo(p))
            .OrderBy(x => x.Name)
            .ToList();

        public IEnumerable<IProjectInfo> AffectedProjects => _affectedProjects.Value
            .Select(p => new ProjectInfo(p))
            .OrderBy(x => x.Name)
            .ToList();

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

            return output;
        }

        private IEnumerable<ProjectGraphNode> DetermineAffectedProjects()
        {
            // Find projects referencing NuGet packages that changed
            var changedPackages = _changedNugetPackages.Value.Select(p => p.Name);
            var projectsAffectedByNugetPackages = _graph.Value
                .FindNodesReferencingNuGetPackages(changedPackages)
                .ToList();

            // Combine changed projects with projects affected by nuget changes
            var changedAndNugetAffected = _changedProjects.Value
                .Concat(projectsAffectedByNugetPackages)
                .Deduplicate();

            // Find projects that depend on the changed projects + projects affected by nuget
            var output = changedAndNugetAffected
                .FindReferencingProjects()
                .Concat(projectsAffectedByNugetPackages)
                .Deduplicate()
                .ToArray();

            if (!_changedProjects.Value.Any() && !output.Any())
            {
                throw new NoChangesException();
            }

            return output;
        }

        private IEnumerable<PackageChange> DetermineChangedNugetPackages()
        {
            // Try to find a Directory.Packages.props file in the list of changed files
            var packagePropsPath = _changedFiles.Value
                .SingleOrDefault(f => f.EndsWith("Directory.Packages.props"));

            if (packagePropsPath is null)
            {
                return Enumerable.Empty<PackageChange>();
            }

            // Get the contents of the file at from/to revisions
            var (fromFile, toFile) = _changesProvider.Value
                .GetTextFileContents(
                    _executionData.RepositoryPath,
                    packagePropsPath,
                    _executionData.From,
                    _executionData.To);

            // Parse props files into package and version dictionary
            var fromPackages = NugetHelper.ParseDirectoryPackageProps(fromFile);
            var toPackages = NugetHelper.ParseDirectoryPackageProps(toFile);

            // Compare both dictionaries
            return NugetHelper.DiffPackageDictionaries(fromPackages, toPackages);
        }
    }
}
