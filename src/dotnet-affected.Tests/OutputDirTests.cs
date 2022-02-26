using Affected.Cli.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace Affected.Cli.Tests
{
    public class OutputDirTests
    {
        private class TestPathsClassData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[]
                {
                    "/home/lchaia/dotnet-affected", "", "/home/lchaia/dotnet-affected"
                };
                yield return new object[]
                {
                    "/home/lchaia/dotnet-affected", "relative/path",
                    Path.Combine("/home/lchaia/dotnet-affected", "relative/path")
                };
                yield return new object[]
                {
                    "/home/lchaia/dotnet-affected", "/some/absolute/path", "/some/absolute/path"
                };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [Theory]
        [ClassData(typeof(TestPathsClassData))]
        public void Should_determine_output_dir_correctly(
            string repositoryPath,
            string outputDir,
            string expected)
        {
            var data = new CommandExecutionData(
                repositoryPath,
                String.Empty,
                string.Empty,
                string.Empty,
                false,
                Enumerable.Empty<string>(),
                new string[0],
                false,
                outputDir,
                string.Empty);

            Assert.Equal(expected, data.OutputDir);
        }
    }
}
