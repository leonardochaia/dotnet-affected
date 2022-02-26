using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.CommandLine.IO;
using System.CommandLine.Rendering;
using System.Threading.Tasks;
using Xunit;

namespace Affected.Cli.Tests
{
    public abstract class BaseRepositoryTest
        : BaseMSBuildTest, IAsyncLifetime
    {
        protected ITerminal Terminal { get; } = new TestTerminal()
        {
            OutputMode = OutputMode.PlainText
        };

        protected TemporaryRepository Repository { get; } = new TemporaryRepository();

        public virtual Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        protected virtual AffectedCommandLineBuilder BuildAffectedCli()
        {
            return AffectedCli
                .CreateAffectedCommandLineBuilder()
                .ConfigureServices(services =>
                {
                    services.AddSingleton<ITerminal>(this.Terminal);
                    services.AddSingleton<IConsole>(new TestConsole());
                    this.ConfigureServices(services);
                });
        }

        protected virtual void ConfigureServices(IServiceCollection services)
        {
        }

        public virtual Task DisposeAsync()
        {
            Repository?.Dispose();
            return Task.CompletedTask;
        }
    }
}
