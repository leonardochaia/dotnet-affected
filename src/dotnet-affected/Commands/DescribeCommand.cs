using Affected.Cli.Views;
using System.CommandLine;
using System.CommandLine.Rendering;

namespace Affected.Cli.Commands
{
    internal class DescribeCommand : Command
    {
        public DescribeCommand()
            : base("describe", "Prints the current changed and affected projects.")
        {
            this.SetHandler(ctx =>
            {
                var console = ctx.Console;
                var data = ctx.GetCommandExecutionData(AffectedRootCommand.DataBinder);
                var executor = data.BuildAffectedExecutor();

                var summary = executor.Execute();
                summary.ThrowIfNoChanges();

                var view = new AffectedInfoView(summary);
                console.Append(view);
            });
        }
    }
}
