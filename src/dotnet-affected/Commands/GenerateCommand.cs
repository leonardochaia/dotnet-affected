using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Graph;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.IO;
using System.Linq;
using System.Xml;

namespace Affected.Cli.Commands
{
    internal class GenerateCommand : Command
    {
        public GenerateCommand()
            : base("generate")
        {
            this.Description = "Generates a Microsoft.Sdk.Traversal project which includes" +
                " all affected projects as build targets.";

            this.AddOption(new OutputOptions());

            this.Handler = CommandHandler.Create<IConsole, string, CommandExecutionData>(this.GenTraversalHandler);
        }

        private void GenTraversalHandler(
            IConsole console,
            string output,
            CommandExecutionData data)
        {
            using var context = data.BuildExecutionContext();

            if (data.Verbose)
            {
                console.Out.WriteLine("Files inside these projects have changed:");
                foreach (var node in context.NodesWithChanges)
                {
                    console.Out.WriteLine($"\t{node.GetProjectName()}");
                }

                console.Out.WriteLine();
            }

            var affectedProjects = context.FindAffectedProjects();

            if (data.Verbose)
            {
                console.Out.WriteLine("These projects are affected by those changes:");
                foreach (var affected in affectedProjects)
                {
                    console.Out.WriteLine($"\t{affected.GetProjectName()}");
                }

                console.Out.WriteLine();
            }

            if (data.Verbose)
            {
                console.Out.WriteLine("Generating Traversal SDK Project");
                console.Out.WriteLine();
            }

            var project = this.CreateTraversalProjectForTree(
                affectedProjects,
                context.NodesWithChanges);

            this.WriteOutput(console, project.Xml.RawXml, output);
        }

        private void WriteOutput(IConsole console, string xml, string? outputPath)
        {
            if (outputPath == null)
            {
                // if no output, write to stdout
                console.Out.WriteLine(xml);
            }
            else
            {
                // If we have an output path, we'll create a file with the contents.
                // If it's a directory, append a file name to it
                if (Path.GetFileName(outputPath) == null)
                {
                    outputPath = Path.Combine(outputPath, "dir.proj");
                }

                console.Out.WriteLine($"Creating file at {outputPath}");

                using StreamWriter outputFile = new StreamWriter(outputPath);
                outputFile.Write(xml);
            }
        }

        private Project CreateTraversalProjectForTree(
            IEnumerable<ProjectGraphNode> affectedNodes,
            IEnumerable<ProjectGraphNode> nodesWithChanges)
        {
            var projectRootElement = @"<Project Sdk=""Microsoft.Build.Traversal/3.0.3""></Project>";
            var stringReader = new StringReader(projectRootElement);
            var xmlReader = new XmlTextReader(stringReader);
            var root = ProjectRootElement.Create(xmlReader);

            void AddProjectReference(Project project, ProjectGraphNode node)
            {
                var path = node.ProjectInstance.FullPath;
                if (!project.Items.Any(i => i.EvaluatedInclude == path))
                {
                    project.AddItem("ProjectReference", path);
                }
            }

            var project = new Project(root);

            // Find all affected and add them as project references
            foreach (var node in affectedNodes)
            {
                AddProjectReference(project, node);
            }

            foreach (var node in nodesWithChanges)
            {
                AddProjectReference(project, node);
            }

            return project;
        }

        private class OutputOptions : Option<string>
        {
            public OutputOptions()
                : base(new[] { "--output", "-o" })
            {
                this.Description = "Location of the output file. Will output to stdout if not present";
            }
        }
    }
}
