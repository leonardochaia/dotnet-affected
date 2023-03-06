using DotnetAffected.Abstractions;
using System.CommandLine.Rendering.Views;
using System.Linq;

namespace Affected.Cli.Views
{
    internal sealed class WithChangesAndAffectedView : StackLayoutView
    {
        public WithChangesAndAffectedView(AffectedSummary summary)
        {
            if (!summary.ProjectsWithChangedFiles.Any() && !summary.AffectedProjects.Any())
            {
                Add(new NoChangesView());
                return;
            }

            if (summary.ProjectsWithChangedFiles.Any())
            {
                Add(new ContentView("Changed Projects"));
                Add(new ProjectInfoTable(summary.ProjectsWithChangedFiles));
            }

            if (summary.ChangedPackages.Any())
            {
                Add(new ContentView(""));
                Add(new ContentView("Changed NuGet Packages"));
                Add(new NugetPackagesTable(summary.ChangedPackages));
            }

            Add(new ContentView("\nAffected Projects"));

            if (!summary.AffectedProjects.Any())
            {
                Add(new ContentView("No projects where affected by any of the changed projects."));
                return;
            }

            Add(new ProjectInfoTable(summary.AffectedProjects));
        }
    }
}
