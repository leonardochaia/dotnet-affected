using DotnetAffected.Abstractions;
using Microsoft.Build.Graph;
using System.Collections.Generic;
using System.Linq;

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
            context.ChangedFiles = DiscoverChangedFiles(context).ToArray();

            // Map the files that changed to their corresponding project/s.
            context.ChangedProjects = DiscoverProjectsForFiles(context).ToArray();

            // Get packages that have changed, either from central package management or from the project file
            context.ChangedPackages = DiscoverPackageChanges(context);

            // Determine which projects are affected by the projects and packages that have changed.
            context.AffectedProjects = DiscoverAffectedProjects(context);

            // Output a summary of the operation.
            return new AffectedSummary(context.ChangedFiles, context.ChangedProjects, context.AffectedProjects, context.ChangedPackages);
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
            var provider = context.ChangedProjectsProvider ?? new PredictionChangedProjectsProvider(context.Graph, context.Options);
            // Match which files belong to which of our known projects
            return provider.GetReferencingProjects(context.ChangedFiles);
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
