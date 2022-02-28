using Affected.Cli.Commands;
using Affected.Cli.Formatters;
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
                    services.AddSingleton<ICommandExecutionContext, CommandExecutionContext>();
                    services.AddTransient<IChangesProvider, GitChangesProvider>();
                    services.AddFromBindingContext<CommandExecutionData>();

                    services.AddTransient<IOutputFormatter, TextOutputFormatter>();
                    services.AddTransient<IOutputFormatter, TraversalProjectOutputFormatter>();
                    services.AddTransient<IOutputFormatterExecutor, OutputFormatterExecutor>();
                })
                .ConfigureCommandLine(builder =>
                {
                    builder.UseDefaults()
                        .UseCommandHandler<AffectedRootCommand, AffectedRootCommand.AffectedCommandHandler>()
                        .UseCommandHandler<DescribeCommand, DescribeCommand.CommandHandler>();
                    builder.UseRenderingErrorHandler(new Dictionary<Type, RenderingErrorConfig>()
                    {
                        [typeof(NoChangesException)] = new(AffectedExitCodes.NothingChanged, new NoChangesView()),
                    });
                });
        }
    }
}
