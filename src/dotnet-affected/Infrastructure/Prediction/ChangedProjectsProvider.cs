using Affected.Cli.Commands;
using Microsoft.Build.Graph;
using Microsoft.Build.Prediction;
using Microsoft.Build.Prediction.Predictors;
using Microsoft.Build.Prediction.Predictors.CopyTask;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Affected.Cli
{
    internal class ChangedProjectsProvider : IChangedProjectsProvider
    {
        private readonly IProjectGraphRef _graph;

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

        private readonly ProjectGraphPredictionExecutor _executor = new ProjectGraphPredictionExecutor(
            GraphPredictors,
            ProjectPredictors);

        /// <summary>
        /// REMARKS: we have other means for detecting changes excluded files 
        /// </summary>
        private readonly string[] _fileExclusions = new[]
        {
            // Predictors won't take into account package references
            "Directory.Packages.props"
        };

        private readonly string _repositoryPath;

        public ChangedProjectsProvider(
            IProjectGraphRef graph,
            CommandExecutionData data)
        {
            _graph = graph;
            _repositoryPath = data.RepositoryPath;
        }

        public IEnumerable<ProjectGraphNode> GetReferencingProjects(
            IEnumerable<string> files)
        {
            var hasReturned = new HashSet<string>();

            var collector = new FilesByProjectGraphCollector(this._graph.Value, this._repositoryPath);
            _executor.PredictInputsAndOutputs(_graph.Value, collector);

            // normalize paths so that they match on windows.
            var normalizedFiles = files
                .Where(f => !_fileExclusions.Any(f.EndsWith))
                .Select(Path.GetFullPath);

            foreach (var file in normalizedFiles)
            {
                // determine nodes depending on the changed file
                var nodesWithFiles = collector.PredictionsPerNode
                    .Where(x => x.Value.Contains(file));

                foreach (var (key, _) in nodesWithFiles)
                {
                    if (hasReturned.Add(key.ProjectInstance.FullPath))
                    {
                        yield return key;
                    }
                }
            }
        }
    }
}
