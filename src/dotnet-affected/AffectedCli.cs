using Affected.Cli.Commands;
using Affected.Cli.Formatters;
using Affected.Cli.Views;
using DotnetAffected.Abstractions;
using DotnetAffected.Core;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.CommandLine.Builder;
using System.Linq;

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
                    services.AddFromBindingContext<CommandExecutionData>();

                    services.AddTransient<IOutputFormatter, TextOutputFormatter>();
                    services.AddTransient<IOutputFormatter, TraversalProjectOutputFormatter>();
                    services.AddTransient<IOutputFormatterExecutor, OutputFormatterExecutor>();

                    services.AddSingleton<IAffectedExecutor>(sp =>
                    {
                        var data = sp.GetRequiredService<CommandExecutionData>();
                        var options = data.ToAffectedOptions();
                        var graph = new ProjectGraphFactory(options).BuildProjectGraph();
                        IChangesProvider changesProvider = data.AssumeChanges?.Any() == true
                            ? new AssumptionChangesProvider(graph, data.AssumeChanges)
                            : new GitChangesProvider();
                        return new AffectedExecutor(options,
                            graph,
                            changesProvider,
                            new PredictionChangedProjectsProvider(graph, options));
                    });
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
