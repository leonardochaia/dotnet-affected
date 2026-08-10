using DotnetAffected.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DotnetAffected.Core
{
    /// <summary>
    /// Decides which projects exist as far as the rest of the tool is concerned: picks a discoverer
    /// for the input, then applies <see cref="IDiscoveryOptions.ExcludeDiscoveryRegex"/>.
    ///
    /// Exclusion belongs here rather than further down the pipeline. Everything after this point
    /// works from an evaluated project graph, and a project that cannot be evaluated takes the run
    /// down while it is being built. See https://github.com/leonardochaia/dotnet-affected/issues/146
    /// </summary>
    internal class ProjectDiscoveryManager
    {
        public ProjectDiscoveryResult DiscoverProjects(IDiscoveryOptions options)
        {
            var discovered = SelectDiscoverer(options)
                .DiscoverProjects(options)
                // REMARKS: a solution references its projects relatively and
                // SolutionFileProjectDiscoverer only joins them onto the solution's directory, so
                // segments like "../" survive. The pattern used to be matched against
                // ProjectInstance.FullPath, which MSBuild had already normalized, so normalizing
                // here is what keeps existing patterns matching what they always matched.
                .Select(Path.GetFullPath)
                .ToArray();

            var pattern = options.ExcludeDiscoveryRegex;

            if (string.IsNullOrEmpty(pattern))
                return new ProjectDiscoveryResult(discovered, Array.Empty<string>());

            var regex = new Regex(pattern);
            var projects = new List<string>();
            var excluded = new List<string>();

            foreach (var project in discovered)
            {
                if (regex.IsMatch(project))
                    excluded.Add(project);
                else
                    projects.Add(project);
            }

            return new ProjectDiscoveryResult(projects.ToArray(), excluded.ToArray());
        }

        private static IProjectDiscoverer SelectDiscoverer(IDiscoveryOptions options)
        {
            // Whe no filtering file is provided, discover from file system.
            if (options.FilterFilePath == null)
            {
                return new DirectoryProjectDiscoverer();
            }

            // When a filtering file is provided, use a specific discoverer based on its path.
            if (options.FilterFilePath.EndsWith(".sln") || options.FilterFilePath.EndsWith(".slnx"))
            {
                return new SolutionFileProjectDiscoverer();
            }

            // Solution filters cannot go through SolutionSerializers,
            // Microsoft.VisualStudio.SolutionPersistence does not support them.
            if (options.FilterFilePath.EndsWith(".slnf"))
            {
                return new SolutionFilterProjectDiscoverer();
            }

            if (options.FilterFilePath.EndsWith(".proj"))
            {
                return new MSBuildProjectDiscoverer();
            }

            throw new NotImplementedException($"Filtering by {options.FilterFilePath} is not yet implemented");
        }
    }
}
