using Microsoft.Build.Construction;

namespace Affected.Cli.Tests
{
    internal static class TestingGraphExtensions
    {
        public static ProjectRootElement SetName(this ProjectRootElement element, string name)
        {
            element.AddProperty("ProjectName", name);
            return element;
        }

        public static ProjectRootElement AddProjectDependency(this ProjectRootElement element, string dependencyPath)
        {
            element.AddItem("ProjectReference", dependencyPath);
            return element;
        }
    }
}
