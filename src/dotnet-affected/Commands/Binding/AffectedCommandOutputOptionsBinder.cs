using DotnetAffected.Core;
using System.CommandLine.Binding;

namespace Affected.Cli.Commands
{
    internal class AffectedCommandOutputOptionsBinder : BinderBase<AffectedCommandOutputOptions>
    {
        private readonly AffectedOptions _options;

        public AffectedCommandOutputOptionsBinder(AffectedOptions options)
        {
            _options = options;
        }

        protected override AffectedCommandOutputOptions GetBoundValue(BindingContext bindingContext)
        {
            var result = bindingContext.ParseResult;
            return new AffectedCommandOutputOptions(
                _options.RepositoryPath,
                result.GetValueForOption(AffectedRootCommand.OutputDirOption)!,
                result.GetValueForOption(AffectedRootCommand.OutputNameOption)!,
                result.GetValueForOption(AffectedRootCommand.FormatOption)!,
                result.GetValueForOption(AffectedRootCommand.DryRunOption)
            );
        }
    }
}
