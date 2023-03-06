using DotnetAffected.Abstractions;
using System.Collections.Generic;
using System.CommandLine.Rendering.Views;
using System.Linq;

namespace Affected.Cli.Views
{
    internal sealed class NugetPackagesTable : TableView<PackageChange>
    {
        public NugetPackagesTable(IEnumerable<PackageChange> nugetPackages)
        {
            this.Items = nugetPackages.OrderBy(x => x.Name)
                .ToList();
            this.AddColumn(p => p.Name, "Name");
            this.AddColumn(p => string.Join(",", p.OldVersions), "Old Versions");
            this.AddColumn(p => string.Join(",", p.NewVersions), "New Versions");
        }
    }
}
