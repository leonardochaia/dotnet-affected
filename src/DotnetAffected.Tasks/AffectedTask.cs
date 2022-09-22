using System;
using System.Linq;
using DotnetAffected.Abstractions;
using DotnetAffected.Core;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Collections;
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

        public ITaskItem[]? FilterClasses { get; set; }

        [Output]
        public ITaskItem[] FilterInstances { get; private set; }

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

                var executor = new AffectedExecutor(affectedOptions,
                                                    graph,
                                                    changesProvider,
                                                    new PredictionChangedProjectsProvider(graph, affectedOptions));

                var results = executor.Execute();
                var modifiedProjectInstances = new HashSet<ProjectInstance>();
                var modifiedProjects = new List<string>();
                var filterInstances = new List<ITaskItem>();
                var filterTypes = BuildFilterClassMetadata();

                foreach (var node in results.ProjectsWithChangedFiles.Concat(results.AffectedProjects))
                {
                    if (modifiedProjectInstances.Add(node.ProjectInstance))
                    {
                        modifiedProjects.Add(node.ProjectInstance.FullPath);

                        if (filterTypes.Length > 0)
                        {
                            var projectInstance = node.ProjectInstance;
                            foreach (var filterType in filterTypes)
                            {
                                var taskItem = new TaskItem(projectInstance.FullPath);
                                filterInstances.Add(taskItem);

                                foreach (var kvp in filterType)
                                    taskItem.SetMetadata(kvp.Key, projectInstance.GetProperty(kvp.Key)?.EvaluatedValue ?? kvp.Value);
                            }
                        }
                    }
                }

                FilterInstances = filterInstances.ToArray();
                ModifiedProjects = modifiedProjects.ToArray();
                ModifiedProjectsCount = ModifiedProjects.Length;
            }
            catch (Exception? e)
            {
                while (e is not null)
                {
                    Log.LogErrorFromException(e);
                    e = e.InnerException;
                }
            }

            return !Log.HasLoggedErrors;
        }
        
        private Dictionary<string, string>[] BuildFilterClassMetadata()
        {
            Dictionary<string, string> Selector(ITaskItem filter)
            {
                var t = new Dictionary<string, string>();
                foreach (var obj in filter.CloneCustomMetadata())
                {
                    var entry = (DictionaryEntry)obj;
                    t[(string)entry.Key] = entry.Value as string ?? "";
                }

                t["AffectedFilterClassName"] = filter.ItemSpec;
                return t;
            }

           return FilterClasses is null
               ? Array.Empty<Dictionary<string, string>>()
               : FilterClasses.Select(Selector).ToArray();
        }
        
        static AffectedTask()
        {
            Lib2GitNativePathHelper.ResolveCustomNativeLibraryPath();
        }
    }
}
