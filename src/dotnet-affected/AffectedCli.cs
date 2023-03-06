using Affected.Cli.Commands;
using Affected.Cli.Views;
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
        public static CommandLineBuilder CreateAffectedCommandLineBuilder()
        {
            return new CommandLineBuilder(new AffectedRootCommand())
                .UseDefaults()
                .UseRenderingErrorHandler(new Dictionary<Type, RenderingErrorConfig>
                {
                    [typeof(NoChangesException)] = new(AffectedExitCodes.NothingChanged, new NoChangesView()),
                });
        }
    }
}
