using Affected.Cli.Views;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Rendering;
using System.Linq;
using System.Threading.Tasks;

namespace Affected.Cli.Commands
{
    internal class ChangesCommand : Command
    {
        public ChangesCommand()
            : base("changes")
        {
            this.Description = "Finds projects that have any changes in any of its files using Git";
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
                if (!_context.ChangedProjects.Any())
                {
                    throw new NoChangesException();
                }

                var view = new NodesWithChangesView(_context.ChangedProjects);
                _console.Append(view);

                return Task.FromResult(0);
            }
        }
    }
}
