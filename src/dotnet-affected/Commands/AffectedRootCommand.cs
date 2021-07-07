using Affected.Cli.Views;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;

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

            this.Handler = CommandHandler.Create<CommandExecutionData, ViewRenderingContext>(this.AffectedHandler);
        }

        private void AffectedHandler(
            CommandExecutionData data,
            ViewRenderingContext renderingContext)
        {
            using var context = data.BuildExecutionContext();
            var affectedNodes = context.FindAffectedProjects();

            var rootView = new WithChangesAndAffectedView(
                context.NodesWithChanges,
                affectedNodes);

            renderingContext.Render(rootView);
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
                this.Description = "Path to the root of the repository, where the .git directory is." +
                    Environment.NewLine +
                    "[default: current directory]";
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
