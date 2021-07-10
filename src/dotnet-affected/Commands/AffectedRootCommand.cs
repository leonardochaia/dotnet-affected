using Affected.Cli.Views;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Rendering;
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

            this.AddCommand(new GenerateCommand());
            this.AddCommand(new ChangesCommand());

            this.AddGlobalOption(new RepositoryPathOptions());
            this.AddGlobalOption(new SolutionPathOption());
            this.AddGlobalOption(new VerboseOption());
            this.AddGlobalOption(new AssumeChangesOption());

            var fromOption = new FromOption();
            this.AddGlobalOption(fromOption);
            this.AddGlobalOption(new ToOption(fromOption));

            // TODO: We need to specify the handler manually ONLY for the RootCommand
            this.Handler = CommandHandler.Create(
                typeof(AffectedCommandHandler).GetMethod(nameof(ICommandHandler.InvokeAsync))!);
        }

        public class AffectedCommandHandler : ICommandHandler
        {
            private readonly ICommandExecutionContext _context;
            private readonly IConsole _console;

            public AffectedCommandHandler(
                ICommandExecutionContext context,
                IConsole console)
            {
                _context = context;
                _console = console;
            }

            public Task<int> InvokeAsync(InvocationContext ic)
            {
                if (!_context.ChangedProjects.Any())
                {
                    throw new NoChangesException();
                }

                var affectedNodes = _context.AffectedProjects.ToList();
                _console.Append(new WithChangesAndAffectedView(
                    _context.ChangedProjects,
                    affectedNodes));

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
                    "Path to a Solution file (.sln) used to discover projects that may be affected. " +
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
    }
}
