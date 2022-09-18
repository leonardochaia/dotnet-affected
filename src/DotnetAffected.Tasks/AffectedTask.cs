using System;
using System.Linq;
using DotnetAffected.Abstractions;
using DotnetAffected.Core;
using Microsoft.Build.Framework;
using Microsoft.Build.Graph;
using System.Collections.Generic;

namespace DotnetAffected.Tasks
{
    public class AffectedTask : Microsoft.Build.Utilities.Task
    {
        [Required]
        public string Root { get; set; }
    
        [Required]
        public ITaskItem[] Projects { get; set; }

        public ITaskItem[] AssumeChanges { get; set; }

        [Output]
        public string[] ModifiedProjects { get; private set; }

        [Output]
        public int ModifiedProjectsCount { get; private set; }

        public override bool Execute()
        {
            try
            {
                var affectedOptions = new AffectedOptions(Root);

                var graph = new ProjectGraphFactory(affectedOptions).BuildProjectGraph();
                IChangesProvider changesProvider = AssumeChanges?.Any() == true
                    ? new AssumptionChangesProvider(graph, AssumeChanges.Select(c => c.ItemSpec))
                    : new GitChangesProvider();

                var results = new AffectedExecutor(affectedOptions,
                    graph,
                    changesProvider,
                    new PredictionChangedProjectsProvider(graph, affectedOptions)).Execute();

                var affectedProjects = new HashSet<ProjectGraphNode>(results.ProjectsWithChangedFiles.Concat(results.AffectedProjects))
                    .Select(p => p.ProjectInstance.FullPath);
   
                ModifiedProjects = affectedProjects.ToArray();
                ModifiedProjectsCount = ModifiedProjects.Length;
            }
            catch (Exception e)
            {
                Log.LogErrorFromException(e);
            }

            return !Log.HasLoggedErrors;
        }

        static AffectedTask()
        {
            Lib2GitNativePathHelper.ResolveCustomNativeLibraryPath();
        }
    }
}
