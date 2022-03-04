using System.Collections.Generic;
using System.CommandLine.Rendering.Views;

namespace Affected.Cli.Views
{
    internal class NuGetPackageStackLayoutView : StackLayoutView
    {
        public NuGetPackageStackLayoutView(IEnumerable<string> nugetPackages)
        {
            Add(new ContentView("Changed NuGet Packages"));

            foreach (var package in nugetPackages)
            {
                this.Add(new ContentView(package));
            }
        }
    }
}
