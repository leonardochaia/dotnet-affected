using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Affected.Cli.Tests
{
    /// <summary>
    /// Tests for detecting affected projects when directory build props changes
    /// </summary>
    public class DirectoryBuildPropsDetectionTests
        : BaseServiceProviderRepositoryTest
    {
        [Theory]
        [InlineData("Directory.Build.props")]
        [InlineData("Directory.Build.targets")]
        public async Task When_Directory_Build_props_is_updated_dependant_projects_should_be_affected(
            string propsName)
        {
            // Create a Directory.Build.props file
            var propsFile = @"
<Project>
    <PropertyGroup>
        <TargetFramework>net5.0</TargetFramework>
    </PropertyGroup>
</Project>
";
            var propsPath = Path.Combine(Repository.Path, propsName);
            await Repository.CreateTextFileAsync(propsPath, propsFile);

            // Create a project
            var dependantProjectName = "InventoryManagement";
            this.Repository.CreateCsProject(
                dependantProjectName);

            // Commit all so there are no changes
            Repository.StageAndCommit();

            // Update versions in props file
            propsFile = @"
<Project>
    <PropertyGroup>
        <TargetFramework>net6.0</TargetFramework>
    </PropertyGroup>
</Project>
";
            await Repository.CreateTextFileAsync(propsPath, propsFile);

            Assert.Single(Context.ChangedProjects);
            Assert.Empty(Context.AffectedProjects);
        }

        [Theory]
        [InlineData("Directory.Build.props")]
        [InlineData("Directory.Build.targets")]
        public async Task When_Directory_Build_props_has_imports_updated_dependant_projects_should_be_affected(
            string propsName)
        {
            // Create a props file to define defaults
            var defaultPropsFile = @"
<Project>
    <PropertyGroup>
        <TargetFramework>net5.0</TargetFramework>
    </PropertyGroup>
</Project>
";
            var defaultPropsPath = Path.Combine(Repository.Path, "defaults.props");
            await Repository.CreateTextFileAsync(defaultPropsPath, defaultPropsFile);

            // Create a Directory.Build.props file importing the defaults.props
            var propsFile = @"
<Project>
    <Import Project=""defaults.props"" />
    <PropertyGroup>
        <TestProperty>1992</TestProperty>
    </PropertyGroup>
</Project>
";
            var propsPath = Path.Combine(Repository.Path, propsName);
            await Repository.CreateTextFileAsync(propsPath, propsFile);

            // Create a project
            var dependantProjectName = "InventoryManagement";
            this.Repository.CreateCsProject(
                dependantProjectName);

            // Commit all so there are no changes
            Repository.StageAndCommit();

            // Update the default props file
            defaultPropsFile = @"
<Project>
    <PropertyGroup>
        <TargetFramework>net6.0</TargetFramework>
    </PropertyGroup>
</Project>
";
            await Repository.CreateTextFileAsync(defaultPropsPath, defaultPropsFile);

            Assert.Single(Context.ChangedProjects);
            Assert.Empty(Context.AffectedProjects);
        }

        [Theory]
        [InlineData("Directory.Build.props")]
        [InlineData("Directory.Build.targets")]
        public async Task When_nested_Directory_Build_props_is_updated_only_dependant_projects_should_be_affected(
            string propsName)
        {
            // Create a root Directory.Build.props
            var propsFile = @"
<Project>
    <PropertyGroup>
        <TargetFramework>net5.0</TargetFramework>
    </PropertyGroup>
</Project>
";
            var propsPath = Path.Combine(Repository.Path, propsName);
            await Repository.CreateTextFileAsync(propsPath, propsFile);

            // Create a project
            var dependantProjectName = "Inventory/InventoryManagement";
            this.Repository.CreateCsProject(
                dependantProjectName);

            // Create a nested Directory.Build.props
            var nestedPropsFile = @"
<Project>
    <PropertyGroup>
        <TestProperty>20090103</TestProperty>
    </PropertyGroup>
</Project>
";
            var nestedPropsPath = Path.Combine(Repository.Path, "Inventory", propsName);
            await Repository.CreateTextFileAsync(nestedPropsPath, nestedPropsFile);

            // Create another project that should not be affected by the Inventory props file
            var otherProjectName = "Purchasing/PurchaseOrderManager";
            this.Repository.CreateCsProject(
                otherProjectName);

            // Commit all so there are no changes
            Repository.StageAndCommit();

            // Update versions in props file
            nestedPropsFile = @"
<Project>
    <PropertyGroup>
        <TestProperty>20220103</TestProperty>
    </PropertyGroup>
</Project>
";
            await Repository.CreateTextFileAsync(nestedPropsPath, nestedPropsFile);

            Assert.Single(Context.ChangedProjects);
            Assert.Equal("Inventory/InventoryManagement", Context.ChangedProjects.First()
                .Name);
            Assert.Empty(Context.AffectedProjects);
        }

        [Theory]
        [InlineData("Directory.Build.props")]
        [InlineData("Directory.Build.targets")]
        public async Task When_root_Directory_Build_props_is_updated_only_dependant_projects_should_be_affected(
            string propsName)
        {
            // Create a root Directory.Build.props
            var propsFile = @"
<Project>
    <PropertyGroup>
        <TargetFramework>net5.0</TargetFramework>
    </PropertyGroup>
</Project>
";
            var propsPath = Path.Combine(Repository.Path, propsName);
            await Repository.CreateTextFileAsync(propsPath, propsFile);

            // Create a project
            var dependantProjectName = "Inventory/InventoryManagement";
            this.Repository.CreateCsProject(
                dependantProjectName);

            // Create a nested Directory.Build.props
            var nestedPropsFile = @"
<Project>
    <PropertyGroup>
        <TestProperty>20090103</TestProperty>
    </PropertyGroup>
</Project>
";
            var nestedPropsPath = Path.Combine(Repository.Path, "Inventory", propsName);
            await Repository.CreateTextFileAsync(nestedPropsPath, nestedPropsFile);

            // Create another project that should not be affected by the Inventory props file
            var otherProjectName = "Purchasing/PurchaseOrderManager";
            this.Repository.CreateCsProject(
                otherProjectName);

            // Commit all so there are no changes
            Repository.StageAndCommit();

            // Update versions in props file
            propsFile = @"
<Project>
    <PropertyGroup>
        <TargetFramework>net6.0</TargetFramework>
    </PropertyGroup>
</Project>
";
            await Repository.CreateTextFileAsync(propsPath, propsFile);

            Assert.Single(Context.ChangedProjects);
            Assert.Equal("Purchasing/PurchaseOrderManager",
                Context.ChangedProjects.First()
                    .Name);
            Assert.Empty(Context.AffectedProjects);
        }
    }
}
