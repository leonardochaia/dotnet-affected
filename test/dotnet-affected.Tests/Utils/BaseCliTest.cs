using DotnetAffected.Testing.Utils;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.CommandLine.IO;
using System.CommandLine.Rendering;

namespace Affected.Cli.Tests
{
    public abstract class BaseCliTest
        : BaseRepositoryTest
    {
        protected ITerminal Terminal { get; } = new TestTerminal()
        {
            OutputMode = OutputMode.PlainText
        };

        protected virtual AffectedCommandLineBuilder BuildAffectedCli()
        {
            return AffectedCli
                .CreateAffectedCommandLineBuilder()
                .ConfigureServices(services =>
                {
                    services.AddSingleton<ITerminal>(this.Terminal);
                    services.AddSingleton<IConsole>(new SystemConsole());
                    this.ConfigureServices(services);
                });
        }

        protected virtual void ConfigureServices(IServiceCollection services)
        {
        }
    }
}
