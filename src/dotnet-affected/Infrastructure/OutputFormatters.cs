using Affected.Cli.Formatters;

namespace Affected.Cli
{
    internal static class OutputFormatters
    {
        public static readonly IOutputFormatter[] All = new IOutputFormatter[]
        {
            new TextOutputFormatter(), new TraversalProjectOutputFormatter()
        };
    }
}
