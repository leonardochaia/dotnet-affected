using Affected.Cli.Views;
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
        public static readonly OutputStrategyOption OutputStrategyOption = new();
        public static readonly OutputFilterOption OutputFilterOption = new();

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
            this.AddGlobalOption(AffectedGlobalOptions.ExclusionRegexOption);

            this.AddOption(FormatOption);
            this.AddOption(DryRunOption);
            this.AddOption(OutputDirOption);
            this.AddOption(OutputNameOption);
            this.AddOption(OutputStrategyOption);
            this.AddOption(OutputFilterOption);

            this.SetHandler(async ctx =>
            {
                var (options, summary) = ctx.ExecuteAffectedExecutor();
                summary.ThrowIfNoChanges();

                var verbose = ctx.ParseResult.GetValueForOption(AffectedGlobalOptions.VerboseOption);
                var console = ctx.Console;

                if (verbose)
                {
                    var infoView = new AffectedInfoView(summary);
                    console.Append(infoView);
                }

                var outputOptions = ctx.GetAffectedCommandOutputOptions(options);
                var filter = new OutputFilter(outputOptions);
                var projects = filter.GetFilteredProjects(summary);
                var outputFactory = new OutputStrategyFactory(outputOptions);
                var outputStrategy = outputFactory.CreateOutputStrategy(projects);

                foreach (IOutput output in outputStrategy.GetOutputs())
                {
                    // Generate output using formatters
                    var formatterExecutor = new OutputFormatterExecutor(console);
                    await formatterExecutor.Execute(
                        output.Projects,
                        outputOptions.Formatters,
                        output.Directory,
                        output.Name,
                        outputOptions.DryRun,
                        verbose);
                }
            });
        }
    }

    internal sealed class FormatOption : Option<string[]>
    {
        public FormatOption()
            : base(new[] { "--format", "-f" })
        {
            this.Description = "Space-seperated output file formats. Possible values: <traversal, text, json>.";

            this.SetDefaultValue(new[] { "traversal" });
            this.AllowMultipleArgumentsPerToken = true;
        }
    }

    internal sealed class OutputStrategyOption : Option<string>
    {
        public OutputStrategyOption()
            : base(new[] { "--output-strategy" })
        {
            this.Description =
                "Determines the output strategy. If set to \"combined\", all output will be written to a single file. If set to \"split\", files will be created based on affected, changed, and excluded projects.";
            this.SetDefaultValue(OutputStrategies.Combined);
            this.FromAmong(OutputStrategies.All.ToArray());
            this.AddCompletions(OutputStrategies.All.ToArray());
        }
    }
    
    internal sealed class OutputFilterOption : Option<string[]>
    {
        public OutputFilterOption()
            : base(new[] { "--output-filter" })
        {
            this.Description =
                "Determines the output strategy. If set to \"combined\", all output will be written to a single file. If set to \"split\", files will be created based on affected, changed, and excluded projects.";
            this.SetDefaultValue(new[] { OutputFilters.Affected, OutputFilters.Changed });
            this.FromAmong(OutputFilters.All.ToArray());
            this.AddCompletions(OutputFilters.All.ToArray());
        }
    }

    internal sealed class DryRunOption : Option<bool>
    {
        public DryRunOption()
            : base(new[] { "--dry-run" })
        {
            this.Description = "Only output to stdout. No output files will be created.";
            this.SetDefaultValue(false);
        }
    }

    internal sealed class OutputDirOption : Option<string>
    {
        public OutputDirOption()
            : base(new[] { "--output-dir" })
        {
            this.Description = "The directory where the output file(s) will be generated.\n" +
                               "Relative paths will be based on --repository-path.";
        }
    }

    internal sealed class OutputNameOption : Option<string>
    {
        public OutputNameOption()
            : base(new[] { "--output-name" })
        {
            this.Description = "The filename to create.\n" +
                               "Format file extensions will be appended.";
            this.SetDefaultValue("affected");
        }
    }
}
