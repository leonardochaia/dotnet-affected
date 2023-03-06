using DotnetAffected.Abstractions;
using Microsoft.Build.Graph;
using System.Collections.Generic;
using System.Linq;

namespace DotnetAffected.Core.Processor
{
    /// <summary>
    /// <inheritdoc cref="AffectedProcessorBase"/> <para/>
    /// Discovery is implemented using a custom, limited, project evaluation strategy. <br/>
    /// Based on files changed, specific logic is applied, on limited known files, to discover change metadata such as
    /// packages changed.<para/>
    ///
    /// The legacy processor compliments <see cref="AffectedProcessor"/> where netcore 3.1 is used as <see cref="AffectedProcessor"/>
    /// does not support it.
    /// </summary>
    internal class AffectedProcessorLegacy : AffectedProcessorBase
    {

        /// <inheritdoc/>
        protected override PackageChange[] DiscoverPackageChanges(AffectedProcessorContext context)
        {
            return FindChangedNugetPackages(context).ToArray();
        }

        /// <inheritdoc/>
        protected override ProjectGraphNode[] DiscoverAffectedProjects(AffectedProcessorContext context)
        {
            return DetermineAffectedProjectsLegacy(context).ToArray();
        }

        private ProjectGraphNode[] DetermineAffectedProjectsLegacy(AffectedProcessorContext context)
        {
            // Find projects referencing NuGet packages that changed
            var changedPackageNames = context.ChangedPackages.Select(p => p.Name);
            var projectsAffectedByNugetPackages = context.Graph
                .FindNodesReferencingNuGetPackages(changedPackageNames)
                .ToList();

            // Combine changed projects with projects affected by nuget changes
            var changedAndNugetAffected = context.ChangedProjects
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

        private IEnumerable<PackageChange> FindChangedNugetPackages(AffectedProcessorContext context) {
            // Try to find a Directory.Packages.props file in the list of changed files
            // We try to take the deepest file, assuming they import up
            var packagePropsPath = context.ChangedFiles
                .Where(f => f.EndsWith("Directory.Packages.props"))
                .OrderByDescending(p => p.Length)
                .Take(1)
                .FirstOrDefault();

            if (packagePropsPath is null)
                return Enumerable.Empty<PackageChange>();

            var fromFile = context.ChangesProvider.LoadDirectoryPackagePropsProject(context.RepositoryPath, packagePropsPath, context.FromRef, false);
            var toFile = context.ChangesProvider.LoadDirectoryPackagePropsProject(context.RepositoryPath, packagePropsPath, context.ToRef, true);

            // Parse props files into package and version dictionary
            var fromPackages = NugetHelper.ParseDirectoryPackageProps(fromFile);
            var toPackages = NugetHelper.ParseDirectoryPackageProps(toFile);

            // Compare both dictionaries
            return NugetHelper.TryFindDiffPackageDictionaries(fromPackages, toPackages, out var packageChanges)
                ? packageChanges
                : Enumerable.Empty<PackageChange>();
        }
    }
}
