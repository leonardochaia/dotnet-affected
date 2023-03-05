using System;
using System.IO;
using System.Runtime.InteropServices;

namespace DotnetAffected.Tasks
{
    internal static class Lib2GitNativePathHelper
    {
        public static void ResolveCustomNativeLibraryPath()
        {
            var assemblyDirectory = Path.GetDirectoryName(typeof(LibGit2Sharp.GlobalSettings).Assembly.Location);
            var runtimesDirectory = Path.Combine(assemblyDirectory ?? "", "runtimes");

            if (!Directory.Exists(runtimesDirectory))
                return;

            var processorArchitecture = RuntimeInformation.ProcessArchitecture.ToString()
                .ToLowerInvariant();

            var (os, libExtension) = GetNativeInfo();

            foreach (var runtimeFolder in Directory.GetDirectories(runtimesDirectory, $"{os}*-{processorArchitecture}"))
            {
                var libFolder = Path.Combine(runtimeFolder, "native");

                foreach (var libFilePath in Directory.GetFiles(libFolder, $"*{libExtension}"))
                {
                    if (IsLibraryLoadable(libFilePath))
                    {
                        LibGit2Sharp.GlobalSettings.NativeLibraryPath = libFolder;
                        return;
                    }
                }
            }
        }

        private static (string Os, string LibExtension) GetNativeInfo()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return ("linux", ".so");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return ("osx", ".dylib");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return ("win", ".dll");

            throw new PlatformNotSupportedException();
        }

        private static bool IsLibraryLoadable(string libPath)
        {
            if (File.Exists(libPath) && NativeLibrary.TryLoad(libPath, out var ptr))
            {
                NativeLibrary.Free(ptr);
                return true;
            }

            return false;
        }
    }
}
