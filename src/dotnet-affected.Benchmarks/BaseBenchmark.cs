using Affected.Cli.Tests;
using Microsoft.Build.Locator;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.CommandLine.IO;
using System.CommandLine.Rendering;

namespace Affected.Cli.Benchmarks
{
    public abstract class BaseBenchmark
    {
        static BaseBenchmark()
        {
            MSBuildLocator.RegisterDefaults();
        }

        protected ITerminal Terminal { get; } = new TestTerminal()
        {
            OutputMode = OutputMode.PlainText
        };

        protected TemporaryRepository Repository { get; } = new TemporaryRepository();

        protected virtual AffectedCommandLineBuilder BuildAffectedCli()
        {
            return AffectedCli
                .CreateAffectedCommandLineBuilder()
                .ConfigureServices(services =>
                {
                    services.AddSingleton<ITerminal>(this.Terminal);
                    services.AddSingleton<IConsole>(new SystemConsole());
                });
        }
    }
}
