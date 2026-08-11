using System;

namespace Affected.Cli
{
    /// <summary>
    /// Information about the output being generated, for
    /// <see cref="IOutputFormatter"/>s that need more than the list of projects.
    /// </summary>
    public sealed class OutputFormatterContext
    {
        /// <summary>
        /// Creates a new instance of <see cref="OutputFormatterContext"/>.
        /// </summary>
        /// <param name="outputPath">Full path to the file being written.</param>
        /// <param name="filterFilePath">Path to the filter file projects were discovered from, if any.</param>
        public OutputFormatterContext(string outputPath, string? filterFilePath = null)
        {
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                throw new ArgumentException("A path to the output file is required.", nameof(outputPath));
            }

            OutputPath = outputPath;
            FilterFilePath = filterFilePath;
        }

        /// <summary>
        /// Gets the full path to the file being written.
        /// Formatters writing paths should make them relative to its directory,
        /// so that the file stays portable.
        /// </summary>
        public string OutputPath { get; }

        /// <summary>
        /// Gets the path to the filter file the projects were discovered from,
        /// which is <c>null</c> when projects were discovered from the file system.
        /// </summary>
        public string? FilterFilePath { get; }
    }
}
