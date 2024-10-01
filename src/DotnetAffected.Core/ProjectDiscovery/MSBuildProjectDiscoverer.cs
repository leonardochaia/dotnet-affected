using DotnetAffected.Abstractions;
using Microsoft.Build.Evaluation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DotnetAffected.Core
{
    /// <summary>
    /// Discovers projects based off the provided <see cref="Project"/>.
    /// </summary>
    internal class MSBuildProjectDiscoverer : IProjectDiscoverer
    {
        public IEnumerable<string> DiscoverProjects(IDiscoveryOptions options)
        {
            var traversalProjectPath = options.FilterFilePath;
            if (string.IsNullOrEmpty(traversalProjectPath) || !traversalProjectPath.EndsWith(".proj"))
            {
                throw new InvalidOperationException($"{traversalProjectPath} should be a .proj file");
            }

            var traversalProjectDirectory = Path.GetDirectoryName(traversalProjectPath);
            var traversalProject = new Project(traversalProjectPath);
            
            return traversalProject
                .GetItems("ProjectReference")
                .Select(i => i.EvaluatedInclude)
                .Select(p=> Path.IsPathRooted(p) ? p : Path.Join(traversalProjectDirectory, p))
                .Select(Path.GetFullPath)
                .Distinct()
                .ToArray();
        }
    }
}
