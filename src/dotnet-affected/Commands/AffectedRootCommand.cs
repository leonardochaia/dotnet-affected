using Affected.Cli.Views;
using DotnetAffected.Core;
using System;
using System.CommandLine;
using System.CommandLine.Rendering;
using System.Linq;

namespace Affected.Cli.Commands
{
    internal class AffectedRootCommand : RootCommand
    {
        public static readonly FormatOption FormatOption = new();
        public static readonly DryRunOption DryRunOption = new();
        public static readonly OutputDirOption OutputDirOption = new();
        public static readonly OutputNameOption OutputNameOption = new();

        public AffectedRootCommand()
            : base("Determines which projects are affected by a set of changes.\n" +
                   "For examples and detailed descriptions see: " +
                   "https://github.com/leonardochaia/dotnet-affected/blob/main/README.md")
        {
            this.Name = "dotnet-affected";
            this.AddCommand(new DescribeCommand());

            this.AddGlobalOption(AffectedGlobalOptions.RepositoryPathOptions);
            this.AddGlobalOption(AffectedGlobalOptions.SolutionPathOption);
            this.AddGlobalOption(AffectedGlobalOptions.FilterFilePathOption);
            this.AddGlobalOption(AffectedGlobalOptions.VerboseOption);
            this.AddGlobalOption(AffectedGlobalOptions.AssumeChangesOption);
            this.AddGlobalOption(AffectedGlobalOptions.FromOption);
            this.AddGlobalOption(AffectedGlobalOptions.ToOption);
            this.AddGlobalOption(AffectedGlobalOptions.ExcludeOutputRegexOption);
            this.AddGlobalOption(AffectedGlobalOptions.ExcludeDiscoveryRegexOption);
            this.AddGlobalOption(AffectedGlobalOptions.ExclusionRegexOption);

            this.AddOption(FormatOption);
            this.AddOption(DryRunOption);
            this.AddOption(OutputDirOption);
            this.AddOption(OutputNameOption);

            this.SetHandler(async ctx =>
            {
                var (options, summary) = ctx.ExecuteAffectedExecutor();
                summary.ThrowIfNoChanges();

                var verbose = ctx.ParseResult.GetValueForOption(AffectedGlobalOptions.VerboseOption)!;
                var console = ctx.Console;
                if (verbose)
                {
                    var infoView = new AffectedInfoView(summary);
                    console.Append(infoView);
                }

                var allProjects = summary
                    .ProjectsWithChangedFiles
                    .Concat(summary.AffectedProjects)
                    .Select(p => new ProjectInfo(p));

                // Generate output using formatters
                var outputOptions = ctx.GetAffectedCommandOutputOptions(options);

                var formatterExecutor = new OutputFormatterExecutor(console);
                await formatterExecutor.Execute(
                    allProjects,
                    outputOptions.Formatters,
                    outputOptions.OutputDir,
                    outputOptions.OutputName,
                    options.FilterFilePath,
                    outputOptions.DryRun,
                    verbose);
            });
        }
    }

    internal sealed class FormatOption : Option<string[]>
    {
        public FormatOption()
            : base(new[]
            {
                "--format", "-f"
            })
        {
            this.Description = "Space-seperated output file formats. Possible values: <traversal, text, json, slnf>.";

            this.SetDefaultValue(new[]
            {
                "traversal"
            });
            this.AllowMultipleArgumentsPerToken = true;

            // slnf is the only format referencing a file other than its own,
            // fail at parse time instead of after building the whole graph.
            this.AddValidator(optionResult =>
            {
                var formats = optionResult.GetValueOrDefault<string[]>();
                if (formats is null || !formats.Any(format =>
                        format.Equals("slnf", StringComparison.InvariantCultureIgnoreCase)))
                {
                    return;
                }

                // --solution-path is the deprecated alias of --filter-file-path.
                var filterFilePath =
                    optionResult.FindResultFor(AffectedGlobalOptions.FilterFilePathOption)
                        ?.GetValueOrDefault<string>()
                    ?? optionResult.FindResultFor(AffectedGlobalOptions.SolutionPathOption)
                        ?.GetValueOrDefault<string>();

                if (!SolutionFilter.CanCreateFrom(filterFilePath))
                {
                    optionResult.ErrorMessage =
                        "The slnf format needs a Solution to reference. Point --filter-file-path at " +
                        "a Solution (.sln, .slnx) or a Solution Filter (.slnf).";
                }
            });
        }
    }

    internal sealed class DryRunOption : Option<bool>
    {
        public DryRunOption()
            : base(new[]
            {
                "--dry-run"
            })
        {
            this.Description = "Only output to stdout. No output files will be created.";
            this.SetDefaultValue(false);
        }
    }

    internal sealed class OutputDirOption : Option<string>
    {
        public OutputDirOption()
            : base(new[]
            {
                "--output-dir"
            })
        {
            this.Description = "The directory where the output file(s) will be generated.\n" +
                               "Relative paths will be based on --repository-path.";
        }
    }

    internal sealed class OutputNameOption : Option<string>
    {
        public OutputNameOption()
            : base(new[]
            {
                "--output-name"
            })
        {
            this.Description = "The filename to create.\n" +
                               "Format file extensions will be appended.";
            this.SetDefaultValue("affected");
        }
    }
}
