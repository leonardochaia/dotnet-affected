using System.Collections.Generic;
using System.CommandLine.Rendering.Views;
using System.Linq;

namespace Affected.Cli.Views
{
    internal class ProjectInfoTable : TableView<IProjectInfo>
    {
        public ProjectInfoTable(IEnumerable<IProjectInfo> projectInfos)
        {
            this.Items = projectInfos.ToList();
            this.AddColumn(p => p.Name, "Name");
            this.AddColumn(p => p.FilePath, "Path");
        }
    }
}
