using Microsoft.Build.Construction;
using System.CommandLine.Parsing;
using System.CommandLine.Rendering;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Affected.Cli.Tests
{
    public class BaseDotnetAffectedCommandTest
        : BaseMSBuildTest
    {
        private readonly ITestOutputHelper _helper;

        private readonly ITerminal _terminal = new TestTerminal()
        {
            OutputMode = OutputMode.PlainText
        };

        protected BaseDotnetAffectedCommandTest(ITestOutputHelper helper)
        {
            _helper = helper;
        }

        protected async Task<(string Output, int ExitCode)> InvokeAsync(string args)
        {
            // Create the parser just as we do at Program.cs
            var parser = CommandLineBuilderUtils
                .CreateCommandLineBuilder()
                .Build();

            // Execute against the testing infra
            var exitCode = await parser.InvokeAsync(args, _terminal);
            var output = _terminal.Out.ToString();

            // Log stuff for troubleshooting
            this._helper.WriteLine(output);
            this._helper.WriteLine(_terminal.Error.ToString());

            // Return for assertions
            return (output, exitCode);
        }

        /// <summary>
        /// Creates an MSBuild <see cref="ProjectRootElement"/> inside a see
        /// <see cref="FileUtilities.TempWorkingDirectory"/> and returns that.
        /// </summary>
        /// <param name="projectName">New project's name.</param>
        /// <returns>The directory where the project is deleted. Remember to dispose it.</returns>
        protected FileUtilities.TempWorkingDirectory CreateSingleProject(string projectName)
        {
            var directory = new FileUtilities.TempWorkingDirectory();
            var csprojPath = directory.GetTemporaryCsProjFile();

            CreateProject(csprojPath, projectName)
                .Save();

            return directory;
        }

        /// <summary>
        /// Creates an MSBuild <see cref="ProjectRootElement"/> at the provided
        /// <paramref name="csprojPath"/> with the corresponding <paramref name="projectName"/>.
        /// </summary>
        /// <param name="csprojPath">Path for new csproj.</param>
        /// <param name="projectName">Project name.</param>
        /// <returns></returns>
        protected ProjectRootElement CreateProject(string csprojPath, string projectName)
        {
            return ProjectRootElement
                .Create(csprojPath)
                .SetName(projectName);
        }
    }
}
