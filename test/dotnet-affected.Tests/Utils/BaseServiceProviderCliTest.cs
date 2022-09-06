using Affected.Cli.Commands;
using DotnetAffected.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;

namespace Affected.Cli.Tests
{
    public abstract class BaseServiceProviderCliTest
        : BaseCliTest
    {
        private readonly Lazy<ServiceProvider> _serviceProviderLazy;

        protected ServiceProvider ServiceProvider => _serviceProviderLazy.Value;

        private readonly Lazy<AffectedSummary> _affectedSummaryLazy;

        protected AffectedSummary AffectedSummary => _affectedSummaryLazy.Value;

        protected BaseServiceProviderCliTest()
        {
            this._serviceProviderLazy = new Lazy<ServiceProvider>(() =>
            {
                var services = this.BuildAffectedCli()
                    .ComposeServiceCollection();

                return services.BuildServiceProvider(true);
            });

            this._affectedSummaryLazy = new Lazy<AffectedSummary>(() =>
            {
                var executor = this.ServiceProvider.GetRequiredService<IAffectedExecutor>();
                return executor.Execute();
            });
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
