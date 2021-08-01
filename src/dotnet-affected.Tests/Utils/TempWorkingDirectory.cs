using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Affected.Cli.Tests
{
    public sealed class TempWorkingDirectory : IDisposable
    {
        public string Path { get; }

        public TempWorkingDirectory()
        {
            Path = CreateTemporaryDirectory();
        }

        public void Dispose()
        {
            Directory.Delete(Path, true);
        }

        public string MakePathForCsProj(string projectName)
        {
            return System.IO.Path.Combine(Path, projectName, $"{projectName}.csproj");
        }

        public string MakePathFor(string fileName)
        {
            return System.IO.Path.Combine(Path, fileName);
        }

        public async Task<string> CreateSolutionFileForProjects(string solutionName, params string[] projectPaths)
        {
            var i = 0;
            var solutionContents = new SolutionFileBuilder
            {
                Projects = projectPaths.ToDictionary(p => i++.ToString())
            }.BuildSolution();

            var solutionPath = MakePathFor(solutionName);

            await File.WriteAllTextAsync(solutionPath, solutionContents);

            return solutionPath;
        }

        /// <summary>
        /// Generates a unique directory name in the temporary folder.
        /// Caller must delete when finished.
        /// </summary>
        private static string CreateTemporaryDirectory()
        {
            string temporaryDirectory = System.IO.Path.Combine(System.IO.Path.GetTempPath(),
                "Temporary" + Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(temporaryDirectory);

            return temporaryDirectory;
        }
    }
}
