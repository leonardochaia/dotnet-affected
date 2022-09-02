using Microsoft.Build.Graph;
using System.Collections.Generic;
using System.Linq;

namespace Affected.Cli
{
    public class AffectedExecutor : IAffectedExecutor
    {
        private readonly IChangesProvider _changesProvider;
        private readonly string _repositoryPath;
        private readonly string _fromRef;
        private readonly string _toRef;
        private readonly AffectedOptions _options;
        private ProjectGraph? _graph;
        private readonly IChangedProjectsProvider? _changedProjectsProvider;

        public AffectedExecutor(AffectedOptions options, ProjectGraph graph)
            : this(options, null, graph, null)
        {
        }

        public AffectedExecutor(
            AffectedOptions options,
            IChangesProvider? changesProvider = null,
            ProjectGraph? graph = null,
            IChangedProjectsProvider? changedProjectsProvider = null)
        {
            _changesProvider = changesProvider ?? new GitChangesProvider();
            _options = options;
            _graph = graph;
            _changedProjectsProvider = changedProjectsProvider;

            this._repositoryPath = options.RepositoryPath;
            this._fromRef = options.From;
            this._toRef = options.To;
        }

        private ProjectGraph Graph => _graph ??= new ProjectGraphFactory(_options).BuildProjectGraph();

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
            var provider = this._changedProjectsProvider ?? new ChangedProjectsProvider(Graph, _options);
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
            var packagePropsPath = changedFiles
                .SingleOrDefault(f => f.EndsWith("Directory.Packages.props"));

            if (packagePropsPath is null)
            {
                return Enumerable.Empty<PackageChange>();
            }

            // Get the contents of the file at from/to revisions
            var (fromFile, toFile) = _changesProvider
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
