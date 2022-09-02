using System;
using System.IO;

namespace DotnetAffected.Testing.Utils
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
            this.Delete();
        }

        /// <summary>
        /// Generates a unique directory name in the temporary folder.
        /// Caller must delete when finished.
        /// </summary>
        private static string CreateTemporaryDirectory()
        {
            string temporaryDirectory = System.IO.Path.Combine(System.IO.Path.GetTempPath(),
                "Temporary" + Guid.NewGuid()
                    .ToString("N"));

            Directory.CreateDirectory(temporaryDirectory);

            return temporaryDirectory;
        }

        /// <summary>
        /// REMARKS: git2LibSharp leaves some read only files that <see cref="Directory.Delete(string)"/>
        /// has trouble deleting.
        /// source: https://stackoverflow.com/a/26372070
        /// deletion source: https://stackoverflow.com/a/8714329
        /// </summary>
        private void Delete()
        {
            var directory = new DirectoryInfo(this.Path)
            {
                Attributes = FileAttributes.Normal
            };

            foreach (var info in directory.GetFileSystemInfos("*", SearchOption.AllDirectories))
            {
                info.Attributes = FileAttributes.Normal;
            }

            directory.Delete(true);
        }
    }
}
