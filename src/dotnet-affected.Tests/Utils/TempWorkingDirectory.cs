using System;
using System.IO;

namespace Affected.Cli.Tests
{
    public class TempWorkingDirectory : IDisposable
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
