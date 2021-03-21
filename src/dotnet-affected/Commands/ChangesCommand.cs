using Affected.Cli.Views;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Rendering.Views;

namespace Affected.Cli.Commands
{
    internal class ChangesCommand : Command
    {
        public ChangesCommand()
            : base("changes")
        {
            this.Description = "Finds projects that have any changes in any of its files using Git";

            this.Handler = CommandHandler.Create<CommandExecutionData, ViewRenderingContext>(this.ChangesHandler);
        }

        private void ChangesHandler(
            CommandExecutionData data,
            ViewRenderingContext renderingContext)
        {
            using var context = data.BuildExecutionContext();

            var rootView = new NodesWithChangesView(context.NodesWithChanges);
            rootView.Add(new ContentView(string.Empty));
            renderingContext.Render(rootView);
        }
    }
}
