using Microsoft.Build.Graph;
using System.Collections.Generic;
using System.CommandLine.Rendering.Views;

namespace Affected.Cli.Views
{
    internal class WithChangesAndAffectedView : StackLayoutView
    {
        public WithChangesAndAffectedView(
            IEnumerable<ProjectGraphNode> nodesWithChanges,
            IEnumerable<ProjectGraphNode> affectedNodes
        )
        {
            Add(new NodesWithChangesView(nodesWithChanges));

            Add(new ContentView(string.Empty));

            Add(new AffectedProjectsView(affectedNodes));

            Add(new ContentView(string.Empty));
        }
    }
}
