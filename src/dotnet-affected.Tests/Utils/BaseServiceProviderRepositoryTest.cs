using Affected.Cli.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Affected.Cli.Tests
{
    public abstract class BaseServiceProviderRepositoryTest
        : BaseRepositoryTest, IAsyncLifetime
    {
        protected ServiceProvider ServiceProvider { get; set; }

        protected ICommandExecutionContext Context =>
            this.ServiceProvider.GetRequiredService<ICommandExecutionContext>();

        public Task InitializeAsync()
        {
            var services = this.BuildAffectedCli()
                .ComposeServiceCollection();

            this.ServiceProvider = services.BuildServiceProvider(true);

            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        protected override void Dispose(bool dispose)
        {
            if (dispose)
            {
                this.ServiceProvider.Dispose();
            }
            
            base.Dispose(dispose);
        }

        protected override void ConfigureServices(IServiceCollection services)
        {
            services.Replace(ServiceDescriptor.Singleton(new CommandExecutionData(
                this.Repository.Path,
                string.Empty,
                String.Empty,
                String.Empty,
                true,
                Enumerable.Empty<string>(),
                Array.Empty<string>(),
                true,
                string.Empty,
                string.Empty)));

        }
    }
}
