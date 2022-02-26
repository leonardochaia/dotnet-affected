using Affected.Cli.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Affected.Cli.Tests
{
    public abstract class BaseServiceProviderRepositoryTest
        : BaseRepositoryTest
    {
        protected ServiceProvider ServiceProvider { get; set; }

        protected ICommandExecutionContext Context =>
            this.ServiceProvider.GetRequiredService<ICommandExecutionContext>();

        public override Task InitializeAsync()
        {
            var services = this.BuildAffectedCli()
                .ComposeServiceCollection();

            this.ServiceProvider = services.BuildServiceProvider(true);

            return Task.CompletedTask;
        }

        public override async Task DisposeAsync()
        {
            if (this.ServiceProvider is null) return;

            await this.ServiceProvider.DisposeAsync();

            await base.DisposeAsync();
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
