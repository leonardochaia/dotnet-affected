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
            var targetFramework = typeof(Utils).Assembly
                .GetCustomAttributes(typeof(System.Runtime.Versioning.TargetFrameworkAttribute), false)
                .OfType<System.Runtime.Versioning.TargetFrameworkAttribute>()
                .Single()
                .FrameworkName;

            var majorVersion = new Regex(".+,Version=v(\\d).(\\d)")
                .Match(targetFramework)
                .Groups.Values.Skip(1)
                .Select(g => int.Parse(g.Value))
                .First();

            switch (majorVersion)
            {
                case 3:
                    return "netcoreapp3.1";
                case >= 5:
                    return $"net{majorVersion}.0";
                default:
                    throw new NotSupportedException($"Invalid TargetFramework: {targetFramework}");
            }
        });
    }
}
