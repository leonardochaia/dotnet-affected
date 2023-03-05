using Microsoft.Build.Evaluation;
using System.Diagnostics.CodeAnalysis;

namespace DotnetAffected.Core
{
    internal static class ProjectItemExtensions
    {
        public static bool MatchPropertyFlag(this Project project, string propertyName, bool value)
            => project.GetPropertyValue(propertyName)
                .ToLower() == (value ? "true" : "false");

        public static bool MatchMetadataFlag(this ProjectItem projectItem, string name, bool value)
            => projectItem.GetMetadataValue(name)
                .ToLower() == (value ? "true" : "false");

        public static bool TryGetMetadataValue(this ProjectItem projectItem, string name,
            [NotNullWhen(true)] out string? value)
        {
            value = projectItem.GetMetadataValue(name);
            return !string.IsNullOrEmpty(value);
        }
    }
}
