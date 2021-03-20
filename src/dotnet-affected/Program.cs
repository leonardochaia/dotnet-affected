using Affected.Cli.Commands;
using Microsoft.Build.Locator;
using System.CommandLine;
using System.Threading.Tasks;

namespace Affected.Cli
{
    internal static class Program
    {
        static Program()
        {
            // This is required for MSBuild stuff to function properly.
            MSBuildLocator.RegisterDefaults();
        }

        public static Task<int> Main(string[] args)
        {
            var command = new AffectedRootCommand();
            return command.InvokeAsync(args);
        }
    }
}
