using System.Collections.Generic;
using System.CommandLine;
using System.Linq;

namespace Affected.Cli.Commands
{
    internal static class AffectedGlobalOptions
    {
        public static readonly Option<string> RepositoryPathOptions = new(
            aliases: new[]
            {
                "--repository-path", "-p"
            },
            description: "Path to the root of the repository, where the .git directory is.\n" +
                         "[Defaults to current directory, or solution's directory when using --solution-path]");

        public static readonly Option<string> SolutionPathOption = new(
            aliases: new[]
            {
                "--solution-path"
            },
            description: "Path to a Solution file (.sln) used to discover projects that may be affected.\n" +
                         "When omitted, will search for project files inside --repository-path.");

        public static readonly Option<bool> VerboseOption = new(aliases: new[]
            {
                "--verbose", "-v"
            },
            getDefaultValue: () => false,
            description: "Write useful messages or just the desired output.");

        public static readonly Option<IEnumerable<string>> AssumeChangesOption = new(
            aliases: new[]
            {
                "--assume-changes"
            },
            description:
            "Hypothetically assume that given projects have changed instead of using Git diff to determine them.");

        public static readonly Option<string> FromOption = new(
            new[]
            {
                "--from"
            },
            description: "A branch or commit to compare against --to.");

        public static readonly ToOption ToOption = new(FromOption);
    }

    internal sealed class ToOption : Option<string>
    {
        public ToOption(Option<string> fromOption)
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
}
