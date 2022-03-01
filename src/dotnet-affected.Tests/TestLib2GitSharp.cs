using LibGit2Sharp;
using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace Affected.Cli.Tests
{
    public class TestLib2GitSharp
    {
        private readonly ITestOutputHelper _helper;

        public TestLib2GitSharp(ITestOutputHelper helper)
        {
            _helper = helper;
        }

        [Fact]
        public void TestRepoPaths()
        {
            _helper.WriteLine($"OS: {Environment.OSVersion}");
            using var directory = new TempWorkingDirectory();

            _helper.WriteLine($"Request create repo at {directory.Path}");

            var createdRepoPath = Repository.Init(directory.Path);
            _helper.WriteLine($"Output Git Init {createdRepoPath}");

            using var repo = new Repository(directory.Path);

            _helper.WriteLine($"Repo Path: {repo.Info.Path}");
            _helper.WriteLine($"Repo Workdir: {repo.Info.WorkingDirectory}");

            var workDir = Path.TrimEndingDirectorySeparator(repo.Info.WorkingDirectory);
            Assert.Equal(directory.Path, workDir);
        }
    }
}
