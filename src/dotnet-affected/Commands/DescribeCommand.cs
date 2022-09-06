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
            private readonly IConsole _console;

            public CommandHandler(
                IAffectedExecutor executor,
                IConsole console)
            {
                _executor = executor;
                _console = console;
            }

            public Task<int> InvokeAsync(InvocationContext ic)
            {
                var summary = _executor.Execute();
                summary.ThrowIfNoChanges();

                var view = new AffectedInfoView(summary);
                _console.Append(view);

                return Task.FromResult(0);
            }
        }
    }
}
