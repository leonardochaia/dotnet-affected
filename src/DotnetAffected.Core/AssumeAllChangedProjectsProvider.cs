using DotnetAffected.Abstractions;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Graph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DotnetAffected.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class AssumeAllChangedProjectsProvider : IChangedProjectsProvider
    {
        private readonly ProjectGraph _graph;

        /// <summary>
        /// Creates an <see cref="AssumeAllChangedProjectsProvider"/>.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="assumptions"></param>
        public AssumeAllChangedProjectsProvider(
            ProjectGraph graph)
        {
            _graph = graph;
        }

        /// <summary>
        /// Returns all EntryPointNodes
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public IEnumerable<ProjectGraphNode> GetReferencingProjects(IEnumerable<string> files)
        {
            return _graph.EntryPointNodes;
        }
    }
}
