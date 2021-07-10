using Microsoft.Build.Locator;

namespace Affected.Cli.Tests
{
    /// <summary>
    /// Makes sure MSBuild is properly loaded for all tests
    /// </summary>
    public class BaseMSBuildTest
    {
        static BaseMSBuildTest()
        {
            MSBuildLocator.RegisterDefaults();
        }
    }
}
