using DotnetAffected.Abstractions;
using System.CommandLine.Rendering.Views;
using System.Linq;

namespace Affected.Cli.Views
{
    internal sealed class AffectedInfoView : StackLayoutView
    {
        public AffectedInfoView(AffectedSummary summary)
        {
            Add(new ContentView($"{summary.FilesThatChanged.Count()} files have changed " +
                                $"referenced by {summary.ProjectsWithChangedFiles.Count()} projects"));
            Add(new ContentView($"{summary.ChangedPackages.Count()} NuGet Packages have changed"));
            Add(new ContentView($"{summary.AffectedProjects.Count()} projects are affected by these changes"));

            Add(new WithChangesAndAffectedView(summary));
        }
    }
}
