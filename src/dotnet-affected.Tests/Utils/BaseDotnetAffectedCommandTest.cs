using Microsoft.Build.Construction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using System.CommandLine.Parsing;
using System.CommandLine.Rendering;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Affected.Cli.Tests
{
    public class BaseDotnetAffectedCommandTest
        : BaseMSBuildTest
    {
        protected ITestOutputHelper Helper { get; }

        protected ITerminal Terminal { get; } = new TestTerminal()
        {
            OutputMode = OutputMode.PlainText
        };

        protected Mock<IChangesProvider> ChangesProviderMock { get; } = new Mock<IChangesProvider>();

        protected BaseDotnetAffectedCommandTest(ITestOutputHelper helper)
        {
            Helper = helper;
        }

        protected async Task<(string Output, int ExitCode)> InvokeAsync(string args)
        {
            // Create the parser just as we do at Program.cs
            var parser = AffectedCli
                .CreateAffectedCommandLineBuilder()
                .ConfigureServices(services =>
                {
                    services.Replace(ServiceDescriptor.Singleton(ChangesProviderMock.Object));
                })
                .Build();

            // Execute against the testing infra
            var exitCode = await parser.InvokeAsync(args, Terminal);
            var output = Terminal.Out.ToString();

            // Log stuff for troubleshooting
            this.Helper.WriteLine(string.IsNullOrWhiteSpace(output)
                ? "WARNING: Command Produced No Output! (This is shown by testing infra only)"
                : output);

            // Log stderr
            var stderr = Terminal.Error.ToString();
            if (!string.IsNullOrWhiteSpace(stderr))
            {
                this.Helper.WriteLine(Terminal.Error.ToString());
            }

            // Return for assertions
            return (output, exitCode);
        }

        /// <summary>
        /// Creates an MSBuild <see cref="ProjectRootElement"/> inside a see
        /// <see cref="TempWorkingDirectory"/> and returns that.
        /// </summary>
        /// <param name="projectName">New project's name.</param>
        /// <returns>The directory where the project is deleted. Remember to dispose it.</returns>
        protected TempWorkingDirectory CreateSingleProject(string projectName)
        {
            var directory = new TempWorkingDirectory();
            var csprojPath = directory.MakePathForCsProj(projectName);

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

        protected void SetupChanges(string directory, params string[] output)
        {
            ChangesProviderMock.Setup(
                    cp => cp.GetChangedFiles(directory, It.IsAny<string>(), It.IsAny<string>()))
                .Returns(output);
        }
    }
}
