using Affected.Cli.Tests;
using Microsoft.Build.Construction;
using Microsoft.Build.Graph;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Affected.Cli.Benchmarks
{
    public static class RepositoryExtensions
    {
        private static readonly Random Rnd = new Random();

        public static ProjectRootElement CreateCsProjTree(
            this TemporaryRepository repository,
            int maxDepth,
            int maxChildren = 100)
        {
            var name = $"project-{Rnd.Next()}";
            var node = repository.CreateCsProject(name);
            if (maxDepth > 0)
            {
                var childrenCount = Rnd.Next(maxChildren);
                for (int i = 0; i < childrenCount; i++)
                {
                    var child = repository.CreateCsProjTree(maxDepth - 1, maxChildren);
                    node.AddProjectDependency(child.FullPath);
                }
            }

            return node;
        }

        public static void RandomizeChangesInProjectTree(
            this TemporaryRepository repository,
            ProjectRootElement rootElement)
        {
            var graph = new ProjectGraph(rootElement.FullPath);
            foreach (var node in graph.ProjectNodes)
            {
                var rnd = Rnd.Next();
                if (rnd % 2 != 0)
                    continue;

                var filePath = Path.Combine(node.ProjectInstance.Directory, $"file-{rnd}.cs");
                Task.Run(() => repository.CreateTextFileAsync(filePath, $"// contents {rnd}"));
            }
        }
    }
}
