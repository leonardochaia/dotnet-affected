using DotnetAffected.Abstractions;
using DotnetAffected.Core.FileSystem;
using Microsoft.Build.Graph;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DotnetAffected.Core.Processor
{
    
    /// <summary>
    /// <inheritdoc cref="AffectedProcessorBase"/> <para/>
    /// Discovery is implemented using a full project evaluation strategy. <br/>
    /// For every project in the repository, where a change was detected, the project evaluated and the evaluation
    /// is then used to detect changes. <para/>
    ///
    /// The evaluation is done by MSBuild so it provide an accurate mapping for the project, including all internal
    /// MSBuild feature, without the need to manually implement MSBuild logic. <para/>
    ///
    /// Full evaluation is possible using an internal implementation of a virtual file system representing the filesystem
    /// for a given git commit. See <see cref="MsBuildGitFileSystem"/> and <see cref="EagerCachingMsBuildGitFileSystem"/>
    /// </summary>
    /// <remarks>Support Microsoft.Build 16.10 and above (net5.0 and above)</remarks>
    internal class AffectedProcessor : AffectedProcessorBase
    {
        private static readonly object NugetDataCacheKey = new();

        /// <inheritdoc/>
        protected override PackageChange[] DiscoverPackageChanges(AffectedProcessorContext context)
        {
            var projectsWithChangedPackages = FindChangedNugetPackages(context);
            context.Data[NugetDataCacheKey] = projectsWithChangedPackages;
            return new HashSet<PackageChange>(projectsWithChangedPackages.Values.SelectMany(e => e)).ToArray();
        }

        /// <inheritdoc/>
        protected override ProjectGraphNode[] DiscoverAffectedProjects(AffectedProcessorContext context)
            => DetermineAffectedProjects(context.ChangedProjects, DiscoverProjectsWithExclusivePackageChanges(context));

        private ProjectGraphNode[] DetermineAffectedProjects(ProjectGraphNode[] changedProjects,
                                                             IEnumerable<ProjectGraphNode> projectsWithChangedPackages)
        {
            // Find projects that depend on the changed projects + projects affected by nuget
            var output = changedProjects.FindReferencingProjects()
                .Concat(projectsWithChangedPackages.SelectMany(p => new[] { p }.Concat(p.FindReferencingProjects())))
                .Deduplicate()
                // .Except(changedProjects)
                .ToArray();

            return output;
        }

        /// <summary>
        /// Returns the list of projects that had packages changes with exclusive reference. <br/>
        /// A project might have packages that changed but they are not referenced by the project. This can happen
        /// when central package management is used and there is a version change to a package which the project does not
        /// reference. 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private IEnumerable<ProjectGraphNode> DiscoverProjectsWithExclusivePackageChanges(AffectedProcessorContext context)
        {
            var projectsWithChangedPackages = (Dictionary<ProjectGraphNode,IEnumerable<PackageChange>>)context.Data[NugetDataCacheKey];
            return projectsWithChangedPackages
                .Where(kvp => kvp.Value.Any(p => kvp.Key.ReferencesNuGetPackage(p.Name)))
                .Select(kvp => kvp.Key);
        }

        private Dictionary<ProjectGraphNode, IEnumerable<PackageChange>> FindChangedNugetPackages(AffectedProcessorContext context)
        {
            var changes = new Dictionary<ProjectGraphNode, IEnumerable<PackageChange>>();
            var changedPackageFiles = context.ChangedFiles
                .Where(f => f.EndsWith("Directory.Packages.props"))
                .OrderBy(p => p.Length)
                .Select(p => new FileInfo(p))
                .ToList();

            if (!changedPackageFiles.Any())
                return changes;

            if (context.RepositoryPath == changedPackageFiles[0].DirectoryName)
                changedPackageFiles = new List<FileInfo> { changedPackageFiles[0] };
            
            for (var i = 1; i < changedPackageFiles.Count; i++)
            {
                for (var q = 0; q < i; q++)
                {
                    if (changedPackageFiles[i].FullName.StartsWith(changedPackageFiles[q].DirectoryName))
                    {
                        changedPackageFiles.RemoveAt(i);
                        i -= 1;
                        break;
                    }
                }
            }
            
            var relatedProjects = context.Graph.ProjectNodes
                .Where(node => changedPackageFiles.Any(f => node.ProjectInstance.FullPath.StartsWith(f.DirectoryName)));

            foreach (var graphNode in relatedProjects)
            {
                var fromFile = context.ChangesProvider.LoadProject(context.RepositoryPath, graphNode.ProjectInstance.FullPath, context.FromRef, false);
                var toFile = context.ChangesProvider.LoadProject(context.RepositoryPath, graphNode.ProjectInstance.FullPath, context.ToRef, true);
                
                // Parse props files into package and version dictionary
                var fromPackages = NugetHelper.ParseDirectoryPackageProps(fromFile);
                var toPackages = NugetHelper.ParseDirectoryPackageProps(toFile);

                // Compare both dictionaries
                if (NugetHelper.TryFindDiffPackageDictionaries(fromPackages, toPackages, out var changedPackages))
                    changes[graphNode] = changedPackages;
            }

            return changes;
        }

    }
}
