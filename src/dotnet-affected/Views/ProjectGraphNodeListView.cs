using Microsoft.Build.Graph;
using System.Collections.Generic;
using System.CommandLine.Rendering.Views;

namespace Affected.Cli.Views
{
    internal class ProjectGraphNodeListView : StackLayoutView
    {
        public ProjectGraphNodeListView(IEnumerable<ProjectGraphNode> nodes)
        {
            foreach (var name in nodes)
            {
                Add(new ContentView($"\t{name.GetProjectName()}"));
            }
        }
    }
}
