using System.Collections.Generic;
using System.CommandLine.Rendering.Views;
using System.Linq;

namespace Affected.Cli.Views
{
    internal class WithChangesAndAffectedView : StackLayoutView
    {
        public WithChangesAndAffectedView(
            IEnumerable<IProjectInfo> changedProjects,
            IEnumerable<IProjectInfo> affectedProjects
        )
        {
            if (!changedProjects.Any())
            {
                Add(new NoChangesView());
                return;
            }

            Add(new ContentView("Changed Projects"));
            Add(new ProjectInfoTable(changedProjects));

            Add(new ContentView("\nAffected Projects"));
            
            if (!affectedProjects.Any())
            {
                Add(new ContentView("No projects where affected by any of the changed projects."));
                return;
            }

            Add(new ProjectInfoTable(affectedProjects));
        }
    }
}
