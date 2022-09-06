using Microsoft.Build.Locator;

namespace DotnetAffected.Testing.Utils
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
