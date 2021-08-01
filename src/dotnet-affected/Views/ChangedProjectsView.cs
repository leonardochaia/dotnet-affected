using System.Collections.Generic;
using System.CommandLine.Rendering.Views;

namespace Affected.Cli.Views
{
    internal class ChangedProjectsView : StackLayoutView
    {
        public ChangedProjectsView(IEnumerable<IProjectInfo> projects)
        {
            Add(new ContentView("Files inside these projects have changed:"));

            Add(new ProjectInfoListView(projects));
        }
    }
}
