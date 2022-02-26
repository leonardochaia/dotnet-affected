using Affected.Cli.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace Affected.Cli.Tests
{
    public class RepositoryPathTests
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
                    "", "/home/lchaia/dotnet-affected/Affected.sln", Path.GetDirectoryName("/home/lchaia/dotnet-affected/Affected.sln")
                };
                yield return new object[]
                {
                    "/home/lchaia/dotnet-affected", "/home/lchaia/dotnet-affected/subdirectory/other/Affected.sln",
                    "/home/lchaia/dotnet-affected"
                };
                yield return new object[]
                {
                    "", "", Environment.CurrentDirectory
                };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [Theory]
        [ClassData(typeof(TestPathsClassData))]
        public void Should_determine_repository_path_correctly(
            string repositoryPath,
            string solutionPath,
            string expected)
        {
            var data = new CommandExecutionData(
                repositoryPath,
                solutionPath,
                string.Empty,
                string.Empty,
                false,
                Enumerable.Empty<string>(),
                new string[0],
                false,
                string.Empty,
                string.Empty);

            Assert.Equal(expected, data.RepositoryPath);
        }
    }
}
