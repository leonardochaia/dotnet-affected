using Affected.Cli.Commands;
using Affected.Cli.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.CommandLine.Builder;

namespace Affected.Cli
{
    internal static class AffectedCli
    {
        /// <summary>
        /// Creates the CLI with everything required to run dotnet-affected.
        /// </summary>
        /// <returns>For extensibility purposes.</returns>
        public static AffectedCommandLineBuilder CreateAffectedCommandLineBuilder()
        {
            return new AffectedCommandLineBuilder(new AffectedRootCommand())
                .ConfigureServices(services =>
                {
                    services.AddTransient<ICommandExecutionContext, CommandExecutionContext>();
                    services.AddTransient<IChangesProvider, GitChangesProvider>();
                    services.AddFromBindingContext<CommandExecutionData>();
                })
                .ConfigureCommandLine(builder =>
                {
                    builder.UseDefaults()
                        .UseCommandHandler<AffectedRootCommand, AffectedRootCommand.AffectedCommandHandler>()
                        .UseCommandHandler<ChangesCommand, ChangesCommand.CommandHandler>()
                        .UseCommandHandler<GenerateCommand, GenerateCommand.CommandHandler>();
                    builder.UseRenderingErrorHandler(new Dictionary<Type, RenderingErrorConfig>()
                    {
                        [typeof(NoChangesException)] = new(AffectedExitCodes.NothingChanged, new NoChangesView()),
                    });
                });
        }
    }
}
