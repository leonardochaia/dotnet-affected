using System.CommandLine.Rendering.Views;

namespace Affected.Cli.Views
{
    internal class NoChangesView : ContentView
    {
        public NoChangesView()
            : base("No affected projects were found for the current changes")
        {
        }
    }
}
