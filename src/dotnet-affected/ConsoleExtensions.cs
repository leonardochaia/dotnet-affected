using Affected.Cli.Views;
using System.CommandLine;
using System.CommandLine.IO;
using System.CommandLine.Rendering;

namespace Affected.Cli
{
    internal static class ConsoleExtensions
    {
        public static void WriteChangedAndAffectedProjects(
            this IConsole console,
            ICommandExecutionContext context,
            bool verbose)
        {

            var view = new WithChangesAndAffectedView(context.ChangedProjects, context.AffectedProjects);
            if (verbose)
            {
                console.Out.WriteLine();
            }
            console.Append(view);
        }
    }
}
