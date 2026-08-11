using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace DotnetAffected.Core.Tests
{
    public class SolutionFilterTests
    {
        private static readonly string Root =
            Path.GetFullPath(Path.Combine(Path.GetTempPath(), "solution-filter-tests"));

        private static string PathIn(params string[] parts)
        {
            return Path.GetFullPath(Path.Combine(Root, Path.Combine(parts)));
        }

        private static string NormalizeNewLines(string value)
        {
            return value.Replace("\r\n", "\n");
        }

        [Fact]
        public void Parse_should_resolve_paths_relative_to_their_owner()
        {
            // The solution is relative to the filter file, the projects to the solution.
            var filterFilePath = PathIn("filters", "affected.slnf");
            var json = @"{
  ""solution"": {
    ""path"": ""..\\Application.slnx"",
    ""projects"": [
      ""Libs\\StringUtils\\src\\StringUtils.csproj"",
      ""Apps\\Web\\Web.csproj""
    ]
  }
}";

            var filter = SolutionFilter.Parse(json, filterFilePath);

            Assert.Equal(PathIn("Application.slnx"), filter.SolutionPath);
            Assert.Equal(new[]
            {
                PathIn("Libs", "StringUtils", "src", "StringUtils.csproj"),
                PathIn("Apps", "Web", "Web.csproj")
            }, filter.ProjectPaths);
        }

        [Fact]
        public void Parse_should_honor_rooted_paths()
        {
            var filterFilePath = PathIn("filters", "affected.slnf");
            var solutionPath = PathIn("Application.slnx");
            var projectPath = PathIn("Libs", "StringUtils", "src", "StringUtils.csproj");

            var json = JsonSerializer.Serialize(new
            {
                solution = new
                {
                    path = solutionPath,
                    projects = new[]
                    {
                        projectPath
                    }
                }
            });

            var filter = SolutionFilter.Parse(json, filterFilePath);

            Assert.Equal(solutionPath, filter.SolutionPath);
            Assert.Equal(new[]
            {
                projectPath
            }, filter.ProjectPaths);
        }

        [Fact]
        public void Parse_should_allow_a_filter_without_projects()
        {
            var filter = SolutionFilter.Parse(
                @"{ ""solution"": { ""path"": ""Application.slnx"" } }",
                PathIn("affected.slnf"));

            Assert.Equal(PathIn("Application.slnx"), filter.SolutionPath);
            Assert.Empty(filter.ProjectPaths);
        }

        [Fact]
        public void Parse_should_ignore_unknown_properties()
        {
            var json = @"{
  ""version"": 1,
  ""solution"": {
    ""path"": ""Application.slnx"",
    ""projects"": [ ""Libs\\Lib.csproj"" ],
    ""somethingElse"": true
  }
}";

            var filter = SolutionFilter.Parse(json, PathIn("affected.slnf"));

            Assert.Equal(PathIn("Application.slnx"), filter.SolutionPath);
            Assert.Equal(new[]
            {
                PathIn("Libs", "Lib.csproj")
            }, filter.ProjectPaths);
        }

        [Theory]
        [InlineData("{}")]
        [InlineData("null")]
        [InlineData(@"{ ""solution"": null }")]
        [InlineData(@"{ ""solution"": { ""projects"": [] } }")]
        [InlineData(@"{ ""solution"": { ""path"": """" } }")]
        public void Parse_should_throw_when_the_solution_path_is_missing(string json)
        {
            var exception = Assert.Throws<InvalidOperationException>(
                () => SolutionFilter.Parse(json, PathIn("affected.slnf")));

            Assert.Contains("solution.path is missing", exception.Message);
        }

        [Fact]
        public void Parse_should_throw_when_the_contents_are_not_json()
        {
            var exception = Assert.Throws<InvalidOperationException>(
                () => SolutionFilter.Parse("this is not json", PathIn("affected.slnf")));

            Assert.IsType<JsonException>(exception.InnerException);
        }

        [Fact]
        public void ToJson_should_write_relative_paths_using_windows_separators()
        {
            // Pins the on disk format: MSBuild only translates Windows separators,
            // so a filter written on Linux must still be readable on Windows.
            var filterFilePath = PathIn("artifacts", "affected.slnf");
            var filter = new SolutionFilter(
                PathIn("Application.slnx"),
                new[]
                {
                    PathIn("Libs", "StringUtils", "src", "StringUtils.csproj"),
                    PathIn("Apps", "Web", "Web.csproj")
                });

            var expected = string.Join("\n",
                "{",
                "  \"solution\": {",
                "    \"path\": \"..\\\\Application.slnx\",",
                "    \"projects\": [",
                "      \"Libs\\\\StringUtils\\\\src\\\\StringUtils.csproj\",",
                "      \"Apps\\\\Web\\\\Web.csproj\"",
                "    ]",
                "  }",
                "}");

            Assert.Equal(expected, NormalizeNewLines(filter.ToJson(filterFilePath)));
        }

        [Fact]
        public void ToJson_and_Parse_should_round_trip()
        {
            var filterFilePath = PathIn("artifacts", "affected.slnf");
            var filter = new SolutionFilter(
                PathIn("src", "Application.slnx"),
                new[]
                {
                    PathIn("src", "Libs", "Lib.csproj"),
                    PathIn("test", "Lib.Tests.csproj")
                });

            var parsed = SolutionFilter.Parse(filter.ToJson(filterFilePath), filterFilePath);

            Assert.Equal(filter.SolutionPath, parsed.SolutionPath);
            Assert.Equal(filter.ProjectPaths, parsed.ProjectPaths);
        }

        [Fact]
        public async Task SaveAsync_and_Load_should_round_trip_on_disk()
        {
            var directory = Path.Combine(Path.GetTempPath(), $"dotnet-affected-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path.Combine(directory, "artifacts"));

            try
            {
                var filterFilePath = Path.Combine(directory, "artifacts", "affected.slnf");
                var solutionPath = Path.Combine(directory, "Application.slnx");
                var projectPath = Path.Combine(directory, "Libs", "Lib.csproj");

                await new SolutionFilter(solutionPath, new[]
                    {
                        projectPath
                    })
                    .SaveAsync(filterFilePath);

                var loaded = SolutionFilter.Load(filterFilePath);

                Assert.Equal(Path.GetFullPath(solutionPath), loaded.SolutionPath);
                Assert.Equal(new[]
                {
                    Path.GetFullPath(projectPath)
                }, loaded.ProjectPaths);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [Fact]
        public void Constructor_should_require_a_solution_path()
        {
            Assert.Throws<ArgumentException>(() => new SolutionFilter("", Array.Empty<string>()));
        }
    }
}
