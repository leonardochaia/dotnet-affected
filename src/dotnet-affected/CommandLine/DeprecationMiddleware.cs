using Affected.Cli.Commands;
using System.CommandLine.Builder;
using System.CommandLine.IO;

namespace Affected.Cli
{
    internal static class DeprecationMiddleware
    {
        /// <summary>
        /// Warns about options that are on their way out, before the command runs.
        /// </summary>
        /// <remarks>
        /// Middleware rather than per command: every command reads the same global options, and a
        /// deprecation nobody is told about is one nobody migrates off.
        /// </remarks>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static CommandLineBuilder UseDeprecationWarnings(this CommandLineBuilder builder)
        {
            return builder.AddMiddleware(async (context, next) =>
            {
                // Warned about even when it validates. --to now only names the commit the working
                // tree is already checked out at, so it changes nothing about the comparison, and
                // the only way anyone finds that out before it is removed is by being told.
                if (context.ParseResult.FindResultFor(AffectedGlobalOptions.ToOption) is not null)
                {
                    context.Console.Error.WriteLine("warning: --to is deprecated and will be removed in v8.");
                }

                await next(context);
            });
        }
    }
}
