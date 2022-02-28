using System;
using System.IO;

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
            this.Delete();
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
        
        /// <summary>
        /// REMARKS: git2LibSharp leaves some read only files that <see cref="Directory.Delete(string)"/>
        /// has trouble deleting.
        /// source: https://stackoverflow.com/a/26372070
        /// </summary>
        private void Delete()
        {
            void Recursion(string directory)
            {
                foreach (var subdirectory in Directory.EnumerateDirectories(directory)) 
                {
                    Recursion(subdirectory);
                }

                foreach (var fileName in Directory.EnumerateFiles(directory))
                {
                    var fileInfo = new FileInfo(fileName)
                    {
                        Attributes = FileAttributes.Normal
                    };
                    fileInfo.Delete();
                }

                Directory.Delete(directory);
            }
            
            Recursion(this.Path);
        }
    }
}
