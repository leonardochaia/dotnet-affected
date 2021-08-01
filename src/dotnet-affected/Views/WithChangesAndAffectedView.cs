using System.Collections.Generic;
using System.CommandLine.Rendering.Views;

namespace Affected.Cli.Views
{
    internal class WithChangesAndAffectedView : StackLayoutView
    {
        public WithChangesAndAffectedView(
            IEnumerable<IProjectInfo> changedProjects,
            IEnumerable<IProjectInfo> affectedProjects
        )
        {
            Add(new ContentView("Changed Projects"));
            Add(new ProjectInfoTable(changedProjects));

            Add(new ContentView("\nAffected Projects"));
            Add(new ProjectInfoTable(affectedProjects));
        }
    }
}
