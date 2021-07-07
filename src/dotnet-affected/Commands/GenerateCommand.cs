using Affected.Cli.Views;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Graph;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Rendering.Views;
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

            this.Handler = CommandHandler.Create<IConsole, string, CommandExecutionData, ViewRenderingContext>(this.GenTraversalHandler);
        }

        private int GenTraversalHandler(
            IConsole console,
            string output,
            CommandExecutionData data,
            ViewRenderingContext renderingContext)
        {
            using var context = data.BuildExecutionContext();
            var affectedNodes = context.FindAffectedProjects().ToList();
            var rootView = new StackLayoutView();

            if (!affectedNodes.Any())
            {
                if (data.Verbose)
                {
                    rootView.Add(new NoChangesView());
                }

                renderingContext.Render(rootView);
                return AffectedExitCodes.NothingAffected;
            }

            var project = this.CreateTraversalProjectForTree(
                affectedNodes,
                context.NodesWithChanges);
            var projectXml = project.Xml.RawXml;

            if (data.Verbose)
            {
                var changesAndAffectedView = new WithChangesAndAffectedView(
                    context.NodesWithChanges,
                    affectedNodes
                );

                rootView.Add(changesAndAffectedView);

                rootView.Add(new ContentView("Generating Traversal SDK Project"));
                rootView.Add(new ContentView(string.Empty));
            }

            // If no output path, print the XML
            if (string.IsNullOrEmpty(output))
            {
                rootView.Add(new ContentView(projectXml));
            }
            else
            {
                // If we have an output path, we'll create a file with the contents.
                var filePath = this.WriteProjectFileToDisk(projectXml, output);

                rootView.Add(new ContentView($"Generated Project file at {filePath}"));
            }

            rootView.Add(new ContentView(string.Empty));
            renderingContext.Render(rootView);

            return 0;
        }

        private string WriteProjectFileToDisk(string xml, string outputPath)
        {
            // If it's a directory, append a file name to it
            if (Path.GetFileName(outputPath) is null)
            {
                outputPath = Path.Combine(outputPath, "dir.proj");
            }

            using var outputFile = new StreamWriter(outputPath);
            outputFile.Write(xml);
            return outputPath;
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
