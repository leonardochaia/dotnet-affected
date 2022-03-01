using System.CommandLine.Parsing;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Affected.Cli.Tests
{
    public abstract class BaseInvocationTest : BaseRepositoryTest
    {
        public ITestOutputHelper Helper { get; }

        public BaseInvocationTest(ITestOutputHelper helper)
        {
            Helper = helper;
        }

        protected async Task<(string Output, int ExitCode)> InvokeAsync(string args)
        {
            // Create the parser just as we do at Program.cs
            var parser = this.BuildAffectedCli().Build();

            // Execute against the testing infra
            this.Helper.WriteLine($"dotnet affected {args}");
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
    }
}
