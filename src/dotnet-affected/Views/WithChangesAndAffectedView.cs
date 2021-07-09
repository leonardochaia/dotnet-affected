using Microsoft.Build.Graph;
using System.Collections.Generic;
using System.CommandLine.Rendering.Views;
using System.Linq;

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

            if (affectedNodes.Any())
            {
                Add(new AffectedProjectsView(affectedNodes));
            }
            else
            {
                Add(new NoChangesView());
            }
        }
    }
}
