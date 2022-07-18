using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Affected.Cli.Tests
{
    /// <summary>
    /// Tests for ensure some common MSBuild stuff is getting evaluated
    /// </summary>
    public class EvaluationDetectionTests
        : BaseServiceProviderRepositoryTest
    {
        [Fact]
        public async Task When_file_is_removed_it_should_be_ignored()
        {
            // Create a dummy file
            var dummyFileContents = @"// dummy file contents";
            var dummyFilePath = Path.Combine(Repository.Path, "file.txt");
            await Repository.CreateTextFileAsync(dummyFilePath, dummyFileContents);

            // Create a project including dummy file
            var projectContents = @"
<Project>
    <PropertyGroup>
        <TargetFramework>net5.0</TargetFramework>
    </PropertyGroup>
    <ItemGroup>
        <None Remove=""file.txt"" />
    </ItemGroup>
</Project>
";
            var projectPath = Path.Combine(Repository.Path, "project1.csproj");
            await Repository.CreateTextFileAsync(projectPath, projectContents);

            // Commit all so there are no changes
            Repository.StageAndCommit();

            // Update the dummy file
            await Repository.CreateTextFileAsync(dummyFilePath, "// New Contents");

            Assert.Throws<NoChangesException>(() => Context.AffectedProjects);
        }
    }
}
