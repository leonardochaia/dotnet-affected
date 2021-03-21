using Microsoft.Build.Graph;
using System.Collections.Generic;
using System.CommandLine.Rendering.Views;

namespace Affected.Cli.Views
{
    internal class AffectedProjectsView : StackLayoutView
    {
        public AffectedProjectsView(IEnumerable<ProjectGraphNode> nodes)
        {
            Add(new ContentView("These projects are affected by those changes:"));

            Add(new ProjectGraphNodeListView(nodes));
        }
    }
}
