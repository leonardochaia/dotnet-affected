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
using System.Threading.Tasks;
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
        }

        private class OutputOptions : Option<string>
        {
            public OutputOptions()
                : base(new[] {"--output", "-o"})
            {
                this.Description = "Location of the output file. Will output to stdout if not present";
            }
        }

        public class CommandHandler : ICommandHandler
        {
            private readonly CommandExecutionData _data;
            private readonly ICommandExecutionContext _context;
            private readonly ViewRenderingContext _renderingContext;

            public string? Output { get; set; }

            public CommandHandler(
                CommandExecutionData data,
                ICommandExecutionContext context,
                ViewRenderingContext renderingContext)
            {
                _data = data;
                _context = context;
                _renderingContext = renderingContext;
            }

            public Task<int> InvokeAsync(InvocationContext ic)
            {
                var affectedNodes = _context.FindAffectedProjects().ToList();
                var rootView = new StackLayoutView();

                if (!affectedNodes.Any())
                {
                    if (_data.Verbose)
                    {
                        rootView.Add(new NoChangesView());
                    }

                    _renderingContext.Render(rootView);
                    return Task.FromResult(AffectedExitCodes.NothingAffected);
                }

                var project = this.CreateTraversalProjectForTree(
                    affectedNodes,
                    _context.NodesWithChanges);
                var projectXml = project.Xml.RawXml;

                if (_data.Verbose)
                {
                    var changesAndAffectedView = new WithChangesAndAffectedView(
                        _context.NodesWithChanges,
                        affectedNodes
                    );

                    rootView.Add(changesAndAffectedView);

                    rootView.Add(new ContentView("Generating Traversal SDK Project"));
                    rootView.Add(new ContentView(string.Empty));
                }

                // If no output path, print the XML
                if (string.IsNullOrWhiteSpace(Output))
                {
                    rootView.Add(new ContentView(projectXml));
                }
                else
                {
                    // If we have an output path, we'll create a file with the contents.
                    var filePath = this.WriteProjectFileToDisk(projectXml, Output);

                    rootView.Add(new ContentView($"Generated Project file at {filePath}"));
                }

                rootView.Add(new ContentView(string.Empty));
                _renderingContext.Render(rootView);

                return Task.FromResult(0);
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
        }
    }
}
