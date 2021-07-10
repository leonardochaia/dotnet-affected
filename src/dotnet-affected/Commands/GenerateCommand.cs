using Affected.Cli.Views;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Graph;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Rendering;
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
                : base(new[]
                {
                    "--output", "-o"
                })
            {
                this.Description = "Location of the output file. Will output to stdout if not present";
            }
        }

        public class CommandHandler : ICommandHandler
        {
            private readonly CommandExecutionData _data;
            private readonly ICommandExecutionContext _context;
            private readonly IConsole _console;

            public string? Output { get; set; }

            public CommandHandler(
                CommandExecutionData data,
                ICommandExecutionContext context,
                IConsole console)
            {
                _data = data;
                _context = context;
                _console = console;
            }

            public Task<int> InvokeAsync(InvocationContext ic)
            {
                if (!_context.ChangedProjects.Any())
                {
                    throw new NoChangesException();
                }

                var affectedNodes = _context.AffectedProjects.ToList();
                var project = this.CreateTraversalProjectForTree(
                    affectedNodes,
                    _context.ChangedProjects);
                var projectXml = project.Xml.RawXml;

                if (_data.Verbose)
                {
                    var changesAndAffectedView = new WithChangesAndAffectedView(
                        _context.ChangedProjects,
                        affectedNodes
                    );

                    _console.Append(changesAndAffectedView);

                    _console.Out.WriteLine("Generating Traversal SDK Project");
                }

                // If no output path, print the XML
                if (string.IsNullOrWhiteSpace(Output))
                {
                    _console.Out.Write(projectXml);
                }
                else
                {
                    // If we have an output path, we'll save the project to disk.
                    project.Save(Output);

                    _console.Out.WriteLine($"Generated Project file at {Output}");
                }

                return Task.FromResult(0);
            }

            private Project CreateTraversalProjectForTree(
                IEnumerable<ProjectGraphNode> affectedNodes,
                IEnumerable<ProjectGraphNode> nodesWithChanges)
            {
                var projectRootElement = @"<Project Sdk=""Microsoft.Build.Traversal/3.0.3""></Project>";
                var stringReader = new StringReader(projectRootElement);
                var xmlReader = new XmlTextReader(stringReader);
                var root = ProjectRootElement.Create(xmlReader);

                var project = new Project(root);

                void AddProjectReference(ProjectGraphNode current)
                {
                    var currentProjectPath = current.ProjectInstance.FullPath;

                    // Ignore the current project
                    if (project.Items.All(i => i.EvaluatedInclude != currentProjectPath))
                    {
                        project.AddItem("ProjectReference", currentProjectPath);
                    }
                }

                // Find all affected and add them as project references
                foreach (var node in affectedNodes)
                {
                    AddProjectReference(node);
                }

                // We also want to build everything that changed.
                foreach (var node in nodesWithChanges)
                {
                    AddProjectReference(node);
                }

                return project;
            }
        }
    }
}
