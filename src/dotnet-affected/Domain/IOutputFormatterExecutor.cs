using System.Collections.Generic;
using System.Threading.Tasks;

namespace Affected.Cli
{
    internal interface IOutputFormatterExecutor
    {
        Task Execute(
            IEnumerable<IProjectInfo> projects,
            IEnumerable<string> formatters,
            string outputDirectory,
            string outputName,
            bool dryRun,
            bool verbose = false);
    }
}
