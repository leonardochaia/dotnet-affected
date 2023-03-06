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
            return new AffectedOptions(
                parseResult.GetValueForOption(AffectedGlobalOptions.RepositoryPathOptions),
                parseResult.GetValueForOption(AffectedGlobalOptions.SolutionPathOption),
                parseResult.GetValueForOption(AffectedGlobalOptions.FromOption),
                parseResult.GetValueForOption(AffectedGlobalOptions.ToOption)
            );
        }
    }
}
