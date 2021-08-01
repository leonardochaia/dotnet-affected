using System.Collections.Generic;
using System.CommandLine.Rendering.Views;

namespace Affected.Cli.Views
{
    internal class AffectedProjectsView : StackLayoutView
    {
        public AffectedProjectsView(IEnumerable<IProjectInfo> projects)
        {
            Add(new ContentView("These projects are affected by those changes:"));

            Add(new ProjectInfoListView(projects));
        }
    }
}
