using DotnetAffected.Abstractions;
using Microsoft.Build.Graph;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DotnetAffected.Core.Processor
{
    /// <summary>
    /// Processor engine that discover changes in the projects. Files, packages, projects etc...
    /// </summary>
    internal abstract class AffectedProcessorBase
    {
        /// <summary>
        /// Start processing the repository and discover changes.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public AffectedSummary Process(AffectedProcessorContext context)
        {
            // Get files that changed according to changes provider.
            context.ChangedFiles = DiscoverChangedFiles(context)
                .ToArray();

            // Map the files that changed to their corresponding project/s.
            var excludedProjects = new List<ProjectGraphNode>();
            context.ChangedProjects = ApplyExclusionPattern(
                DiscoverProjectsForFiles(context),
                context.Options,
                excludedProjects);

            // Get packages that have changed, either from central package management or from the project file
            context.ChangedPackages = DiscoverPackageChanges(context);

            // Determine which projects are affected by the projects and packages that have changed.
            context.AffectedProjects = ApplyExclusionPattern(
                DiscoverAffectedProjects(context),
                context.Options,
                excludedProjects);

            // Output a summary of the operation.
            return new AffectedSummary(
                context.ChangedFiles,
                context.ChangedProjects,
                context.AffectedProjects,
                excludedProjects.Distinct()
                    .ToArray(),
                context.ChangedPackages);
        }

        /// <summary>
        /// Discover which files have changes
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected virtual IEnumerable<string> DiscoverChangedFiles(AffectedProcessorContext context)
            => context.ChangesProvider.GetChangedFiles(context.RepositoryPath, context.FromRef, context.ToRef);

        /// <summary>
        /// Discover which projects have changed based on the changed files
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected virtual IEnumerable<ProjectGraphNode> DiscoverProjectsForFiles(AffectedProcessorContext context)
        {
            // We init now because we want the graph to initialize late (lazy)
            var provider = context.ChangedProjectsProvider ??
                           new PredictionChangedProjectsProvider(context.Graph, context.Options);
            // Match which files belong to which of our known projects
            return provider.GetReferencingProjects(context.ChangedFiles);
        }

        /// <summary>
        /// Applies the <see cref="AffectedOptions.ExclusionRegex"/> to exclude
        /// projects that matches the regular expression.
        /// </summary>
        /// <param name="inputProjects">List of projects that changed.</param>
        /// <param name="options">Affected options.</param>
        /// <param name="excludedProjects">Collection of excluded projects</param>
        /// <returns>Project lis excluding the ones that matches the exclusion regex.</returns>
        protected virtual ProjectGraphNode[] ApplyExclusionPattern(
            IEnumerable<ProjectGraphNode> inputProjects,
            AffectedOptions options,
            ICollection<ProjectGraphNode> excludedProjects)
        {
            var pattern = options.ExclusionRegex;

            if (string.IsNullOrEmpty(pattern))
                return inputProjects.ToArray();

            var changedProjects = new List<ProjectGraphNode>();
            var regex = new Regex(pattern);
            foreach (var project in inputProjects)
            {
                if (regex.IsMatch(project.GetFullPath()))
                    excludedProjects.Add(project);
                else
                    changedProjects.Add(project);
            }

            return changedProjects.ToArray();
        }

        /// <summary>
        /// Discover which packages have changed. <br/>
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected abstract PackageChange[] DiscoverPackageChanges(AffectedProcessorContext context);

        /// <summary>
        /// Discover which projects are affected, indirectly, due to changes in other projects.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected abstract ProjectGraphNode[] DiscoverAffectedProjects(AffectedProcessorContext context);
    }
}
