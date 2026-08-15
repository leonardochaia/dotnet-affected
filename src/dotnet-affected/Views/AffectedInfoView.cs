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
            Add(new ContentView($"{summary.ExcludedProjects.Count()} projects were excluded"));

            // Only when it happened. The two exclusions differ in kind, not degree, so a
            // permanent "0 projects were excluded from discovery" would be noise on every run
            // that never asked for it.
            if (summary.ProjectsExcludedFromDiscovery.Any())
            {
                Add(new ContentView(
                    $"{summary.ProjectsExcludedFromDiscovery.Length} projects were excluded from discovery"));
            }

            Add(new WithChangesAndAffectedView(summary));
        }
    }
}
