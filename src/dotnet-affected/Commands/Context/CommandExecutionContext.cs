﻿using Microsoft.Build.Graph;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Affected.Cli.Commands
{
    /// <summary>
    /// Calculates contextual information used across commands
    /// </summary>
    internal class CommandExecutionContext : ICommandExecutionContext
    {
        private readonly CommandExecutionData _data;
        private readonly Lazy<IEnumerable<string>> _changedFiles;
        private readonly Lazy<IEnumerable<ProjectGraphNode>> _changedProjects;
        private readonly Lazy<IEnumerable<ProjectGraphNode>> _affectedProjects;
        private readonly Lazy<IEnumerable<PackageChange>> _changedNugetPackages;
        private readonly Lazy<AffectedSummary> _summary;

        public CommandExecutionContext(
            CommandExecutionData data)
        {
            _data = data;
            // Discovering projects, and finding affected may throw
            // For error handling to be managed properly at the handler level,
            // we use Lazies so that its done on demand when its actually needed
            // instead of happening here on the constructor
            _summary = new Lazy<AffectedSummary>(() =>
            {
                var executor = BuildAffectedExecutor();

                return executor.Execute();
            });

            _changedFiles = new Lazy<IEnumerable<string>>(() => _summary.Value.FilesThatChanged);

            _changedNugetPackages = new Lazy<IEnumerable<PackageChange>>(() => _summary.Value.ChangedPackages);

            _changedProjects = new Lazy<IEnumerable<ProjectGraphNode>>(() =>
            {
                ThrowIfNoChanges();
                return _summary.Value.ProjectsWithChangedFiles;
            });

            _affectedProjects = new Lazy<IEnumerable<ProjectGraphNode>>(() =>
            {
                ThrowIfNoChanges();
                return _summary.Value.AffectedProjects;
            });
        }

        public AffectedExecutor BuildAffectedExecutor()
        {
            var options = _data.ToAffectedOptions();
            var graph = new ProjectGraphRef(options).Value;
            IChangesProvider changesProvider = _data.AssumeChanges?.Any() == true
                ? new AssumptionChangesProvider(graph, _data.AssumeChanges)
                : new GitChangesProvider();
            var executor = new AffectedExecutor(options,
                changesProvider,
                graph,
                new ChangedProjectsProvider(graph, options));
            return executor;
        }

        public IEnumerable<string> ChangedFiles => _changedFiles.Value;

        public IEnumerable<PackageChange> ChangedNuGetPackages => _changedNugetPackages.Value;

        public IEnumerable<IProjectInfo> ChangedProjects => _changedProjects.Value
            .Select(p => new ProjectInfo(p))
            .OrderBy(x => x.Name)
            .ToArray();

        public IEnumerable<IProjectInfo> AffectedProjects => _affectedProjects.Value
            .Select(p => new ProjectInfo(p))
            .OrderBy(x => x.Name)
            .ToArray();

        private void ThrowIfNoChanges()
        {
            if (!this._summary.Value.ProjectsWithChangedFiles.Any()
                && !this._summary.Value.AffectedProjects.Any())
            {
                throw new NoChangesException();
            }
        }
    }
}
