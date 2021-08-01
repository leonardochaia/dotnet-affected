using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.IO;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Affected.Cli
{
    internal class OutputFormatterExecutor : IOutputFormatterExecutor
    {
        private readonly IEnumerable<IOutputFormatter> _formatters;
        private readonly IConsole _console;

        public OutputFormatterExecutor(
            IEnumerable<IOutputFormatter> formatters,
            IConsole console)
        {
            _formatters = formatters;
            _console = console;
        }

        public async Task Execute(IEnumerable<IProjectInfo> projects,
            IEnumerable<string> formatters,
            string outputDirectory,
            string outputName,
            bool dryRun,
            bool verbose = false)
        {
            // build a dictionary of requested formatter to formatter instance
            var formatterDictionary = formatters
                .ToDictionary(t => t, FindFormatter);

            var allProjects = projects.ToList();

            foreach (var (type, formatter) in formatterDictionary)
            {
                if (formatter is null)
                {
                    throw new InvalidOperationException("Couldn't find formatter of type: " + type);
                }

                // Format the projects and calculate output path.
                var outputContents = await formatter.Format(allProjects);

                if (string.IsNullOrWhiteSpace(outputContents))
                {
                    throw new InvalidOperationException($"Formatter {type} returned no output");
                }

                var outputFileName = outputName + formatter.NewFileExtension;
                var outputPath = Path.Combine(outputDirectory, outputFileName);

                if (dryRun)
                {
                    _console.Out.WriteLine($"DRY-RUN: WRITE {outputPath}");
                    _console.Out.WriteLine($"DRY-RUN: CONTENTS:");
                    _console.Out.Write(outputContents);
                    _console.Out.WriteLine();
                }
                else
                {
                    _console.Out.WriteLine($"WRITE: {outputPath}");
                    Directory.CreateDirectory(outputDirectory);
                    await File.WriteAllTextAsync(outputPath, outputContents);
                }
            }
        }

        private IOutputFormatter? FindFormatter(string type)
        {
            return _formatters.SingleOrDefault(f => f.Type.Equals(type, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
