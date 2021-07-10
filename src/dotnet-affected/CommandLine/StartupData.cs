using Microsoft.Extensions.DependencyInjection;

namespace Affected.Cli
{
    internal class StartupData
    {
        public StartupData(IServiceCollection serviceCollection)
        {
            ServiceCollection = serviceCollection;
        }

        public IServiceCollection ServiceCollection { get; }
    }
}
