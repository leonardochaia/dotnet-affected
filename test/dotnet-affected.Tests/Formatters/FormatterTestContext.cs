using System.IO;

namespace Affected.Cli.Tests.Formatters
{
    internal static class FormatterTestContext
    {
        /// <summary>
        /// Formatters that don't care where their output lands get a throwaway
        /// context, so that their tests stay about the format itself.
        /// </summary>
        public static OutputFormatterContext Default { get; } =
            new OutputFormatterContext(Path.Combine(Path.GetTempPath(), "affected"));
    }
}
