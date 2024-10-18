using System.Collections.Generic;

namespace Affected.Cli
{
    /// <summary>
    /// Available output strategies for dotnet-affected command.
    /// </summary>
    internal static class OutputStrategies
    {
        public const string Combined = "combined";
        public const string Split = "split";
        
        public static readonly IReadOnlyList<string> All = new[] { Combined, Split };
    }
}
