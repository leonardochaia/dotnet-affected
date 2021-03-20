using System;
using System.Collections.Generic;
using System.CommandLine;
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
        private readonly IConsole console;

        public CommandExecutionData(
           string? repositoryPath,
           string? from,
           string to,
           bool verbose,
           IEnumerable<string>? assumeChanges,
           IConsole console)
        {
            this.RepositoryPath = repositoryPath ?? Environment.CurrentDirectory;
            this.To = to;
            this.From = from;
            this.Verbose = verbose;
            this.AssumeChanges = assumeChanges ?? Enumerable.Empty<string>();
            this.console = console;
        }

        public string RepositoryPath { get; }

        public string? From { get; }

        public string To { get; }

        public bool Verbose { get; }

        public IEnumerable<string> AssumeChanges { get; }

        public CommandExecutionContext BuildExecutionContext()
        {
            return new CommandExecutionContext(this, this.console);
        }
    }
}
