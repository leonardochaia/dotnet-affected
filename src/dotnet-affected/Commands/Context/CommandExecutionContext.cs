using LibGit2Sharp;
using Microsoft.Build.Graph;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.IO;
using System.IO;
using System.Linq;
using Microsoft.Build.Construction;

namespace Affected.Cli.Commands
{
    internal class CommandExecutionContext : IDisposable
    {
        private readonly CommandExecutionData executionData;
        private readonly IConsole console;
        private readonly Repository repository;

        public CommandExecutionContext(
            CommandExecutionData executionData,
            IConsole console)
        {
            this.executionData = executionData;
            this.console = console;
            this.repository = new Repository(this.executionData.RepositoryPath);
            this.Graph = this.BuildProjectGraph();

            if (this.executionData.AssumeChanges?.Any() == true)
            {
                this.WriteLine($"Assuming hypothetical project changes, won't use Git diff");

                this.NodesWithChanges = this.Graph
                    .FindNodesByName(this.executionData.AssumeChanges);
            }
            else
            {
                this.NodesWithChanges = this.FindNodesThatChangedUsingGit();
            }

            this.WriteLine();
        }

        public ProjectGraph Graph { get; private set; }

        public IEnumerable<ProjectGraphNode> NodesWithChanges { get; private set; }

        public IEnumerable<ProjectGraphNode> FindAffectedProjects()
        {
            return this.Graph.FindNodesThatDependOn(this.NodesWithChanges);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            this.repository?.Dispose();
        }

        /// <summary>
        /// Builds a <see cref="ProjectGraph"/> from all found project files
        /// inside the <see cref="CommandExecutionData.SolutionPath"/> or <see cref="CommandExecutionData.RepositoryPath"/>.
        /// </summary>
        /// <returns>A new Project Graph.</returns>
        private ProjectGraph BuildProjectGraph()
        {
            // Find all csproj and build the dependency tree
            var allProjects = !string.IsNullOrWhiteSpace(this.executionData.SolutionPath) 
                ? FindProjectsInSolution() 
                : FindProjectsInDirectory();

            this.WriteLine($"Building Dependency Graph");

            var graph = new ProjectGraph(allProjects);

            this.WriteLine($"Found {graph.ConstructionMetrics.NodeCount} projects");

            return graph;
        }

        private IEnumerable<string> FindProjectsInSolution()
        {
            this.WriteLine($"Finding all projects from Solution {this.executionData.SolutionPath}");

            var solution = SolutionFile.Parse(this.executionData.SolutionPath);

            return solution.ProjectsInOrder
                .Where(x => x.ProjectType != SolutionProjectType.SolutionFolder)
                .Select(x => x.AbsolutePath);
        }

        private IEnumerable<string> FindProjectsInDirectory()
        {
            this.WriteLine($"Finding all csproj at {this.executionData.RepositoryPath}");

            // TODO: Find *.*proj ?
            return Directory.GetFiles(
                this.executionData.RepositoryPath,
                "*.csproj",
                SearchOption.AllDirectories);
        }

        private IEnumerable<ProjectGraphNode> FindNodesThatChangedUsingGit()
        {
            // Determine the git diff strategy
            IEnumerable<string> filesWithChanges;
            var to = GitUtils.GetCommitOrHead(this.repository, this.executionData.To);

            if (this.executionData.From == null)
            {
                this.WriteLine($"Finding changes from working directory against {to}");

                filesWithChanges = GitUtils.GetChangesAgainstWorkingDirectory(
                    this.repository,
                    to.Tree);
            }
            else
            {
                var from = GitUtils.GetCommitOrThrow(this.repository, this.executionData.From);
                this.WriteLine($"Finding changes from {from} against {to}");

                filesWithChanges = GitUtils.GetChangesBetweenTrees(
                    this.repository,
                    from.Tree,
                    to.Tree);
            }

            var nodesWithChanges = this.Graph
                .FindNodesContainingFiles(filesWithChanges)
                .ToList();

            this.WriteLine($"Found {filesWithChanges.Count()} changed files" +
                $" inside {nodesWithChanges.Count} projects.");

            return nodesWithChanges;
        }

        private void WriteLine(string? message = null)
        {
            if (this.executionData.Verbose)
            {
                if (message == null)
                {
                    this.console.Out.WriteLine();
                }
                else
                {
                    this.console.Out.WriteLine(message);
                }
            }
        }
    }
}
