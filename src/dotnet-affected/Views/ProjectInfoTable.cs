using Microsoft.Build.Graph;
using System;
using System.Collections.Generic;
using System.CommandLine.Rendering.Views;
using System.Linq;

namespace Affected.Cli.Views
{
    internal sealed class ProjectInfoTable : TableView<IProjectInfo>
    {
        public ProjectInfoTable(IReadOnlyList<IProjectInfo> nodes)
        {
            this.Items = nodes;
            this.AddColumn(p => p.Name, "Name");
            this.AddColumn(p => p.FilePath, "Path");
            this.AddColumn(p => Enum.GetName(typeof(ProjectStatus), p.Status), "Status");
        }
    }
}
