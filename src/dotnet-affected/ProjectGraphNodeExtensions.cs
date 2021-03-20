using Microsoft.Build.Graph;

namespace Affected.Cli
{
    internal static class ProjectGraphNodeExtensions
    {
        internal static string GetProjectName(this ProjectGraphNode node)
        {
            return node.ProjectInstance.GetPropertyValue("ProjectName");
        }
    }
}
