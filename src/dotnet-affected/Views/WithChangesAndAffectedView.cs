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
            Add(new ChangedProjectsView(changedProjects));

            if (affectedProjects.Any())
            {
                Add(new AffectedProjectsView(affectedProjects));
            }
            else
            {
                Add(new NoChangesView());
            }
        }
    }
}
