using DotnetAffected.Abstractions;
using System.CommandLine.Rendering.Views;
using System.Linq;

namespace Affected.Cli.Views
{
    internal sealed class AffectedInfoView : StackLayoutView
    {
        public AffectedInfoView(AffectedSummary summary)
        {
            Add(new ContentView($"{summary.FilesThatChanged.Length} files have changed " +
                                $"referenced by {summary.ProjectsWithChangedFiles.Length} projects"));
            Add(new ContentView($"{summary.ChangedPackages.Length} NuGet Packages have changed"));
            Add(new ContentView($"{summary.AffectedProjects.Length} projects are affected by these changes"));
            Add(new ContentView($"{summary.ExcludedProjects.Length} projects were excluded"));

            Add(new WithChangesAndAffectedView(summary));
        }
    }
}
