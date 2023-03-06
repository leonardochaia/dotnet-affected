using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Affected.Cli.Tests
{
    /// <summary>
    /// Tests for the public API.
    /// Ensures that the CLI implements common stuff like --help
    /// </summary>
    public class AffectedCliDefaultsTests : BaseInvocationTest
    {
        public AffectedCliDefaultsTests(ITestOutputHelper helper)
            : base(helper)
        {
        }

        [Fact]
        public async Task When_help_should_output_help()
        {
            var (output, exitCode) =
                await this.InvokeAsync("--help");

            Assert.Equal(0, exitCode);

            Assert.Contains("Determines which projects are affected by a set of changes.", output);
            Assert.Contains("Usage:", output);
            Assert.Contains("dotnet-affected [command] [options]", output);
        }
    }
}
