using Affected.Cli.Commands;
using Microsoft.Build.Graph;
using System.Collections.Generic;
using System.Linq;

namespace Affected.Cli
{
    internal class AffectedExecutor : IAffectedExecutor
    {
        private readonly IChangesProviderRef _changesProvider;
        private readonly string _repositoryPath;
        private readonly string _fromRef;
        private readonly string _toRef;
        private readonly IProjectGraphRef _graph;
        private readonly IChangedProjectsProvider _changedProjectsProvider;

        public AffectedExecutor(
            CommandExecutionData executionData,
            IChangesProviderRef changesProvider,
            IProjectGraphRef graph,
            IChangedProjectsProvider changedProjectsProvider)
        {
            _changesProvider = changesProvider;
            _graph = graph;
            _changedProjectsProvider = changedProjectsProvider;

            this._repositoryPath = executionData.RepositoryPath;
            this._fromRef = executionData.From;
            this._toRef = executionData.To;
        }

        public AffectedSummary Execute()
        {
            // Get files that changed according to changes provider.
            var changedFiles = this.DetermineChangedFiles();

            // Get package changes from Directory.Package.props file.
            var changedPackages = this.FindChangedNugetPackages(changedFiles)
                .ToArray();

            // Map the files that changed to their corresponding project/s.
            var changedProjects = this.FindProjectsContainingFiles(changedFiles);

            // Determine which projects are affected by the projects and packages that have changed.
            var affectedProjects = this.DetermineAffectedProjects(changedPackages, changedProjects);

            // Output a summary of the operation.
            return new AffectedSummary(changedFiles,
                changedProjects,
                affectedProjects,
                changedPackages);
        }

        private string[] DetermineChangedFiles()
        {
            // Get all files that have changed
            return this._changesProvider.Value
                .GetChangedFiles(
                    _repositoryPath,
                    _fromRef,
                    _toRef)
                .ToArray();
        }

        private ProjectGraphNode[] FindProjectsContainingFiles(
            IEnumerable<string> changedFiles)
        {
            // Match which files belong to which of our known projects
            return this._changedProjectsProvider.GetReferencingProjects(changedFiles)
                .ToArray();
        }

        private ProjectGraphNode[] DetermineAffectedProjects(
            IEnumerable<PackageChange> changedPackages,
            IEnumerable<ProjectGraphNode> changedProjects)
        {
            // Find projects referencing NuGet packages that changed
            var changedPackageNames = changedPackages.Select(p => p.Name);
            var projectsAffectedByNugetPackages = _graph.Value
                .FindNodesReferencingNuGetPackages(changedPackageNames)
                .ToList();

            // Combine changed projects with projects affected by nuget changes
            var changedAndNugetAffected = changedProjects
                .Concat(projectsAffectedByNugetPackages)
                .Deduplicate();

            // Find projects that depend on the changed projects + projects affected by nuget
            var output = changedAndNugetAffected
                .FindReferencingProjects()
                .Concat(projectsAffectedByNugetPackages)
                .Deduplicate()
                .ToArray();

            return output;
        }

        private IEnumerable<PackageChange> FindChangedNugetPackages(
            IEnumerable<string> changedFiles)
        {
            // Try to find a Directory.Packages.props file in the list of changed files
            var packagePropsPath = changedFiles
                .SingleOrDefault(f => f.EndsWith("Directory.Packages.props"));

            if (packagePropsPath is null)
            {
                return Enumerable.Empty<PackageChange>();
            }

            // Get the contents of the file at from/to revisions
            var (fromFile, toFile) = _changesProvider.Value
                .GetTextFileContents(
                    _repositoryPath,
                    packagePropsPath,
                    _fromRef,
                    _toRef);

            // Parse props files into package and version dictionary
            var fromPackages = NugetHelper.ParseDirectoryPackageProps(fromFile);
            var toPackages = NugetHelper.ParseDirectoryPackageProps(toFile);

            // Compare both dictionaries
            return NugetHelper.DiffPackageDictionaries(fromPackages, toPackages);
        }
    }
}
