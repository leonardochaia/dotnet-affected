using Microsoft.Build.Execution;
using Microsoft.Build.Graph;
using Microsoft.Build.Prediction;
using System;
using System.Collections.Generic;

namespace Affected.Cli
{
    internal class FilesByProjectGraphCollector : IProjectPredictionCollector
    {
        private readonly Dictionary<ProjectInstance, ProjectInputFilesCollector> _collectorByProjectInstance;

        public Dictionary<ProjectGraphNode, ICollection<string>> PredictionsPerNode { get; }

        public FilesByProjectGraphCollector(ProjectGraph projectGraph, string repositoryPath)
        {
            var projectGraphNodes = projectGraph.ProjectNodes;

            PredictionsPerNode = new Dictionary<ProjectGraphNode, ICollection<string>>(projectGraphNodes.Count);

            _collectorByProjectInstance =
                new Dictionary<ProjectInstance, ProjectInputFilesCollector>(projectGraphNodes.Count);

            foreach (var projectGraphNode in projectGraphNodes)
            {
                var collector = new ProjectInputFilesCollector(repositoryPath);
                PredictionsPerNode.Add(projectGraphNode, collector.AllFiles);
                _collectorByProjectInstance.Add(projectGraphNode.ProjectInstance, collector);
            }
        }

        public void AddInputFile(string path, ProjectInstance projectInstance, string predictorName) =>
            GetProjectCollector(projectInstance)
                .AddInputFile(path, projectInstance, predictorName);

        public void AddInputDirectory(string path, ProjectInstance projectInstance, string predictorName)
        {
        }

        public void AddOutputFile(string path, ProjectInstance projectInstance, string predictorName)
        {
        }

        public void AddOutputDirectory(string path, ProjectInstance projectInstance, string predictorName)
        {
        }

        private ProjectInputFilesCollector GetProjectCollector(ProjectInstance projectInstance)
        {
            if (!_collectorByProjectInstance.TryGetValue(projectInstance,
                    out ProjectInputFilesCollector? collector))
            {
                throw new InvalidOperationException(
                    "Prediction collected for ProjectInstance not in the ProjectGraph");
            }

            return collector;
        }
    }
}
