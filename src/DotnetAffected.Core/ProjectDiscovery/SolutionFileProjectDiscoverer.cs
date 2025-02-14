using DotnetAffected.Abstractions;
using Microsoft.VisualStudio.SolutionPersistence.Serializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DotnetAffected.Core
{
    internal class SolutionFileProjectDiscoverer : IProjectDiscoverer
    {
        public IEnumerable<string> DiscoverProjects(IDiscoveryOptions options)
        {
            // It should not be possible for this to be null based on call paths - but this makes the warning go away
            ArgumentNullException.ThrowIfNull(options.FilterFilePath);

            var serializer = SolutionSerializers.GetSerializerByMoniker(options.FilterFilePath);

            if (serializer is null) throw new NotImplementedException($"Filtering by {options.FilterFilePath} is not yet implemented");

            var solution = serializer.OpenAsync(options.FilterFilePath, CancellationToken.None).GetAwaiter().GetResult();

            return solution.SolutionProjects
                .Select(x => x.FilePath)
                .ToArray();
        }
    }
}
