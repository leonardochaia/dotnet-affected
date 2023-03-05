using Microsoft.Build.Locator;
using System.CommandLine.Parsing;
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
            => AffectedCli.CreateAffectedCommandLineBuilder()
                .Build()
                .InvokeAsync(args);
    }
}
