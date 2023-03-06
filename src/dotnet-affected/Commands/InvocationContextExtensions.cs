using DotnetAffected.Abstractions;
using DotnetAffected.Core;
using System;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.Linq;

namespace Affected.Cli.Commands
{
    internal static class InvocationContextExtensions
    {
        public static CommandExecutionData GetCommandExecutionData(
            this InvocationContext context,
            CommandExecutionDataBinder dataBinder)
        {
            if (((IValueSource)dataBinder).TryGetValue(dataBinder, context.BindingContext, out var dataObj) &&
                dataObj is not null)
            {
                return (dataObj as CommandExecutionData)!;
            }

            throw new InvalidOperationException("Failed to obtain CommandExecutionData from context");
        }

        public static IAffectedExecutor BuildAffectedExecutor(
            this CommandExecutionData data)
        {
            var options = data.ToAffectedOptions();

            var graph = new ProjectGraphFactory(options).BuildProjectGraph();

            IChangesProvider changesProvider = data.AssumeChanges.Any()
                ? new AssumptionChangesProvider(graph, data.AssumeChanges)
                : new GitChangesProvider();

            return new AffectedExecutor(options,
                graph,
                changesProvider,
                new PredictionChangedProjectsProvider(graph, options));
        }
    }
}
