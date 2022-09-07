using DotnetAffected.Core;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Linq;

namespace Affected
{
    /// <summary>
    /// Simplest example of an msbuild task
    /// </summary>
    public class AffectedTask : Task
    {
        public override bool Execute()
        {
            Log.LogMessage("Starting Affected Task");
            var options = new AffectedOptions(
                this.RepositoryPath);
            var graph = new ProjectGraphFactory(options)
                .BuildProjectGraph();
            var executor = new AffectedExecutor(options, graph);
            var summary = executor.Execute();

            this.AffectedProjects = string.Join(",",
                summary.AffectedProjects.Select(p => p.GetFullPath()));

            return true;
        }

        [Required] public string RepositoryPath { get; set; }

        [Output] public string AffectedProjects { get; set; }
    }
}
