using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;

namespace Affected.Cli.Commands
{
    internal class ChangesCommand : Command
    {
        public ChangesCommand()
            : base("changes")
        {
            this.Description = "Finds projects that have any changes in any of its files using Git";

            this.Handler = CommandHandler.Create<CommandExecutionData, IConsole>(this.ChangesHandler);
        }

        private void ChangesHandler(
            CommandExecutionData data,
            IConsole console)
        {
            using var context = data.BuildExecutionContext();

            console.Out.WriteLine("Files inside these projects have changed:");
            foreach (var node in context.NodesWithChanges)
            {
                console.Out.WriteLine($"\t{node.GetProjectName()}");
            }
        }
    }
}
