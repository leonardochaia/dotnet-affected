using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;

namespace Affected.Cli
{
    /// <summary>
    /// This custom builder is a helper class that allows us to keep state while we build the CommandLineBuilder.
    /// We need this so that we can call Configure* methods multiple times.
    /// </summary>
    public class AffectedCommandLineBuilder
    {
        private readonly RootCommand _rootCommand;

        private readonly ICollection<Action<IServiceCollection>> _configureServices =
            new List<Action<IServiceCollection>>();

        private readonly ICollection<Action<CommandLineBuilder>> _configureCommandLine =
            new List<Action<CommandLineBuilder>>();

        /// <summary>
        /// Initializes a new instance of <see cref="AffectedCommandLineBuilder"/>.
        /// </summary>
        /// <param name="rootCommand"></param>
        public AffectedCommandLineBuilder(RootCommand rootCommand)
        {
            _rootCommand = rootCommand;
        }

        /// <summary>
        /// Adds a callback for configuring the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public AffectedCommandLineBuilder ConfigureServices(Action<IServiceCollection> callback)
        {
            this._configureServices.Add(callback);
            return this;
        }

        /// <summary>
        /// Adds a callback for configuring the <see cref="CommandLineBuilder"/>.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public AffectedCommandLineBuilder ConfigureCommandLine(Action<CommandLineBuilder> callback)
        {
            this._configureCommandLine.Add(callback);
            return this;
        }

        /// <summary>
        /// Composes the <see cref="IServiceCollection"/> with <see cref="ConfigureServices"/> and
        /// builds a new <see cref="CommandLineBuilder"/> which is configured by calling all
        /// <see cref="ConfigureCommandLine"/>.
        /// </summary>
        /// <returns>A new <see cref="Parser"/> instance.</returns>
        public Parser Build()
        {
            var services = ComposeServiceCollection();

            var builder = new CommandLineBuilder(this._rootCommand)
                .UseAffectedCli(new StartupData(services));

            foreach (var callback in _configureCommandLine)
            {
                callback.Invoke(builder);
            }

            return builder.Build();
        }

        /// <summary>
        /// Creates a new <see cref="IServiceCollection"/> and applies all <see cref="ConfigureServices"/>
        /// </summary>
        /// <returns>A newly configured <see cref="IServiceCollection"/>.</returns>
        public ServiceCollection ComposeServiceCollection()
        {
            var services = new ServiceCollection();
            foreach (var callback in _configureServices)
            {
                callback.Invoke(services);
            }

            return services;
        }
    }
}
