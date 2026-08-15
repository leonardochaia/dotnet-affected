using System.Collections.Generic;
using System.CommandLine.Rendering.Views;
using System.Linq;

namespace Affected.Cli.Views
{
    /// <summary>
    /// Lists projects that are known only by path.
    ///
    /// Path is the only column because nothing evaluated these projects, so there is no
    /// ProjectName to show. That is what separates them from <see cref="ProjectInfoTable"/>.
    /// </summary>
    internal sealed class ProjectPathTable : TableView<string>
    {
        public ProjectPathTable(IEnumerable<string> paths)
        {
            this.Items = paths.OrderBy(path => path)
                .ToList();
            this.AddColumn(path => path, "Path");
        }
    }
}
