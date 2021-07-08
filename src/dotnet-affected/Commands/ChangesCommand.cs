using Affected.Cli.Views;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Rendering.Views;
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
            private readonly CommandExecutionContext _context;
            private readonly ViewRenderingContext _renderingContext;

            public CommandHandler(CommandExecutionContext context, ViewRenderingContext renderingContext)
            {
                _context = context;
                _renderingContext = renderingContext;
            }

            public Task<int> InvokeAsync(InvocationContext ic)
            {
                var rootView = new NodesWithChangesView(_context.NodesWithChanges);
                rootView.Add(new ContentView(string.Empty));
                _renderingContext.Render(rootView);

                return Task.FromResult(0);
            }
        }
    }
}
