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

        public string? Uncommitted { get; set; }

        public ITaskItem[]? FilterClasses { get; set; }

        public bool HonourGitIgnore { get; set; } = true;

        [Output] public ITaskItem[] FilterInstances { get; private set; } = null!;

        [Output] public string[] ModifiedProjects { get; private set; } = null!;

        [Output] public int ModifiedProjectsCount { get; private set; }
#pragma warning restore CS1591

        /// <inheritdoc />
        public override bool Execute()
        {
            try
            {
                // Item specs are passed through as given: a glob expands to project paths, while a
                // bare item like "MyProject" is matched by project name.
                var assumeChanges = AssumeChanges?.Select(c => c.ItemSpec)
                    .ToArray() ?? Array.Empty<string>();

                var affectedOptions = new AffectedOptions(Root, null, FromRef ?? "", ToRef ?? "",
                    null, assumeChanges, honourGitIgnore: HonourGitIgnore,
                    uncommittedChanges: ParseUncommitted());

                if (assumeChanges.Length > 0
                    && (!string.IsNullOrWhiteSpace(affectedOptions.FromRef) ||
                        !string.IsNullOrWhiteSpace(affectedOptions.ToRef)))
                {
                    Log.LogWarning(
                        "DotnetAffected AssumeChanges is set along with FromRef/ToRef. Only AssumeChanges is used.");
                }
                else if (!string.IsNullOrWhiteSpace(affectedOptions.ToRef))
                {
                    // Warned about even when it validates, for the same reason the CLI does.
                    // See DeprecationMiddleware.
                    Log.LogWarning(
                        "DotnetAffectedToRef is deprecated and will be removed in v8. Projects are " +
                        "discovered and evaluated from the working tree, so it is only accepted when " +
                        "it names the commit the working tree is checked out at, which makes it a " +
                        "no-op. Remove it, and use DotnetAffectedUncommitted to choose what the " +
                        "working tree contributes.");
                }

                // Deliberately no graph: the executor defers building it until the changed files
                // are known, so files the diff removed are restored before evaluation and stay
                // attributed to their project. Supplying one here opts out of that.
                // See https://github.com/leonardochaia/dotnet-affected/issues/84
                var executor = new AffectedExecutor(affectedOptions,
                    changesProvider: new GitChangesProvider());

                var results = executor.Execute();

                foreach (var diagnostic in results.Diagnostics)
                {
                    if (diagnostic.Severity == AffectedDiagnosticSeverity.Warning)
                        Log.LogWarning(diagnostic.Message);
                    else
                        Log.LogMessage(MessageImportance.Low, diagnostic.Message);
                }

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

        /// <summary>
        /// MSBuild properties arrive as strings, so an unparseable one has to be refused here
        /// rather than silently falling back to the default and analysing something else.
        /// </summary>
        private UncommittedChanges ParseUncommitted()
        {
            if (string.IsNullOrWhiteSpace(Uncommitted))
                return UncommittedChanges.All;

            if (Enum.TryParse<UncommittedChanges>(Uncommitted, ignoreCase: true, out var parsed))
                return parsed;

            throw new ArgumentException(
                $"DotnetAffectedUncommitted was given '{Uncommitted}'. " +
                $"Expected one of: {string.Join(", ", Enum.GetNames(typeof(UncommittedChanges)))}.");
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
