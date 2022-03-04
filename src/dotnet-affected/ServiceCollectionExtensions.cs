using Microsoft.Extensions.DependencyInjection;
using System;
using System.CommandLine.Binding;
using System.CommandLine.NamingConventionBinder;

namespace Affected.Cli
{
    internal static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers <typeparamref name="T"/> as Transient, to be resolved using CommandLine's Invocation API.
        /// This means the type can "inject" arguments and Options and that kind of stuff.
        /// </summary>
        /// <param name="services">The Service collection.</param>
        /// <typeparam name="T">Type to register.</typeparam>
        /// <returns>For chaining.</returns>
        /// <exception cref="InvalidOperationException">When T cannot be activated.</exception>
        public static IServiceCollection AddFromBindingContext<T>(this IServiceCollection services) where T : class
        {
            services.AddTransient(sp =>
                (T)(new ModelBinder(typeof(T)).CreateInstance(
                        sp.GetRequiredService<BindingContext>()) ??
                    throw new InvalidOperationException($"Failed to instantiate {typeof(T).Name}")));
            return services;
        }
    }
}
