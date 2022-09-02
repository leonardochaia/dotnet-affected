using Microsoft.Build.Execution;
using Microsoft.Build.Prediction;
using System.Collections.Generic;
using System.IO;

namespace DotnetAffected.Core
{
    /// <summary>
    /// Keeps track of each <see cref="ProjectInstance"/> and its predictions.
    /// Inspired from https://github.com/microsoft/MSBuildPrediction/blob/c9bcdea11c06102d8c21db89acb11a99198670fd/src/BuildPrediction/DefaultProjectPredictionCollector.cs#L1
    /// Striped down version to only store what we need.
    /// </summary>
    internal class ProjectInputFilesCollector : IProjectPredictionCollector
    {
        private readonly string _repositoryPath;
        public HashSet<string> AllFiles { get; } = new HashSet<string>();

        public ProjectInputFilesCollector(string repositoryPath)
        {
            _repositoryPath = repositoryPath;
        }

        public void AddInputFile(string path, ProjectInstance projectInstance, string predictorName)
        {
            // Make the path absolute if needed.
            if (!Path.IsPathRooted(path))
            {
                path = Path.GetFullPath(Path.Combine(projectInstance.Directory, path));
            }
            else if (!path.StartsWith(_repositoryPath))
            {
                // ignore files outside the project's directory
                return;
            }

            this.AllFiles.Add(path);
        }

        public void AddInputDirectory(string path, ProjectInstance projectInstance, string predictorName)
        {
        }

        public void AddOutputFile(string path, ProjectInstance projectInstance, string predictorName)
        {
        }

        public void AddOutputDirectory(string path, ProjectInstance projectInstance, string predictorName)
        {
        }
    }
}
