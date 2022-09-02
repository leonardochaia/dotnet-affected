﻿using Affected.Cli.Commands;
using Microsoft.Build.Graph;
using System.Collections.Generic;
using System.Linq;

namespace Affected.Cli
{
    /// <summary>
    /// Uses input parameters to fake which projects have changed.
    /// </summary>
    internal class AssumptionChangesProvider : IChangesProvider
    {
        private readonly ProjectGraph _graph;
        private readonly IEnumerable<string> _assumptions;

        public AssumptionChangesProvider(
            ProjectGraph graph,
            IEnumerable<string> assumptions)
        {
            _graph = graph;
            _assumptions = assumptions;
        }

        public IEnumerable<string> GetChangedFiles(string directory, string @from, string to)
        {
            // REMARKS: we are just selecting the MsBuild project as changed file
            return _graph
                .FindNodesByName(_assumptions)
                .Select(n => n.ProjectInstance.FullPath);
        }

        public (string FromText, string ToText) GetTextFileContents(string directory, string pathToFile, string from,
            string to)
        {
            throw new System.InvalidOperationException("--assume-changes should not try to access file contents");
        }
    }
}
