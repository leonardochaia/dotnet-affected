using Affected.Cli.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;

namespace Affected.Cli.Tests
{
    public abstract class BaseServiceProviderRepositoryTest
        : BaseRepositoryTest
    {
        private readonly Lazy<ServiceProvider> _serviceProviderLazy;

        protected ServiceProvider ServiceProvider => _serviceProviderLazy.Value;

        protected ICommandExecutionContext Context =>
            this.ServiceProvider.GetRequiredService<ICommandExecutionContext>();

        protected BaseServiceProviderRepositoryTest()
        {
            this._serviceProviderLazy = new Lazy<ServiceProvider>(() =>
            {
                var services = this.BuildAffectedCli()
                    .ComposeServiceCollection();

                return services.BuildServiceProvider(true);
            });
        }

        protected override void Dispose(bool dispose)
        {
            if (dispose)
            {
                this.ServiceProvider?.Dispose();
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
