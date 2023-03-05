using System;
using System.Linq;
using DotnetAffected.Abstractions;
using DotnetAffected.Core;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace DotnetAffected.Tasks
{
    /// <inheritdoc />
    public class AffectedTask : Microsoft.Build.Utilities.Task
    {
#pragma warning disable CS1591
        [Required] public string Root { get; set; } = null!;

        public ITaskItem[]? AssumeChanges { get; set; } = null!;

        public string? FromRef { get; set; }

        public string? ToRef { get; set; }

        public ITaskItem[]? FilterClasses { get; set; }

        [Output] public ITaskItem[] FilterInstances { get; private set; } = null!;

        [Output] public string[] ModifiedProjects { get; private set; } = null!;

        [Output] public int ModifiedProjectsCount { get; private set; }
#pragma warning restore CS1591

        /// <inheritdoc />
        public override bool Execute()
        {
            try
            {
                var affectedOptions = new AffectedOptions(Root, null, FromRef ?? "", ToRef ?? "");

                if (AssumeChanges is not null
                    && AssumeChanges.Length > 0
                    && (!string.IsNullOrWhiteSpace(affectedOptions.FromRef) ||
                        !string.IsNullOrWhiteSpace(affectedOptions.ToRef)))
                {
                    Log.LogWarning(
                        "DotnetAffected AssumeChanges is set along with FromRef/ToRef. Only AssumeChanges is used.");
                }

                var graph = new ProjectGraphFactory(affectedOptions).BuildProjectGraph();
                IChangesProvider changesProvider = AssumeChanges?.Any() == true
                    ? new AssumptionChangesProvider(graph,
                        AssumeChanges.Select(c => Path.GetFileNameWithoutExtension(c.ItemSpec)))
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
                                    taskItem.SetMetadata(kvp.Key, projectInstance.GetProperty(kvp.Key)
                                        ?.EvaluatedValue ?? kvp.Value);
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
                foreach (var entry in filter.CloneCustomMetadata()
                             .Cast<KeyValuePair<string, string>>())
                {
                    t[entry.Key] = entry.Value ?? "";
                }

                t["AffectedFilterClassName"] = filter.ItemSpec;
                return t;
            }

            return FilterClasses is null
                ? Array.Empty<Dictionary<string, string>>()
                : FilterClasses.Select(Selector)
                    .ToArray();
        }

        static AffectedTask()
        {
            Lib2GitNativePathHelper.ResolveCustomNativeLibraryPath();
        }
    }
}
