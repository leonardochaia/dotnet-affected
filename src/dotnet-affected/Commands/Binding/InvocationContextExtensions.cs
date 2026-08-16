using DotnetAffected.Abstractions;
using DotnetAffected.Core;
using System;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.CommandLine.IO;

namespace Affected.Cli.Commands
{
    internal static class InvocationContextExtensions
    {
        public static (IAffectedExecutor Executor, AffectedOptions Options) BuildAffectedExecutor(
            this InvocationContext ctx)
        {
            var options = ctx.GetAffectedOptions();

            // Deliberately no graph: the executor defers building it until the changed files are
            // known, so files the diff removed are restored before evaluation and stay attributed
            // to their project. Supplying one here opts out of that.
            // See https://github.com/leonardochaia/dotnet-affected/issues/84
            var executor = new AffectedExecutor(options, changesProvider: new GitChangesProvider());

            return (executor, options);
        }

        public static (AffectedOptions Options, AffectedSummary Summary) ExecuteAffectedExecutor(
            this InvocationContext ctx)
        {
            var (executor, options) = ctx.BuildAffectedExecutor();
            var summary = executor.Execute();

            ctx.ReportDiagnostics(summary);

            summary.ThrowIfNoChanges();
            return (options, summary);
        }

        /// <summary>
        /// Writes diagnostics to standard error, so that they stay out of whatever the chosen
        /// formatter is writing to standard output.
        /// </summary>
        private static void ReportDiagnostics(this InvocationContext ctx, AffectedSummary summary)
        {
            var verbose = ctx.ParseResult.GetValueForOption(AffectedGlobalOptions.VerboseOption);

            foreach (var diagnostic in summary.Diagnostics)
            {
                switch (diagnostic.Severity)
                {
                    case AffectedDiagnosticSeverity.Warning:
                        ctx.Console.Error.WriteLine($"warning: {diagnostic.Message}");
                        break;

                    // Kept behind --verbose on purpose: a change to something no project builds,
                    // documentation being the usual one, is normal and would otherwise be noise
                    // on a large share of runs.
                    case AffectedDiagnosticSeverity.Info when verbose:
                        ctx.Console.Error.WriteLine($"info: {diagnostic.Message}");
                        break;
                }
            }
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
