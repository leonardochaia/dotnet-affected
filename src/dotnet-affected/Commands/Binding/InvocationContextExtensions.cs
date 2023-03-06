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
        public static (IAffectedExecutor Executor, AffectedOptions Options) BuildAffectedExecutor(
            this InvocationContext ctx)
        {
            var assumeChanges = ctx.ParseResult.GetValueForOption(AffectedGlobalOptions.AssumeChangesOption);

            var options = ctx.GetAffectedOptions();
            var graph = new ProjectGraphFactory(options).BuildProjectGraph();

            var assumptions = assumeChanges?.ToArray() ?? Array.Empty<string>();

            IChangesProvider changesProvider = assumptions.Any()
                ? new AssumptionChangesProvider(graph, assumptions)
                : new GitChangesProvider();

            var executor = new AffectedExecutor(options,
                graph,
                changesProvider,
                new PredictionChangedProjectsProvider(graph, options));

            return (executor, options);
        }

        public static (AffectedOptions Options, AffectedSummary Summary) ExecuteAffectedExecutor(
            this InvocationContext ctx)
        {
            var (executor, options) = ctx.BuildAffectedExecutor();
            var summary = executor.Execute();
            summary.ThrowIfNoChanges();
            return (options, summary);
        }

        public static AffectedOptions GetAffectedOptions(
            this InvocationContext context)
        {
            var binder = new AffectedOptionsBinder();
            if (((IValueSource)binder).TryGetValue(binder, context.BindingContext, out var dataObj) &&
                dataObj is not null)
            {
                return (dataObj as AffectedOptions)!;
            }

            throw new InvalidOperationException("Failed to obtain AffectedOptions from context");
        }

        public static AffectedCommandOutputOptions GetAffectedCommandOutputOptions(
            this InvocationContext context,
            AffectedOptions options)
        {
            var binder = new AffectedCommandOutputOptionsBinder(options);
            if (((IValueSource)binder).TryGetValue(binder, context.BindingContext, out var dataObj) &&
                dataObj is not null)
            {
                return (dataObj as AffectedCommandOutputOptions)!;
            }

            throw new InvalidOperationException("Failed to obtain AffectedCommandOutputOptions from context");
        }
    }
}
