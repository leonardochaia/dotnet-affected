using Microsoft.Extensions.DependencyInjection;
using System;
using System.CommandLine;
using System.CommandLine.IO;
using System.CommandLine.Rendering;

namespace Affected.Cli.Tests
{
    public abstract class BaseRepositoryTest
        : BaseMSBuildTest, IDisposable
    {
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
                    this.ConfigureServices(services);
                });
        }

        protected virtual void ConfigureServices(IServiceCollection services)
        {
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool dispose)
        {
            if(!dispose) return;

            Repository?.Dispose();
        }
    }
}
