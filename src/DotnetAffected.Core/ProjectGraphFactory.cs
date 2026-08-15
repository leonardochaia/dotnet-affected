using DotnetAffected.Abstractions;
using Microsoft.Build.Definition;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Evaluation.Context;
using Microsoft.Build.FileSystem;
using Microsoft.Build.Graph;
using System.Collections.Generic;
using System.Linq;

namespace DotnetAffected.Core
{
    /// <summary>
    /// Resolves the <see cref="ProjectGraph"/> for the directory provided in user input.
    /// </summary>
    public class ProjectGraphFactory
    {
        private readonly IDiscoveryOptions _options;
        private readonly MSBuildFileSystemBase? _fileSystem;

        /// <summary>
        /// Creates an instance of the factory.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="fileSystem">
        /// File system to evaluate projects against. When null, projects are evaluated
        /// against the real file system, which is the usual case.
        /// </param>
        public ProjectGraphFactory(IDiscoveryOptions options, MSBuildFileSystemBase? fileSystem = null)
        {
            _options = options;
            _fileSystem = fileSystem;
        }

        /// <summary>
        /// Builds a <see cref="ProjectGraph"/> from all discovered projects.
        /// </summary>
        /// <returns>A new Project Graph.</returns>
        public ProjectGraph BuildProjectGraph()
            => BuildProjectGraph(new ProjectDiscoveryManager()
                .DiscoverProjects(_options)
                .Projects);

        /// <summary>
        /// Builds a <see cref="ProjectGraph"/> from projects that have already been discovered,
        /// for callers that need to know what discovery left out. Discovery is the only place
        /// that knows, and it cannot report it through a graph it is deliberately absent from.
        /// </summary>
        /// <returns>A new Project Graph.</returns>
        internal ProjectGraph BuildProjectGraph(IReadOnlyCollection<string> allProjects)
        {
            WriteLine($"Building Dependency Graph");

            var output = _fileSystem is null
                ? new ProjectGraph(allProjects)
                : BuildProjectGraphUsingFileSystem(allProjects);

            WriteLine(
                $"Built Graph with {output.ConstructionMetrics.NodeCount} Projects " +
                $"in {output.ConstructionMetrics.ConstructionTime:s\\.ff}s");

            return output;
        }

        /// <summary>
        /// Evaluates every project in the graph against <see cref="_fileSystem"/>, by way of a
        /// shared <see cref="EvaluationContext"/>. Sharing matters: imports and globs are
        /// resolved through the context, so each project has to be evaluated with the same one.
        /// </summary>
        private ProjectGraph BuildProjectGraphUsingFileSystem(IEnumerable<string> allProjects)
        {
            var projectCollection = new ProjectCollection();
            var evaluationContext = EvaluationContext.Create(
                EvaluationContext.SharingPolicy.Shared,
                _fileSystem);

            if (_fileSystem is FileSystem.DeletedFilesOverlayFileSystem overlay)
                overlay.AttachProjectCollection(projectCollection);

            return new ProjectGraph(
                allProjects.Select(project => new ProjectGraphEntryPoint(project)),
                projectCollection,
                (projectPath, globalProperties, collection) => Project
                    .FromFile(projectPath, new ProjectOptions
                    {
                        GlobalProperties = globalProperties,
                        ProjectCollection = collection,
                        EvaluationContext = evaluationContext,
                        LoadSettings = ProjectLoadSettings.Default,
                    })
                    .CreateProjectInstance());
        }

        private void WriteLine(string? message = null)
        {
            // TODO: Logging
        }
    }
}
