using Affected.Cli.Views;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Rendering;
using System.Linq;

namespace Affected.Cli.Commands
{
    internal class AffectedRootCommand : RootCommand
    {
        private static readonly RepositoryPathOptions RepositoryPathOptions = new();
        private static readonly SolutionPathOption SolutionPathOption = new();
        private static readonly VerboseOption VerboseOption = new();
        private static readonly AssumeChangesOption AssumeChangesOption = new();
        private static readonly FromOption FromOption = new();
        private static readonly ToOption ToOption = new(FromOption);
        private static readonly FormatOption FormatOption = new();
        private static readonly DryRunOption DryRunOption = new();
        private static readonly OutputDirOption OutputDirOption = new();
        private static readonly OutputNameOption OutputNameOption = new();

        public static readonly CommandExecutionDataBinder DataBinder = new(RepositoryPathOptions,
            SolutionPathOption,
            FromOption,
            ToOption, VerboseOption, AssumeChangesOption, FormatOption, DryRunOption, OutputDirOption,
            OutputNameOption);

        public AffectedRootCommand()
            : base("Determines which projects are affected by a set of changes.")
        {
            this.AddCommand(new DescribeCommand());

            this.AddGlobalOption(RepositoryPathOptions);
            this.AddGlobalOption(SolutionPathOption);
            this.AddGlobalOption(VerboseOption);
            this.AddGlobalOption(AssumeChangesOption);
            this.AddGlobalOption(FromOption);
            this.AddGlobalOption(ToOption);

            this.AddOption(FormatOption);
            this.AddOption(DryRunOption);
            this.AddOption(OutputDirOption);
            this.AddOption(OutputNameOption);

            this.SetHandler(async ctx =>
            {
                var console = ctx.Console;
                var data = ctx.GetCommandExecutionData(DataBinder);
                var executor = data.BuildAffectedExecutor();

                var formatterExecutor = new OutputFormatterExecutor(console);

                var summary = executor.Execute();
                summary.ThrowIfNoChanges();

                if (data.Verbose)
                {
                    var infoView = new AffectedInfoView(summary);

                    console.Append(infoView);
                }

                var allProjects = summary.ProjectsWithChangedFiles.Concat(summary.AffectedProjects)
                    .Select(p => new ProjectInfo(p));

                await formatterExecutor.Execute(
                    allProjects,
                    data.Formatters,
                    data.OutputDir,
                    data.OutputName,
                    data.DryRun,
                    data.Verbose);
            });
        }
    }

    internal sealed class AssumeChangesOption : Option<IEnumerable<string>>
    {
        public AssumeChangesOption()
            : base("--assume-changes")
        {
            this.Description =
                "Hypothetically assume that given projects have changed instead of using Git diff to determine them.";
        }
    }

    internal sealed class RepositoryPathOptions : Option<string>
    {
        public RepositoryPathOptions()
            : base(
                aliases: new[]
                {
                    "--repository-path", "-p"
                })
        {
            this.Description = "Path to the root of the repository, where the .git directory is.\n" +
                               "[Defaults to current directory, or solution's directory when using --solution-path]";
        }
    }

    internal sealed class SolutionPathOption : Option<string>
    {
        public SolutionPathOption()
            : base(new[]
            {
                "--solution-path"
            })
        {
            this.Description =
                "Path to a Solution file (.sln) used to discover projects that may be affected.\n" +
                "When omitted, will search for project files inside --repository-path.";
        }
    }

    internal sealed class VerboseOption : Option<bool>
    {
        public VerboseOption()
            : base(
                aliases: new[]
                {
                    "--verbose", "-v"
                },
                getDefaultValue: () => false)
        {
            this.Description = "Write useful messages or just the desired output.";
        }
    }

    internal sealed class FromOption : Option<string>
    {
        public FromOption()
            : base(new[]
            {
                "--from"
            })
        {
            this.Description = "A branch or commit to compare against --to.";
        }
    }

    internal sealed class ToOption : Option<string>
    {
        public ToOption(FromOption fromOption)
            : base(new[]
            {
                "--to"
            })
        {
            this.Description = "A branch or commit to compare against --from";

            this.AddValidator(optionResult =>
            {
                if (optionResult.FindResultFor(fromOption) is null)
                {
                    optionResult.ErrorMessage =
                        $"{fromOption.Aliases.First()} is required when using {this.Aliases.First()}";
                }
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
            this.Description = "Space separated list of formatters to write the output.";
            this.SetDefaultValue(new[]
            {
                "traversal"
            });
            this.AllowMultipleArgumentsPerToken = true;
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
            this.Description = "Doesn't create files, outputs to stdout instead.";
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
            this.Description = "The directory where the output file(s) will be generated\n" +
                               "If relative, it's relative to the --repository-path";
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
            this.Description = "The name for the file to create for each format.\n" +
                               "Format extension is appended to this name.";
            this.SetDefaultValue("affected");
        }
    }
}
