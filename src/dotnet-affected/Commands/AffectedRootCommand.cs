using Affected.Cli.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.Rendering;
using System.CommandLine.Rendering.Views;
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

            this.Handler = new CommandHandler();
        }

        public class CommandHandler : ICommandHandler
        {
            public Task<int> InvokeAsync(InvocationContext ic)
            {
                // TODO: Use constructor DI when command-line-api #1344 is fixed
                // https://github.com/dotnet/command-line-api/issues/1344
                var services = ic.GetHost().Services;
                var data = services.GetRequiredService<CommandExecutionData>();
                var context = services.GetRequiredService<ICommandExecutionContext>();
                var console = services.GetRequiredService<IConsole>();

                var affectedNodes = context.FindAffectedProjects().ToList();

                var rootView = new StackLayoutView();

                if (!affectedNodes.Any())
                {
                    if (data.Verbose)
                    {
                        rootView.Add(new NoChangesView());
                    }
                    
                    console.Append(rootView);
                    return Task.FromResult(AffectedExitCodes.NothingAffected);
                }

                rootView.Add(new WithChangesAndAffectedView(
                    context.NodesWithChanges,
                    affectedNodes));

                console.Append(rootView);
                return Task.FromResult(0);
            }
        }

        private class AssumeChangesOption : Option<IEnumerable<string>>
        {
            public AssumeChangesOption()
                : base("--assume-changes")
            {
                this.Description = "Hypothetically assume that given projects have changed instead of using Git diff to determine them.";
            }
        }

        private class RepositoryPathOptions : Option<string>
        {
            public RepositoryPathOptions()
                : base(
                      aliases: new[] { "--repository-path", "-p" })
            {
                this.Description = "Path to the root of the repository, where the .git directory is.";
                this.SetDefaultValueFactory(()=> Environment.CurrentDirectory);
            }
        }

        private class SolutionPathOption : Option<string>
        {
            public SolutionPathOption()
                : base(new [] { "--solution-path" })
            {
                this.Description = "Path to a Solution file (.sln) used to find all projects that may be affected. When omitted, will search for project files inside --repository-path.";
            }
        }

        private class VerboseOption : Option<bool>
        {
            public VerboseOption()
                : base(
                      aliases: new[] { "--verbose", "-v" },
                      getDefaultValue: () => false)
            {
                this.Description = "Write useful messages or just the desired output.";
            }
        }

        private class FromOption : Option<string>
        {
            public FromOption()
            : base(new[] { "--from" })
            {
                this.Description = "A branch or commit to compare against --to.";
            }
        }

        private class ToOption : Option<string>
        {
            public ToOption(FromOption fromOption)
                : base(new[] { "--to" })
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
