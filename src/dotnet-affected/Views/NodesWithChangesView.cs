using Microsoft.Build.Graph;
using System.Collections.Generic;
using System.CommandLine.Rendering.Views;

namespace Affected.Cli.Views
{
    internal class NodesWithChangesView : StackLayoutView
    {
        public NodesWithChangesView(IEnumerable<ProjectGraphNode> nodes)
        {
            Add(new ContentView("Files inside these projects have changed:"));

            Add(new ProjectGraphNodeListView(nodes));
        }
    }
}
