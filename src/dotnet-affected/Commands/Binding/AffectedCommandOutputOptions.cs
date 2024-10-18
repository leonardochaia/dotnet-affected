using System.IO;

namespace Affected.Cli.Commands
{
    /// <summary>
    /// Holds all the information required for dotnet-affected command output.
    /// </summary>
    internal class AffectedCommandOutputOptions
    {
        public AffectedCommandOutputOptions(
            string repositoryPath,
            string? outputDir,
            string outputName,
            string[] formatters,
            string[] outputFilters,
            string outputStrategy,
            bool dryRun)
        {
            OutputDir = DetermineOutputDir(repositoryPath, outputDir);
            OutputName = outputName;
            Formatters = formatters;
            OutputFilters = outputFilters;
            OutputStrategy = outputStrategy;
            DryRun = dryRun;
        }

        public string[] OutputFilters { get; }
        public string OutputDir { get; }
        public string OutputName { get; }
        public string[] Formatters { get; }
        public string OutputStrategy { get; }
        public bool DryRun { get; }

        private static string DetermineOutputDir(string repositoryPath, string? outDir)
        {
            if (string.IsNullOrWhiteSpace(outDir))
            {
                return repositoryPath;
            }

            return Path.IsPathFullyQualified(outDir) ? outDir : Path.Combine(repositoryPath, outDir);
        }
    }
}
