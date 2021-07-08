using Affected.Cli.Commands;
using Affected.Cli.Views;
using Microsoft.Build.Locator;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using System.Threading.Tasks;

namespace Affected.Cli
{
    internal static class Program
    {
        static Program()
        {
            // This is required for MSBuild stuff to function properly.
            MSBuildLocator.RegisterDefaults();
        }

        public static Task<int> Main(string[] args)
        {
            return CreateCommandLineBuilder()
                .Build()
                .InvokeAsync(args);
        }

        public static CommandLineBuilder CreateCommandLineBuilder(Action<IServiceCollection>? configureServices = null)
        {
            return new CommandLineBuilder(new AffectedRootCommand())
                .UseHost(host =>
                {
                    host.ConfigureServices((_, services) =>
                    {
                        services.AddTransient<CommandExecutionContext>();
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
