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
    internal class AffectedCommandLineBuilder
    {
        private readonly RootCommand _rootCommand;

        private readonly ICollection<Action<IServiceCollection>> _configureServices =
            new List<Action<IServiceCollection>>();

        private readonly ICollection<Action<CommandLineBuilder>> _configureCommandLine =
            new List<Action<CommandLineBuilder>>();

        public AffectedCommandLineBuilder(RootCommand rootCommand)
        {
            _rootCommand = rootCommand;
        }

        public AffectedCommandLineBuilder ConfigureServices(Action<IServiceCollection> callback)
        {
            this._configureServices.Add(callback);
            return this;
        }

        public AffectedCommandLineBuilder ConfigureCommandLine(Action<CommandLineBuilder> callback)
        {
            this._configureCommandLine.Add(callback);
            return this;
        }

        public Parser Build()
        {
            var services = new ServiceCollection();
            foreach (var callback in _configureServices)
            {
                callback.Invoke(services);
            }

            var builder = new CommandLineBuilder(this._rootCommand)
                .UseAffectedCli(new StartupData(services));

            foreach (var callback in _configureCommandLine)
            {
                callback.Invoke(builder);
            }

            return builder.Build();
        }
    }
}
