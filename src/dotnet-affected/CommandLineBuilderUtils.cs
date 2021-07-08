using Affected.Cli.Commands;
using Affected.Cli.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;

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
                        services.AddFromModelBinder<ViewRenderingContext>();
                        configureServices?.Invoke(services);
                    });

                    // All commands and their corresponding handlers need to be added here
                    // for constructor DI to work.
                    host.UseCommandHandler<ChangesCommand, ChangesCommand.CommandHandler>()
                        .UseCommandHandler<GenerateCommand, GenerateCommand.CommandHandler>();
                })
                .UseDefaults();
        }
    }
}
