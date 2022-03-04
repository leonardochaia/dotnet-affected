using System.CommandLine.Rendering.Views;
using System.Linq;

namespace Affected.Cli.Views
{
    internal class AffectedInfoView : StackLayoutView
    {
        public AffectedInfoView(ICommandExecutionContext context)
        {
            Add(new ContentView($"{context.ChangedFiles.Count()} files have changed " +
                                $"inside {context.ChangedProjects.Count()} projects"));
            Add(new ContentView($"{context.ChangedNuGetPackages.Count()} NuGet Packages have changed"));
            Add(new ContentView($"{context.AffectedProjects.Count()} projects are affected by these changes"));
            
            Add(new WithChangesAndAffectedView(context));
        }
    }
}
