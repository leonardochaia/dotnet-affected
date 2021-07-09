using Affected.Cli.Commands;
using Affected.Cli.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;

namespace Affected.Cli
{
    internal static class CommandLineBuilderUtils
    {
        public static CommandLineBuilder CreateCommandLineBuilder(Action<IServiceCollection>? configureServices = null)
        {
            return new CommandLineBuilder(new AffectedRootCommand())
                .UseHost(host =>
                {
                    host.ConfigureServices((_, services) =>
                    {
                        services.AddTransient<ICommandExecutionContext, CommandExecutionContext>();
                        services.AddFromModelBinder<CommandExecutionData>();
                        configureServices?.Invoke(services);
                    });

                    // All commands and their corresponding handlers need to be added here
                    // for constructor DI to work.
                    host.UseCommandHandler<ChangesCommand, ChangesCommand.CommandHandler>()
                        .UseCommandHandler<GenerateCommand, GenerateCommand.CommandHandler>();
                })
                .UseRenderingErrorHandler(new Dictionary<Type, RenderingErrorConfig>()
                {
                    [typeof(NoChangesException)] = new(AffectedExitCodes.NothingChanged, new NoChangesView()),
                })
                .UseDefaults();
        }
    }
}
