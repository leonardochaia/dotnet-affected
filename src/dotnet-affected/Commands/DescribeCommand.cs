using Affected.Cli.Views;
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
            private readonly ICommandExecutionContext _context;
            private readonly IConsole _console;

            public CommandHandler(ICommandExecutionContext context, IConsole console)
            {
                _context = context;
                _console = console;
            }

            public Task<int> InvokeAsync(InvocationContext ic)
            {
                var view = new WithChangesAndAffectedView(_context.ChangedProjects, _context.AffectedProjects);
                _console.Append(view);

                return Task.FromResult(0);
            }
        }
    }
}
