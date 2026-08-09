using BenchmarkDotNet.Running;
using Microsoft.Build.Locator;
using System.Linq;
using System.Threading.Tasks;

namespace Affected.Cli.Benchmarks
{
    public class Program
    {
        static Program()
        {
            MSBuildLocator.RegisterDefaults();
        }

        static async Task Main(string[] args)
        {
            // `profile` runs the phase profiler instead of BenchmarkDotNet. It is a single
            // timed run per size, which is what you want when locating a hotspot rather than
            // measuring a stable number.
            if (args.Length > 0 && args[0] == "predictors")
            {
                var size = args.Length > 1 && int.TryParse(args[1], out var parsed) ? parsed : 1000;
                await PhaseProfiler.RunPredictorBreakdownAsync(size, childrenPerProject: 20);
                return;
            }

            if (args.Length > 0 && args[0] == "profile")
            {
                var sizes = args.Skip(1)
                    .Select(arg => int.TryParse(arg, out var value) ? value : 0)
                    .Where(value => value > 0)
                    .ToArray();

                await PhaseProfiler.RunAsync(
                    sizes.Length > 0 ? sizes : new[] { 250, 500, 1000 },
                    childrenPerProject: 20);

                return;
            }

            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        }
    }
}
