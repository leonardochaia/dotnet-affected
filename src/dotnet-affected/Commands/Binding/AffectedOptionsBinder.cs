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
            
            return new AffectedOptions(
                parseResult.GetValueForOption(AffectedGlobalOptions.RepositoryPathOptions),
                filterFilePath,
                parseResult.GetValueForOption(AffectedGlobalOptions.FromOption),
                parseResult.GetValueForOption(AffectedGlobalOptions.ToOption),
                parseResult.GetValueForOption(AffectedGlobalOptions.ExclusionRegexOption)
            );
        }
    }
}
