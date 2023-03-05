using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Binding;

namespace Affected.Cli.Commands
{
    internal class CommandExecutionDataBinder : BinderBase<CommandExecutionData>
    {
        private readonly Option<string> _repositoryPathOption;
        private readonly Option<string> _solutionPathOption;
        private readonly Option<string> _fromOption;
        private readonly Option<string> _toOption;
        private readonly Option<bool> _verboseOption;
        private readonly Option<IEnumerable<string>> _assumeChangesOption;
        private readonly Option<string[]> _formatOption;
        private readonly Option<bool> _dryRunOption;
        private readonly Option<string> _outputDirOption;
        private readonly Option<string> _outputNameOption;

        public CommandExecutionDataBinder(
            Option<string> repositoryPathOption,
            Option<string> solutionPathOption,
            Option<string> fromOption,
            Option<string> toOption,
            Option<bool> verboseOption,
            Option<IEnumerable<string>> assumeChangesOption,
            Option<string[]> formatOption,
            Option<bool> dryRunOption,
            Option<string> outputDirOption,
            Option<string> outputNameOption)
        {
            _repositoryPathOption = repositoryPathOption;
            _solutionPathOption = solutionPathOption;
            _fromOption = fromOption;
            _toOption = toOption;
            _verboseOption = verboseOption;
            _assumeChangesOption = assumeChangesOption;
            _formatOption = formatOption;
            _dryRunOption = dryRunOption;
            _outputDirOption = outputDirOption;
            _outputNameOption = outputNameOption;
        }

        protected override CommandExecutionData GetBoundValue(BindingContext bindingContext)
        {
            var result = bindingContext.ParseResult;
            return new CommandExecutionData(
                result.GetValueForOption(_repositoryPathOption)!,
                result.GetValueForOption(_solutionPathOption)!,
                result.GetValueForOption(_fromOption)!,
                result.GetValueForOption(_toOption)!,
                result.GetValueForOption(_verboseOption)!,
                result.GetValueForOption(_assumeChangesOption)!,
                result.GetValueForOption(_formatOption)!,
                result.GetValueForOption(_dryRunOption)!,
                result.GetValueForOption(_outputDirOption)!,
                result.GetValueForOption(_outputNameOption)!
            );
        }
    }
}
