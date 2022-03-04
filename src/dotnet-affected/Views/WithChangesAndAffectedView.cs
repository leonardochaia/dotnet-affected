using System.CommandLine.Rendering.Views;
using System.Linq;

namespace Affected.Cli.Views
{
    internal class WithChangesAndAffectedView : StackLayoutView
    {
        public WithChangesAndAffectedView(ICommandExecutionContext context)
        {
            if (!context.ChangedProjects.Any() && !context.AffectedProjects.Any())
            {
                Add(new NoChangesView());
                return;
            }

            Add(new ContentView("Changed Projects"));
            Add(new ProjectInfoTable(context.ChangedProjects));

            if (context.ChangedNuGetPackages.Any())
            {
                Add(new ContentView(""));
                Add(new NuGetPackageStackLayoutView(context.ChangedNuGetPackages));
            }

            Add(new ContentView("\nAffected Projects"));

            if (!context.AffectedProjects.Any())
            {
                Add(new ContentView("No projects where affected by any of the changed projects."));
                return;
            }

            Add(new ProjectInfoTable(context.AffectedProjects));
        }
    }
}
