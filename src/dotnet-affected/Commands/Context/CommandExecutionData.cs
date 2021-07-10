using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Affected.Cli.Commands
{
    /// <summary>
    /// This class is resolved by <see cref="System.CommandLine"/>
    /// automatically. It "injects" the value of global options
    /// and also the ones of the current command being executed.
    /// </summary>
    internal class CommandExecutionData
    {
        public CommandExecutionData(
            string repositoryPath,
            string solutionPath,
            string from,
            string to,
            bool verbose,
            IEnumerable<string>? assumeChanges)
        {
            this.RepositoryPath = DetermineRepositoryPath(repositoryPath, solutionPath);
            this.SolutionPath = solutionPath;
            this.To = to;
            this.From = from;
            this.Verbose = verbose;
            this.AssumeChanges = assumeChanges ?? Enumerable.Empty<string>();
        }

        public string RepositoryPath { get; }

        public string SolutionPath { get; }

        public string From { get; }

        public string To { get; }

        public bool Verbose { get; }

        public IEnumerable<string> AssumeChanges { get; }

        private static string DetermineRepositoryPath(string repositoryPath, string solutionPath)
        {
            // the argument takes precedence.
            if (!string.IsNullOrWhiteSpace(repositoryPath))
            {
                return repositoryPath;
            }

            // if no arguments, then use current directory
            if (string.IsNullOrWhiteSpace(solutionPath))
            {
                return Environment.CurrentDirectory;
            }

            // When using solution, and no path specified, assume the solution's directory
            var solutionDirectory = Path.GetDirectoryName(solutionPath);
            if (string.IsNullOrWhiteSpace(solutionDirectory))
            {
                throw new InvalidOperationException(
                    $"Failed to determine directory from solution path {solutionPath}");
            }

            return solutionDirectory;
        }
    }
}
