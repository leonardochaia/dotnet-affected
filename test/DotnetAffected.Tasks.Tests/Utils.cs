using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DotnetAffected.Tasks.Tests
{
    public static class Utils
    {
        public static string TargetFramework => TargetFrameworkLocal.Value;

        public static string DotnetAffectedNugetDir => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? AppContext.BaseDirectory, "DotnetAffected");

        private static readonly Lazy<string> TargetFrameworkLocal = new (() =>
        {
            var targetFrameworkAttribute = typeof(Utils).Assembly
                .GetCustomAttributes(typeof(System.Runtime.Versioning.TargetFrameworkAttribute), false)
                .OfType<System.Runtime.Versioning.TargetFrameworkAttribute>()
                .Single();
            
            var frameworkName = new System.Runtime.Versioning.FrameworkName(targetFrameworkAttribute.FrameworkName);
            var majorVersion = frameworkName.Version.Major;

            switch (majorVersion)
            {
                case >= 5:
                    return $"net{majorVersion}.0";
                default:
                    throw new NotSupportedException($"Invalid TargetFramework: {frameworkName}");
            }
        });
    }
}
