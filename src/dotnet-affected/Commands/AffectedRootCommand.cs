using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Threading.Tasks;

namespace Affected.Cli.Commands
{
    internal class AffectedRootCommand : RootCommand
    {
        public AffectedRootCommand()
        {
            this.Name = "affected";
            this.Description = "Determines which projects are affected by a set of changes.";

            this.AddCommand(new DescribeCommand());

            this.AddGlobalOption(new RepositoryPathOptions());
            this.AddGlobalOption(new SolutionPathOption());
            this.AddGlobalOption(new VerboseOption());
            this.AddGlobalOption(new AssumeChangesOption());

            var fromOption = new FromOption();
            this.AddGlobalOption(fromOption);
            this.AddGlobalOption(new ToOption(fromOption));

            this.AddOption(new FormatOption());
            this.AddOption(new DryRunOption());
            this.AddOption(new OutputDirOption());
            this.AddOption(new OutputNameOption());

            // TODO: We need to specify the handler manually ONLY for the RootCommand
            this.Handler = CommandHandler.Create(
                typeof(AffectedCommandHandler).GetMethod(nameof(ICommandHandler.InvokeAsync))!);
        }

        public class AffectedCommandHandler : ICommandHandler
        {
            private readonly ICommandExecutionContext _context;
            private readonly IOutputFormatterExecutor _formatterExecutor;
            private readonly CommandExecutionData _data;
            private readonly IConsole _console;

            public AffectedCommandHandler(
                ICommandExecutionContext context,
                IOutputFormatterExecutor formatterExecutor,
                CommandExecutionData data,
                IConsole console)
            {
                _context = context;
                _formatterExecutor = formatterExecutor;
                _data = data;
                _console = console;
            }

            public Task<int> InvokeAsync(InvocationContext ic)
            {
                // TODO: OutputName & OutputDir
                _formatterExecutor.Execute(_context.ChangedProjects.Concat(_context.AffectedProjects),
                    _data.Formatters, _data.OutputDir, _data.OutputName, _data.DryRun, _data.Verbose);

                return Task.FromResult(0);
            }
        }

        private class AssumeChangesOption : Option<IEnumerable<string>>
        {
            public AssumeChangesOption()
                : base("--assume-changes")
            {
                this.Description =
                    "Hypothetically assume that given projects have changed instead of using Git diff to determine them.";
            }
        }

        private class RepositoryPathOptions : Option<string>
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

        private class SolutionPathOption : Option<string>
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

        private class VerboseOption : Option<bool>
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

        private class FromOption : Option<string>
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

        private class ToOption : Option<string>
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
                        return $"{fromOption.Aliases.First()} is required when using {this.Aliases.First()}";
                    }

                    return null;
                });
            }
        }

        private class FormatOption : Option<string[]>
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
            }
        }

        private class DryRunOption : Option<bool>
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
        
        private class OutputDirOption : Option<string>
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
        
        private class OutputNameOption : Option<string>
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
}
