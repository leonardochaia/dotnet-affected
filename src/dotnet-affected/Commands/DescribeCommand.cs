using System.CommandLine;
using System.CommandLine.Invocation;
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
            private readonly ICommandExecutionContext _context;
            private readonly IConsole _console;
            private readonly CommandExecutionData _data;

            public CommandHandler(ICommandExecutionContext context, IConsole console, CommandExecutionData data)
            {
                _context = context;
                _console = console;
                _data = data;
            }

            public Task<int> InvokeAsync(InvocationContext ic)
            {
                _console.WriteChangedAndAffectedProjects(_context, _data.Verbose);

                return Task.FromResult(0);
            }
        }
    }
}
