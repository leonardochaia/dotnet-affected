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
    internal class CommandExecutionContext : ICommandExecutionContext
    {
        private readonly CommandExecutionData executionData;
        private readonly IConsole console;

        private readonly Lazy<IEnumerable<ProjectGraphNode>> nodesWithChanges;

        private readonly Lazy<ProjectGraph> graph;

        public CommandExecutionContext(
            CommandExecutionData executionData,
            IConsole console)
        {
            this.executionData = executionData;
            this.console = console;

            // Discovering projects, and finding affected may throw
            // so that error handling is managed properly at the handler level,
            // we use Lazy so that its done on demand when its actually needed.
            this.graph = new Lazy<ProjectGraph>(this.BuildProjectGraph);

            this.nodesWithChanges = new Lazy<IEnumerable<ProjectGraphNode>>(() =>
            {
                if (this.executionData.AssumeChanges?.Any() != true)
                {
                    return this.FindNodesThatChangedUsingGit();
                }

                this.WriteLine($"Assuming hypothetical project changes, won't use Git diff");

                return this.graph.Value
                    .FindNodesByName(this.executionData.AssumeChanges);
            });
        }

        public IEnumerable<ProjectGraphNode> NodesWithChanges => this.nodesWithChanges.Value;

        public IEnumerable<ProjectGraphNode> FindAffectedProjects()
        {
            return this.graph.Value.FindNodesThatDependOn(this.NodesWithChanges);
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

            var output = new ProjectGraph(allProjects);

            this.WriteLine($"Found {output.ConstructionMetrics.NodeCount} projects");

            return output;
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
            using var repository = new Repository(this.executionData.RepositoryPath);

            // Determine the git diff strategy
            IEnumerable<string> filesWithChanges;
            var to = GitUtils.GetCommitOrHead(repository, this.executionData.To);

            if (string.IsNullOrWhiteSpace(this.executionData.From))
            {
                this.WriteLine($"Finding changes from working directory against {to}");

                filesWithChanges = GitUtils.GetChangesAgainstWorkingDirectory(
                    repository,
                    to.Tree);
            }
            else
            {
                var from = GitUtils.GetCommitOrThrow(repository, this.executionData.From);
                this.WriteLine($"Finding changes from {from} against {to}");

                filesWithChanges = GitUtils.GetChangesBetweenTrees(
                    repository,
                    from.Tree,
                    to.Tree);
            }

            var output = this.graph.Value
                .FindNodesContainingFiles(filesWithChanges)
                .ToList();

            this.WriteLine($"Found {filesWithChanges.Count()} changed files" +
                $" inside {output.Count} projects.");

            return output;
        }

        private void WriteLine(string? message = null)
        {
            if (!this.executionData.Verbose)
            {
                return;
            }

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
