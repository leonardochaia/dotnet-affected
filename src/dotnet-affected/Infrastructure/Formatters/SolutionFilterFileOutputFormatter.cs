using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Affected.Cli.Formatters
{
    internal class SolutionFilterFileOutputFormatter : IOutputFormatter
    {
        public string Type => "slnf";

        public string NewFileExtension => ".slnf";

        public static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true
        };

        public Task<string> Format(IEnumerable<IProjectInfo> projects, string? filterFilePath = null)
        {
            if (filterFilePath == null)
            {
                throw new ArgumentException("Path to Solution cannot be null for SolutionFilterFile output", "filterFilePath");
            }

            // Grab the directory the solutionLives in as all project references must be relative to the solution file
            string solutionDir = Path.GetDirectoryName(filterFilePath);

            IEnumerable<string> projectPaths = projects.Select(p => Path.GetRelativePath(solutionDir, p.FilePath));
            var solutionFilter = new SolutionFilter(new Solution(filterFilePath, projectPaths));
            return Task.FromResult(JsonSerializer.Serialize(solutionFilter, SerializerOptions));
        }

    }

    record SolutionFilter(Solution solution);
    record Solution(string path, IEnumerable<string> projects);
}
