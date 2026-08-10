using DotnetAffected.Core;
using System.CommandLine.Binding;

namespace Affected.Cli.Commands
{
    /// <summary>
    /// Build the <see cref="AffectedOptions"/> based on the CLI's input options.
    /// </summary>
    internal class AffectedOptionsBinder : BinderBase<AffectedOptions>
    {
        protected override AffectedOptions GetBoundValue(BindingContext bindingContext)
        {
            var parseResult = bindingContext.ParseResult;

            // solutionFilePath is deprecated,
            var filterFilePath = parseResult.GetValueForOption(AffectedGlobalOptions.FilterFilePathOption);
            var solutionFilePath = parseResult.GetValueForOption(AffectedGlobalOptions.SolutionPathOption);

            if (string.IsNullOrEmpty(filterFilePath))
            {
                filterFilePath = solutionFilePath;
            }
            
            // --exclude is deprecated in favour of --exclude-output, which wins when both are given.
            var excludeOutputRegex = parseResult.GetValueForOption(AffectedGlobalOptions.ExcludeOutputRegexOption);

            if (string.IsNullOrEmpty(excludeOutputRegex))
            {
                excludeOutputRegex = parseResult.GetValueForOption(AffectedGlobalOptions.ExclusionRegexOption);
            }

            return new AffectedOptions(
                parseResult.GetValueForOption(AffectedGlobalOptions.RepositoryPathOptions),
                filterFilePath,
                parseResult.GetValueForOption(AffectedGlobalOptions.FromOption),
                parseResult.GetValueForOption(AffectedGlobalOptions.ToOption),
                excludeOutputRegex,
                parseResult.GetValueForOption(AffectedGlobalOptions.AssumeChangesOption),
                parseResult.GetValueForOption(AffectedGlobalOptions.ExcludeDiscoveryRegexOption)
            );
        }
    }
}
