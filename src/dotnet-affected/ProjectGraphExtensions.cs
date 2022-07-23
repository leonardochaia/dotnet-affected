using Microsoft.Build.Graph;
using Microsoft.Build.Prediction;
using Microsoft.Build.Prediction.Predictors;
using Microsoft.Build.Prediction.Predictors.CopyTask;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Affected.Cli
{
    internal static class ProjectGraphExtensions
    {
        private static readonly ConcurrentDictionary<string, IEnumerable<ProjectGraphNode>> Cache = new();

        private static readonly ProjectFileAndImportsGraphPredictor[] GraphPredictors = new[]
        {
            new ProjectFileAndImportsGraphPredictor()
        };

        private static readonly IProjectPredictor[] ProjectPredictors = new[]
        {
            new AvailableItemNameItemsPredictor(), new ContentItemsPredictor(), new NoneItemsPredictor(),
            (IProjectPredictor)new CopyTaskPredictor(), new CompileItemsPredictor(),
            new IntermediateOutputPathPredictor(), new ProjectFileAndImportsPredictor(),
            new CodeAnalysisRuleSetPredictor(), new AssemblyOriginatorKeyFilePredictor(),
            new EmbeddedResourceItemsPredictor(), new ReferenceItemsPredictor(), new ArtifactsSdkPredictor(),
            new StyleCopPredictor(), new ManifestsPredictor(), new VSCTCompileItemsPredictor(),
            new EditorConfigFilesItemsPredictor(), new ApplicationIconPredictor(),
            new GeneratePackageOnBuildPredictor(), new DocumentationFilePredictor(), new XamlAppDefPredictor(),
            new TypeScriptCompileItemsPredictor(), new ApplicationDefinitionItemsPredictor(), new PageItemsPredictor(),
            new ResourceItemsPredictor(), new SplashScreenItemsPredictor(), new TsConfigPredictor(),
            new MasmItemsPredictor(), new ClIncludeItemsPredictor(), new SqlBuildPredictor(),
            new AdditionalIncludeDirectoriesPredictor(), new AnalyzerItemsPredictor(),
            new ModuleDefinitionFilePredictor(), new CppContentFilesProjectOutputGroupPredictor(),
            new LinkItemsPredictor()
        };

        private static readonly ProjectGraphPredictionExecutor Executor = new ProjectGraphPredictionExecutor(
            GraphPredictors,
            ProjectPredictors);

        /// <summary>
        /// REMARKS: we have other means for detecting changes excluded files 
        /// </summary>
        private static readonly string[] FileExclusions = new[]
        {
            // Predictors won't take into account package references
            "Directory.Packages.props"
        };

        /// <summary>
        /// Recursively searches for all <see cref="ProjectGraphNode.ReferencingProjects"/>
        /// in all provided projects.
        /// </summary>
        /// <param name="targetNodes"></param>
        /// <returns></returns>
        public static IEnumerable<ProjectGraphNode> FindReferencingProjects(
            this IEnumerable<ProjectGraphNode> targetNodes)
        {
            var added = new HashSet<string>();
            foreach (var node in targetNodes)
            {
                foreach (var affected in FindReferencingProjects(node))
                {
                    if (added.Add(affected.ProjectInstance.FullPath))
                        yield return affected;
                }
            }
        }

        private static IEnumerable<ProjectGraphNode> FindReferencingProjectsImpl(ProjectGraphNode node)
        {
            var added = new HashSet<string>();
            foreach (var referencingProject in node.ReferencingProjects)
            {
                // Return all referencing projects
                if (added.Add(referencingProject.ProjectInstance.FullPath))
                    yield return referencingProject;

                // Recurse each node's children
                foreach (var child in FindReferencingProjects(referencingProject))
                {
                    if (added.Add(child.ProjectInstance.FullPath))
                        yield return child;
                }
            }
        }

        /// <summary>
        /// Recursively searches for <see cref="ProjectGraphNode.ReferencingProjects"/>
        /// </summary>
        /// <param name="targetNode"></param>
        /// <returns></returns>
        public static IEnumerable<ProjectGraphNode> FindReferencingProjects(
            this ProjectGraphNode targetNode)
        {
            return Cache.GetOrAdd(
                targetNode.ProjectInstance.FullPath,
                _ => FindReferencingProjectsImpl(targetNode)
                    .ToList());
        }

        internal static IEnumerable<ProjectGraphNode> FindNodesContainingFiles(
            this ProjectGraph graph,
            IEnumerable<string> files)
        {
            var hasReturned = new HashSet<string>();

            var predictions = Executor.PredictInputsAndOutputs(graph)
                .PredictionsPerNode
                .ToArray();

            // normalize paths so that they match on windows.
            var normalizedFiles = files
                .Where(f => !FileExclusions.Any(f.EndsWith))
                .Select(Path.GetFullPath);

            foreach (var file in normalizedFiles)
            {
                // determine nodes depending on the changed file
                var nodesWithFiles = predictions
                    .Where(x => x.Value.InputFiles
                        .Any(i => i.Path == file));

                foreach (var (key, _) in nodesWithFiles)
                {
                    if (key is null) continue;
                    if (hasReturned.Add(key.ProjectInstance.FullPath))
                    {
                        yield return key;
                    }
                }
            }
        }

        internal static IEnumerable<ProjectGraphNode> FindNodesReferencingNuGetPackages(
            this ProjectGraph graph,
            IEnumerable<string> nuGetPackageNames)
        {
            var hasReturned = new HashSet<string>();
            foreach (var nuget in nuGetPackageNames)
            {
                var nodes = graph.ProjectNodes
                    .Where(n => !n.IsOptedOutFromCentrallyManagedNuGetPackageVersions()
                                && n.ReferencesNuGetPackage(nuget));

                foreach (var node in nodes)
                {
                    if (hasReturned.Add(node.ProjectInstance.FullPath))
                    {
                        yield return node;
                    }
                }
            }
        }

        internal static ProjectGraphNode? FindNodeByPath(
            this ProjectGraph graph,
            string projectPath)
        {
            return graph.ProjectNodes
                .FirstOrDefault(n => n.ProjectInstance.FullPath == projectPath);
        }

        internal static ProjectGraphNode? FindNodeByName(
            this ProjectGraph graph,
            string name)
        {
            return graph.ProjectNodes
                .FirstOrDefault(n => n.GetProjectName()
                    .Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        internal static IEnumerable<ProjectGraphNode> FindNodesByName(
            this ProjectGraph graph,
            IEnumerable<string> names)
        {
            foreach (var projName in names)
            {
                var node = graph.FindNodeByName(projName);
                if (node is null)
                {
                    throw new InvalidOperationException($"Couldn't find project with name {projName}");
                }

                yield return node;
            }
        }
    }
}
