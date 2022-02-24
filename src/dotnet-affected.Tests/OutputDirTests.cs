using Affected.Cli.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Affected.Cli.Tests
{
    public class OutputDirTests
    {
        private class TestPathsClassData : IEnumerable<object[]>
        {
            private static readonly char S = System.IO.Path.DirectorySeparatorChar;

            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[]
                {
                    $"{S}home{S}lchaia{S}dotnet-affected", "", $"{S}home{S}lchaia{S}dotnet-affected"
                };
                yield return new object[]
                {
                    $"{S}home{S}lchaia{S}dotnet-affected", $"relative{S}path",
                    $"{S}home{S}lchaia{S}dotnet-affected{S}relative{S}path"
                };
                yield return new object[]
                {
                    $"{S}home{S}lchaia{S}dotnet-affected", $"{S}some{S}absolute{S}path", $"{S}some{S}absolute{S}path"
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
