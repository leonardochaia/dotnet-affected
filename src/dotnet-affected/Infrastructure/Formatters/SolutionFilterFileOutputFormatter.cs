using DotnetAffected.Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Affected.Cli.Formatters
{
    /// <summary>
    /// Writes a Solution Filter file (.slnf) narrowing the Solution
    /// down to the affected projects.
    /// </summary>
    internal class SolutionFilterFileOutputFormatter : IOutputFormatter
    {
        public string Type => "slnf";

        public string NewFileExtension => ".slnf";

        public Task<string> Format(IEnumerable<IProjectInfo> projects, OutputFormatterContext context)
        {
            var filter = SolutionFilter.Create(context.FilterFilePath, projects.Select(p => p.FilePath));

            // Paths are made relative to the file being written, so that
            // the filter stays usable wherever the output directory is.
            return Task.FromResult(filter.ToJson(context.OutputPath));
        }
    }
}
