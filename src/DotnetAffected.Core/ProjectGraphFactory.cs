using DotnetAffected.Abstractions;
using Microsoft.Build.Definition;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Evaluation.Context;
using Microsoft.Build.Execution;
using Microsoft.Build.FileSystem;
using Microsoft.Build.Graph;
using System.Linq;

namespace DotnetAffected.Core
{
    /// <summary>
    /// Resolves the <see cref="ProjectGraph"/> for the directory provided in user input.
    /// </summary>
    public class ProjectGraphFactory
    {
        private readonly IDiscoveryOptions _options;

        /// <summary>
        /// Creates an instance of the factory.
        /// </summary>
        /// <param name="options"></param>
        public ProjectGraphFactory(IDiscoveryOptions options)
        {
            _options = options;
        }

        /// <summary>
        /// Builds a <see cref="ProjectGraph"/> from all discovered projects.
        /// </summary>
        /// <returns>A new Project Graph.</returns>
        public ProjectGraph BuildProjectGraph(MSBuildFileSystemBase fileSystem, ProjectCollection? projectCollection = null)
        {
            // Discover all projects and build the graph
            var allProjects = new ProjectDiscoveryManager()
                .DiscoverProjects(_options);

            WriteLine($"Building Dependency Graph");

#if NET8
            var evaluationContext = EvaluationContext.Create(EvaluationContext.SharingPolicy.Shared, fileSystem);
            ProjectGraph.ProjectInstanceFactoryFunc fn = (path, properties, projectCollection) =>
            {
                var projectOptions = new ProjectOptions
                {
                    EvaluationContext = evaluationContext,
                    GlobalProperties = properties,
                    ProjectCollection = projectCollection
                };

                // Create a Project object which does the evaluation
                var project = Project.FromFile(path, projectOptions);

                // Create a ProjectInstance object which is what this factory needs to return
                var projectInstance = project.CreateProjectInstance(ProjectInstanceSettings.Immutable, evaluationContext);

                return projectInstance;
            };

            var entrypoints = allProjects.Select(p => new ProjectGraphEntryPoint(p));
            var output = new ProjectGraph(entrypoints, projectCollection ?? ProjectCollection.GlobalProjectCollection, fn);
#else
            var output = new ProjectGraph(allProjects, projectCollection ?? ProjectCollection.GlobalProjectCollection);
#endif
            WriteLine(
                $"Built Graph with {output.ConstructionMetrics.NodeCount} Projects " +
                $"in {output.ConstructionMetrics.ConstructionTime:s\\.ff}s");

            return output;
        }

        private void WriteLine(string? message = null)
        {
            // TODO: Logging
        }
    }
}
