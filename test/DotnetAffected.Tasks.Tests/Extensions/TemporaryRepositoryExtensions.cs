using DotnetAffected.Tasks.Tests.Resources;
using DotnetAffected.Testing.Utils;
using Microsoft.Build.Construction;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DotnetAffected.Tasks.Tests
{
    public static class TemporaryRepositoryExtensions
    {
        public static async Task PrepareTaskInfra(this TemporaryRepository repo, string? importResource = null)
        {
            await repo.CreateTextFileAsync("Directory.Build.props", TestProjectScenarios.DirectoryBuildProps);
            await repo.CreateTextFileAsync("ci.props", TestProjectScenarios.CiProps);

            var hasImportResource = !string.IsNullOrWhiteSpace(importResource);
            var isNetCoreApp31 = Utils.TargetFramework == "netcoreapp3.1";
            if (!hasImportResource && !isNetCoreApp31)
                return;

            var ciProps = ProjectRootElement.Open(Path.Combine(repo.Path, "ci.props"))!;

            if (isNetCoreApp31)
            {
                // "ci.props" imports DotnetAffected.Tasks as an Sdk
                // <Import Project="Sdk.props" Sdk="$(DotnetAffectedNugetDir)" />
                // 
                // When we execute the build within the test we provide the property "DotnetAffectedNugetDir" with the lib's location
                // For some reason it's not working in 3.1 so we override
                foreach (var importElement in ciProps.Imports)
                    importElement.Sdk = Utils.DotnetAffectedNugetDir;
            }

            if (hasImportResource)
            {
                var fileName = $"./{Guid.NewGuid().ToString()}.props";
                await repo.CreateTextFileAsync(fileName, importResource);
                ciProps.AddImport(fileName);
            }

            ciProps.Save();
        }
    }
}
