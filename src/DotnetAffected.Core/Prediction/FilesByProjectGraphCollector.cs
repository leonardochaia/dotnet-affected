using Microsoft.Build.Execution;
using Microsoft.Build.Graph;
using Microsoft.Build.Prediction;
using System;
using System.Collections.Generic;
using System.IO;

namespace DotnetAffected.Core
{
    /// <summary>
    /// Records which <see cref="ProjectGraphNode"/> predicted one of a known set of files as an
    /// input.
    /// Inspired from https://github.com/microsoft/MSBuildPrediction/blob/c9bcdea11c06102d8c21db89acb11a99198670fd/src/BuildPrediction/DefaultProjectGraphPredictionCollector.cs#L1
    /// Striped down version to only store what we need.
    /// </summary>
    /// <remarks>
    /// The files being looked for are known before prediction starts, so a predicted input is
    /// matched as it arrives and then dropped. Keeping the inputs per node instead and searching
    /// them afterwards — what this used to do — holds every predicted input live until matching
    /// is over, and then costs a lookup per node per changed file to search, which grows as the
    /// product of the size of the repository and the size of the change. On a 4000 node graph
    /// with 800 changed files the two together are about a third of the attribution phase.
    ///
    /// <see cref="ProjectGraphPredictionExecutor"/> walks the graph nodes in parallel and runs
    /// each project's predictors in parallel as well, all reporting to this one instance, so
    /// <see cref="AddInputFile"/> is called concurrently. Everything it reads is built in the
    /// constructor and never written to again; the matches are the only shared mutable state and
    /// a match is rare, so the lock around them is uncontended.
    /// </remarks>
    internal sealed class FilesByProjectGraphCollector : IProjectPredictionCollector
    {
        private readonly string _repositoryPath;

        private readonly Dictionary<ProjectInstance, ProjectGraphNode> _nodesByProjectInstance;

        /// <summary>
        /// Normalized full paths of the files being looked for.
        /// </summary>
        private readonly HashSet<string> _files;

        /// <summary>
        /// The file names in <see cref="_files"/>. A predicted input whose name is not in here
        /// cannot match, and rejecting on the name leaves <see cref="Path.GetFullPath(string)"/>
        /// to run on the few candidates rather than on every input the predictors produce.
        /// </summary>
        private readonly HashSet<string> _fileNames;

        private readonly HashSet<ProjectGraphNode> _nodesWithChanges = new HashSet<ProjectGraphNode>();

        public FilesByProjectGraphCollector(
            ProjectGraph projectGraph,
            string repositoryPath,
            IEnumerable<string> files)
        {
            _repositoryPath = repositoryPath;
            _files = new HashSet<string>(files);

            _fileNames = new HashSet<string>();
            foreach (var file in _files)
            {
                _fileNames.Add(Path.GetFileName(file));
            }

            var projectGraphNodes = projectGraph.ProjectNodes;

            _nodesByProjectInstance =
                new Dictionary<ProjectInstance, ProjectGraphNode>(projectGraphNodes.Count);

            foreach (var projectGraphNode in projectGraphNodes)
            {
                _nodesByProjectInstance.Add(projectGraphNode.ProjectInstance, projectGraphNode);
            }
        }

        /// <summary>
        /// Whether the node predicted one of the files as an input. Only meaningful once
        /// prediction has finished.
        /// </summary>
        public bool HasChanges(ProjectGraphNode projectGraphNode)
            => _nodesWithChanges.Contains(projectGraphNode);

        public void AddInputFile(string path, ProjectInstance projectInstance, string predictorName)
        {
            // Most of what the predictors report is the SDK's own props and targets: rooted,
            // outside the repository, and rejected here without touching the string at all. The
            // checks below are in increasing order of cost for that reason.
            if (Path.IsPathRooted(path) && !path.StartsWith(_repositoryPath))
            {
                // ignore files outside the project's directory
                return;
            }

            // A file name survives Path.Combine and Path.GetFullPath below, so it can be matched
            // before either of them runs.
            if (!_fileNames.Contains(Path.GetFileName(path)))
            {
                return;
            }

            // Make the path absolute if needed.
            if (!Path.IsPathRooted(path))
            {
                path = Path.Combine(projectInstance.Directory, path);
            }

            // A project can reach a file through a path that is not normalized, so both sides of
            // the comparison are normalized.
            if (!_files.Contains(Path.GetFullPath(path)))
            {
                return;
            }

            var projectGraphNode = GetProjectGraphNode(projectInstance);

            lock (_nodesWithChanges)
            {
                _nodesWithChanges.Add(projectGraphNode);
            }
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

        private ProjectGraphNode GetProjectGraphNode(ProjectInstance projectInstance)
        {
            if (!_nodesByProjectInstance.TryGetValue(projectInstance,
                    out ProjectGraphNode? projectGraphNode))
            {
                throw new InvalidOperationException(
                    "Prediction collected for ProjectInstance not in the ProjectGraph");
            }

            return projectGraphNode;
        }
    }
}
