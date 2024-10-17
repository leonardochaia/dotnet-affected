using DotnetAffected.Testing.Utils;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace DotnetAffected.Core.Tests
{
    /// <summary>
    /// 
    /// </summary>
    public class NugetHelperTests : BaseDotnetAffectedTest
    {
        [Fact]
        public async Task When_Version_Attribute_Casing_Is_Not_Standard_Using_Central_Management_Package_Should_Not_Throw()
        {
            // Create a Directory.Packages.props file with non standard casing for "Version" attribute
            var propsFile = @"
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
    <PackageVersion Include=""Some.Library"" version=""5.0.0"" />
  </ItemGroup>
</Project>
";
            var propsPath = Path.Combine(Repository.Path, "Directory.Packages.props");
            await Repository.CreateTextFileAsync(propsPath, propsFile);

            // Create a project with a nuget dependency
            const string projectName = "InventoryManagement";
            Repository.CreateCsProject(
                projectName,
                b => b.AddNuGetDependency("Some.Library"));
            
            // Commit all so there are no changes
            Repository.StageAndCommit();

            // update Directory.Packages.props file
            propsFile = @"
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
    <PackageVersion Include=""Some.Library"" version=""6.0.0"" />
  </ItemGroup>
</Project>
";
            await Repository.CreateTextFileAsync(propsPath, propsFile);

            // Verify that affected does not throw
            var exception  = Record.Exception(() => AffectedSummary);
            Assert.Null(exception);
        }
    }
}
