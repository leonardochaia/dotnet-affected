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
        public static async Task PrepareTaskInfra(this TemporaryRepository repo, string importResource = null)
        {
            await repo.CreateTextFileAsync("Directory.Build.props", TestProjectScenarios.DirectoryBuildProps);
            await repo.CreateTextFileAsync("ci.props", TestProjectScenarios.CiProps);

            var hasImportResource = !string.IsNullOrWhiteSpace(importResource);
            var ciProps = ProjectRootElement.Open(Path.Combine(repo.Path, "ci.props"))!;

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
