using Microsoft.Extensions.DependencyInjection;
using System;
using System.CommandLine.Binding;

namespace Affected.Cli
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFromModelBinder<T>(this IServiceCollection services) where T : class
        {
            services.AddTransient(sp =>
                (T)(new ModelBinder(typeof(T)).CreateInstance(
                        sp.GetRequiredService<BindingContext>()) ??
                    throw new InvalidOperationException($"Failed to instantiate {typeof(T).Name}")));
            return services;
        }
    }
}
