using Affected.Cli.Formatters;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Affected.Cli.Tests.Formatters
{
    /// <summary>
    /// Resolving which solution gets referenced belongs to SolutionFilter,
    /// these cover what the formatter itself is responsible for.
    /// </summary>
    public class SolutionFilterFileFormatterTests
    {
        private static readonly string Root =
            Path.GetFullPath(Path.Combine(Path.GetTempPath(), "slnf-formatter-tests"));

        private static string PathIn(params string[] parts)
        {
            return Path.GetFullPath(Path.Combine(Root, Path.Combine(parts)));
        }

        private static string NormalizeNewLines(string value)
        {
            return value.Replace("\r\n", "\n");
        }

        [Fact]
        public async Task Should_reference_the_solution_and_the_affected_projects()
        {
            var formatter = new SolutionFilterFileOutputFormatter();
            var solutionPath = PathIn("Application.slnx");
            var outputPath = PathIn("affected.slnf");

            var projects = new[]
            {
                new ProjectInfo("StringUtils", PathIn("Libs", "StringUtils", "StringUtils.csproj")),
                new ProjectInfo("Web", PathIn("Apps", "Web", "Web.csproj"))
            };

            var output = await formatter.Format(projects, new OutputFormatterContext(outputPath, solutionPath));

            var expected = string.Join("\n",
                "{",
                "  \"solution\": {",
                "    \"path\": \"Application.slnx\",",
                "    \"projects\": [",
                "      \"Libs\\\\StringUtils\\\\StringUtils.csproj\",",
                "      \"Apps\\\\Web\\\\Web.csproj\"",
                "    ]",
                "  }",
                "}");

            Assert.Equal(expected, NormalizeNewLines(output));
        }

        [Fact]
        public async Task Should_reference_the_solution_relative_to_the_output_file()
        {
            // The realistic CI shape: the filter is generated into an output
            // directory, so it has to point back up at the solution.
            var formatter = new SolutionFilterFileOutputFormatter();
            var solutionPath = PathIn("Application.slnx");
            var outputPath = PathIn("artifacts", "affected.slnf");

            var projects = new[]
            {
                new ProjectInfo("StringUtils", PathIn("Libs", "StringUtils", "StringUtils.csproj"))
            };

            var output = await formatter.Format(projects, new OutputFormatterContext(outputPath, solutionPath));

            Assert.Contains("\"path\": \"..\\\\Application.slnx\"", output);
            // Projects stay relative to the solution, not to the output file.
            Assert.Contains("\"Libs\\\\StringUtils\\\\StringUtils.csproj\"", output);
        }
    }
}
