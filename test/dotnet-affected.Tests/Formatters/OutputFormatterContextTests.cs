using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Affected.Cli.Tests.Formatters
{
    /// <summary>
    /// Ensures the executor hands formatters everything they need to write
    /// paths relative to the file being created.
    /// </summary>
    public class OutputFormatterContextTests
        : BaseInvocationTest
    {
        public OutputFormatterContextTests(ITestOutputHelper helper)
            : base(helper)
        {
        }

        private class CapturingOutputFormatter : IOutputFormatter
        {
            public OutputFormatterContext Captured { get; private set; }

            public string Type => "capturing";

            public string NewFileExtension => ".captured";

            public Task<string> Format(IEnumerable<IProjectInfo> projects, OutputFormatterContext context)
            {
                Captured = context;
                return Task.FromResult("captured");
            }
        }

        private async Task<OutputFormatterContext> CaptureContext(string filterFilePath)
        {
            var formatter = new CapturingOutputFormatter();
            var executor = new OutputFormatterExecutor(new[]
            {
                formatter
            }, this.Terminal);

            await executor.Execute(new[]
                {
                    new ProjectInfo("TestProject", "/home/dev/test/test.csproj")
                },
                new[]
                {
                    "capturing"
                },
                Repository.Path,
                "affected",
                filterFilePath,
                dryRun: true,
                verbose: true);

            Assert.NotNull(formatter.Captured);
            return formatter.Captured;
        }

        [Fact]
        public async Task Should_receive_the_path_of_the_file_being_written()
        {
            var context = await CaptureContext(null);

            // Includes the formatter's own extension, since that is the file
            // formatters have to make their paths relative to.
            Assert.Equal(Path.Combine(Repository.Path, "affected.captured"), context.OutputPath);
        }

        [Fact]
        public async Task Should_receive_the_filter_file_path()
        {
            var filterFilePath = Path.Combine(Repository.Path, "test-solution.slnx");

            var context = await CaptureContext(filterFilePath);

            Assert.Equal(filterFilePath, context.FilterFilePath);
        }

        [Fact]
        public async Task Should_receive_no_filter_file_path_when_discovering_from_disk()
        {
            var context = await CaptureContext(null);

            Assert.Null(context.FilterFilePath);
        }
    }
}
