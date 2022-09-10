using DotnetAffected.Abstractions;
using Microsoft.Build.Graph;
using System.Collections.Generic;
using System.Linq;

namespace DotnetAffected.Core
{
    /// <summary>
    /// Analyzes MSBuild projects in order to determine which projects are affected by a set of changes.
    /// </summary>
    public class AffectedExecutor : IAffectedExecutor
    {
        private readonly IChangesProvider _changesProvider;
        private readonly string _repositoryPath;
        private readonly string _fromRef;
        private readonly string _toRef;
        private readonly AffectedOptions _options;
        private ProjectGraph? _graph;
        private readonly IChangedProjectsProvider? _changedProjectsProvider;

        /// <summary>
        /// Creates an executor for a repository path and a graph.
        /// </summary>
        /// <param name="repositoryPath"></param>
        /// <param name="graph"></param>
        public AffectedExecutor(string repositoryPath, ProjectGraph? graph = null)
            : this(new AffectedOptions(repositoryPath), graph)
        {
        }

        /// <summary>
        /// Creates the executor using all parameters.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="changesProvider"></param>
        /// <param name="graph"></param>
        /// <param name="changedProjectsProvider"></param>
        public AffectedExecutor(
            AffectedOptions options,
            ProjectGraph? graph = null,
            IChangesProvider? changesProvider = null,
            IChangedProjectsProvider? changedProjectsProvider = null)
        {
            _changesProvider = changesProvider ?? new GitChangesProvider();
            _options = options;
            _graph = graph;
            _changedProjectsProvider = changedProjectsProvider;

            this._repositoryPath = options.RepositoryPath;
            this._fromRef = options.FromRef;
            this._toRef = options.ToRef;
        }

        private ProjectGraph Graph => _graph ??= new ProjectGraphFactory(_options).BuildProjectGraph();

        /// <inheritdoc />
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
            return this._changesProvider
                .GetChangedFiles(
                    _repositoryPath,
                    _fromRef,
                    _toRef)
                .ToArray();
        }

        private ProjectGraphNode[] FindProjectsContainingFiles(
            IEnumerable<string> changedFiles)
        {
            var provider = this._changedProjectsProvider ?? new PredictionChangedProjectsProvider(Graph, _options);
            // Match which files belong to which of our known projects
            return provider.GetReferencingProjects(changedFiles)
                .ToArray();
        }

        private ProjectGraphNode[] DetermineAffectedProjects(
            IEnumerable<PackageChange> changedPackages,
            IEnumerable<ProjectGraphNode> changedProjects)
        {
            // Find projects referencing NuGet packages that changed
            var changedPackageNames = changedPackages.Select(p => p.Name);
            var projectsAffectedByNugetPackages = Graph
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
            // We try to take the deepest file, assuming they import up
            var packagePropsPath = changedFiles
                .Where(f => f.EndsWith("Directory.Packages.props"))
                .OrderByDescending(p => p.Length)
                .Take(1)
                .FirstOrDefault();

            if (packagePropsPath is null)
                return Enumerable.Empty<PackageChange>();

            var fromFile = _changesProvider.LoadDirectoryPackagePropsProject(_repositoryPath, packagePropsPath, _fromRef, false);
            var toFile = _changesProvider.LoadDirectoryPackagePropsProject(_repositoryPath, packagePropsPath, _toRef, true);

            // Parse props files into package and version dictionary
            var fromPackages = NugetHelper.ParseDirectoryPackageProps(fromFile);
            var toPackages = NugetHelper.ParseDirectoryPackageProps(toFile);

            // Compare both dictionaries
            return NugetHelper.DiffPackageDictionaries(fromPackages, toPackages);
        }

    }
}
