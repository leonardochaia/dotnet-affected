using System;
using System.Collections.Generic;
using System.CommandLine.Builder;
using System.CommandLine.IO;
using System.CommandLine.Rendering;
using System.Diagnostics.CodeAnalysis;

namespace Affected.Cli
{
    internal static class ErrorHandlingMiddleware
    {
        /// <summary>
        /// Adds an easy way to map an exception to an exit code and message.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configs"></param>
        /// <returns></returns>
        public static CommandLineBuilder UseRenderingErrorHandler(
            this CommandLineBuilder builder,
            IDictionary<Type, RenderingErrorConfig> configs)
        {
            return builder.AddMiddleware(async (context, next) =>
            {
                try
                {
                    await next(context);
                }
                catch (Exception exception)
                {
                    bool Applies(Exception ex, [NotNullWhen(true)] out RenderingErrorConfig? output)
                    {
                        if (configs.TryGetValue(ex.GetType(), out output))
                        {
                            return true;
                        }

                        return ex.InnerException is not null && Applies(ex.InnerException, out output);
                    }

                    if (!Applies(exception, out var config))
                    {
                        throw;
                    }

                    context.ExitCode = config.ExitCode;

                    if (config.ErrorView is not null)
                    {
                        context.Console.Append(config.ErrorView);
                    }
                    else
                    {
                        context.Console.Error.WriteLine(exception.Message);
                    }
                }
            });
        }
    }
}
