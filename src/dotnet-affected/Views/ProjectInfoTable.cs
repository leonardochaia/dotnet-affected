using Microsoft.Build.Graph;
using System.Collections.Generic;
using System.CommandLine.Rendering.Views;
using System.Linq;

namespace Affected.Cli.Views
{
    internal class ProjectInfoTable : TableView<IProjectInfo>
    {
        public ProjectInfoTable(IEnumerable<ProjectGraphNode> nodes)
        {
            this.Items = nodes.Select(p => new ProjectInfo(p))
                .OrderBy(x => x.Name)
                .ToList();
            this.AddColumn(p => p.Name, "Name");
            this.AddColumn(p => p.FilePath, "Path");
        }
    }
}
