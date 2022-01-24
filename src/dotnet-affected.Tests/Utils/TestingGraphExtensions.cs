using Microsoft.Build.Construction;
using System.Collections.Generic;
using System.Linq;

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

        public static ProjectRootElement AddNuGetDependency(this ProjectRootElement element, string packageName, string version = null)
        {
            var metaData = version != null
                ? new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("Version", version) }
                : null;

            element.AddItem("PackageReference", packageName, metaData);
            return element;
        }

        public static ProjectRootElement OptOutFromCentrallyManagedNuGetPackageVersions(this ProjectRootElement element)
        {
            element.AddProperty("ManagePackageVersionsCentrally", "false");
            return element;
        }

        public static ProjectRootElement AddPackageVersion(this ProjectRootElement element, string packageName, string version)
        {
            var itemGroup = element.ItemGroups.FirstOrDefault() ?? element.AddItemGroup();
            itemGroup.AddItem("PackageVersion", packageName, new []{ new KeyValuePair<string, string>("Version", version)});

            return element;
        }
    }
}
