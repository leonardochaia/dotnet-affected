using Affected.Cli.Views;
using DotnetAffected.Abstractions;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Rendering;
using System.Threading.Tasks;

namespace Affected.Cli.Commands
{
    internal class DescribeCommand : Command
    {
        public DescribeCommand()
            : base("describe")
        {
            this.Description = "Prints the current changed and affected projects.";
        }

        public class CommandHandler : ICommandHandler
        {
            private readonly IAffectedExecutor _executor;
            private readonly CommandExecutionData _data;
            private readonly IConsole _console;

            public CommandHandler(
                IAffectedExecutor executor,
                CommandExecutionData data,
                IConsole console)
            {
                _executor = executor;
                _data = data;
                _console = console;
            }

            public Task<int> InvokeAsync(InvocationContext ic)
            {
                if (!_data.MsBuildPassthrough)
                {
                    var summary = _executor.Execute();
                    summary.ThrowIfNoChanges();

                    var view = new AffectedInfoView(summary);
                    _console.Append(view);
                }

                return Task.FromResult(0);
            }
        }
    }
}
