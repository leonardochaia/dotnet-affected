using System.Collections.Generic;
using System.CommandLine.Rendering.Views;

namespace Affected.Cli.Views
{
    internal class ProjectInfoListView : StackLayoutView
    {
        public ProjectInfoListView(IEnumerable<IProjectInfo> projects)
        {
            foreach (var project in projects)
            {
                Add(new ContentView($"\t{project.Name}"));
            }
        }
    }
}
