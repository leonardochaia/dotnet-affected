using BenchmarkDotNet.Running;
using Microsoft.Build.Locator;

namespace Affected.Cli.Benchmarks
{
    public class Program
    {
        static Program()
        {
            MSBuildLocator.RegisterDefaults();
        }

        static void Main(string[] args) => BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly)
            .Run(args);
    }
}
