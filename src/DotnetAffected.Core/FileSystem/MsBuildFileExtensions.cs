using System;
using System.Collections.Generic;
using System.IO;

namespace DotnetAffected.Core.FileSystem
{
    /// <summary>
    /// Extensions MSBuild uses for projects and imports.
    ///
    /// Imports are not required to use one of these, but recognising them by extension keeps
    /// this off the read path. Imports using a non standard extension are deliberately not
    /// recognised. If that ever needs supporting, an option carrying extra extensions is a
    /// better trade than inspecting the contents of every file that gets probed.
    /// </summary>
    internal static class MsBuildFileExtensions
    {
        private static readonly HashSet<string> Known =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".props", ".targets", ".proj", ".csproj", ".fsproj", ".vbproj",
                ".vcxproj", ".projitems", ".shproj", ".tasks", ".overridetasks", ".user",
            };

        public static bool IsMsBuildProjectFile(string path)
            => Known.Contains(Path.GetExtension(path));
    }
}
