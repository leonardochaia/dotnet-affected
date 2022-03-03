using Affected.Cli.Commands;
using System.Collections.Generic;
using System.Linq;

namespace Affected.Cli
{
    /// <summary>
    /// Uses input parameters to fake which projects have changed.
    /// </summary>
    internal class AssumptionChangesProvider : IChangesProvider
    {
        private readonly IProjectGraphRef _graph;
        private readonly CommandExecutionData _data;

        public AssumptionChangesProvider(
            IProjectGraphRef graph,
            CommandExecutionData data)
        {
            _graph = graph;
            _data = data;
        }

        public IEnumerable<string> GetChangedFiles(string directory, string @from, string to)
        {
            // REMARKS: we are just selecting the MsBuild project as changed file
            return _graph.Value
                .FindNodesByName(_data.AssumeChanges)
                .Select(n => n.ProjectInstance.FullPath);
        }
    }
}
