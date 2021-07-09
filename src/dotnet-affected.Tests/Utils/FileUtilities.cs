using System;
using System.IO;

namespace Affected.Cli.Tests
{
    /// <summary>
    /// This class contains utility methods for file IO.
    /// Credits to: https://github.com/dotnet/msbuild/blob/9bcc06cbe19ae2482ab18eab90a82fd079b26897/src/Shared/TempFileUtilities.cs
    /// </summary>
    public static class FileUtilities
    {
        /// <summary>
        /// Generates a unique directory name in the temporary folder.
        /// Caller must delete when finished.
        /// </summary>
        /// <param name="createDirectory"></param>
        /// <param name="subfolder"></param>
        internal static string GetTemporaryDirectory(bool createDirectory = true, string subfolder = null)
        {
            string temporaryDirectory = Path.Combine(Path.GetTempPath(), "Temporary" + Guid.NewGuid().ToString("N"),
                subfolder ?? string.Empty);

            if (createDirectory)
            {
                Directory.CreateDirectory(temporaryDirectory);
            }

            return temporaryDirectory;
        }

        /// <summary>
        /// Generates a unique temporary file name with a given extension in the temporary folder.
        /// File is guaranteed to be unique.
        /// Extension may have an initial period.
        /// Caller must delete it when finished.
        /// May throw IOException.
        /// </summary>
        internal static string GetTemporaryFile(string extension)
        {
            return GetTemporaryFile(null, extension);
        }

        /// <summary>
        /// Creates a file with unique temporary file name with a given extension in the specified folder.
        /// File is guaranteed to be unique.
        /// Extension may have an initial period.
        /// If folder is null, the temporary folder will be used.
        /// Caller must delete it when finished.
        /// May throw IOException.
        /// </summary>
        internal static string GetTemporaryFile(string directory, string extension, bool createFile = true)
        {
            if (extension[0] != '.')
            {
                extension = '.' + extension;
            }

            directory ??= Path.GetTempPath();

            Directory.CreateDirectory(directory);

            string file = Path.Combine(directory, $"tmp{Guid.NewGuid():N}{extension}");

            if (createFile)
            {
                File.WriteAllText(file, string.Empty);
            }

            return file;
        }

        public class TempWorkingDirectory : IDisposable
        {
            public string Path { get; }

            public TempWorkingDirectory()
            {
                Path = GetTemporaryDirectory();

                if (Directory.Exists(Path))
                {
                    Directory.Delete(Path, true);
                }
            }

            public void Dispose()
            {
                Directory.Delete(Path, true);
            }
            
            /// <summary>
            /// Generates a unique temporary file name with the .csproj extension name this directory.
            /// </summary>
            /// <returns>The path to a csproj file</returns>
            public string CreateTemporaryCsProjFile(string key = "")
            {
                return GetTemporaryFile(Path, $"{key}.csproj");
            }
        }
    }
}
