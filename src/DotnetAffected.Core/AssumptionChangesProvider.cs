using DotnetAffected.Abstractions;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Graph;
using System.Collections.Generic;
using System.Linq;

namespace DotnetAffected.Core
{
    /// <summary>
    /// Uses input parameters to fake which projects have changed.
    /// </summary>
    public class AssumptionChangesProvider : IChangesProvider
    {
        private readonly ProjectGraph _graph;
        private readonly IEnumerable<string> _assumptions;

        /// <summary>
        /// Creates an <see cref="AssumptionChangesProvider"/>.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="assumptions"></param>
        public AssumptionChangesProvider(
            ProjectGraph graph,
            IEnumerable<string> assumptions)
        {
            _graph = graph;
            _assumptions = assumptions;
        }

        /// <inheritdoc />
        public IEnumerable<string> GetChangedFiles(string directory, string @from, string to)
        {
            // REMARKS: we are just selecting the MsBuild project as changed file
            return _graph
                .FindNodesByName(_assumptions)
                .Select(n => n.ProjectInstance.FullPath);
        }
        
        /// <inheritdoc />
        public Project? LoadProject(string directory, string pathToFile, string? commitRef, bool fallbackToHead)
        {
            throw new System.InvalidOperationException("--assume-changes should not try to access file contents");
        }

        /// <inheritdoc />
        public Project? LoadDirectoryPackagePropsProject(string directory, string pathToFile, string? commitRef, bool fallbackToHead)
        {
            throw new System.InvalidOperationException("--assume-changes should not try to access file contents");
        }
    }
}
