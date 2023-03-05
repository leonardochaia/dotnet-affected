using DotnetAffected.Abstractions;
using DotnetAffected.Core.FileSystem;
using LibGit2Sharp;
using Microsoft.Build.Construction;
using Microsoft.Build.Definition;
using Microsoft.Build.Evaluation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

namespace DotnetAffected.Core
{
    /// <summary>
    /// Detects changes using Git.
    /// </summary>
    public class GitChangesProvider : IChangesProvider
    {
        internal static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        private static readonly Lazy<bool> ResolveMsBuildFileSystemSupported = new Lazy<bool>(() =>
        {
            var versionInfo = FileVersionInfo.GetVersionInfo(typeof(Project).Assembly.Location);
            if (versionInfo.FileMajorPart > 16)
                return true;
            if (versionInfo.FileMajorPart < 16)
                return false;
            return versionInfo.FileMinorPart >= 10;
        });

        /// <summary>
        /// When true, the build system supports virtual filesystem which means nested Directory.Packages.props files
        /// are supported in central package management.
        /// </summary>
        public static bool MsBuildFileSystemSupported => ResolveMsBuildFileSystemSupported.Value;

        /// <inheritdoc />
        public IEnumerable<string> GetChangedFiles(string directory, string from, string to)
        {
            using var repository = new Repository(directory);

            var changes = GetChangesForRange<TreeChanges>(repository, from, to);

            return TreeChangesToPaths(changes, directory);
        }

        /// <inheritdoc />
        public Project? LoadDirectoryPackagePropsProject(string directory, string pathToFile, string? commitRef,
            bool fallbackToHead)
        {
            var project = LoadProject(directory, pathToFile, commitRef, fallbackToHead);
            if (project is null && MsBuildFileSystemSupported)
            {
                var fi = new FileInfo(pathToFile);
                var parent = fi.Directory?.Parent?.FullName;
                if (parent is not null && parent.Length >= directory.Length)
                    return LoadDirectoryPackagePropsProject(directory, Path.Combine(parent, "Directory.Packages.props"),
                        commitRef, fallbackToHead);
            }

            return project;
        }

        /// <inheritdoc />
        public Project? LoadProject(string directory, string pathToFile, string? commitRef, bool fallbackToHead)
        {
            return MsBuildFileSystemSupported
                ? LoadProjectCore(directory, pathToFile, commitRef, fallbackToHead)
                : LoadProjectLegacy(directory, pathToFile, commitRef, fallbackToHead);
        }

        private Project? LoadProjectCore(string directory, string pathToFile, string? commitRef, bool fallbackToHead)
        {
            Commit? commit;

            using var repository = new Repository(directory);

            if (string.IsNullOrWhiteSpace(commitRef))
                commit = fallbackToHead ? repository.Head.Tip : null;
            else
                commit = GetCommitOrThrow(repository, commitRef);

            /* TODO: Uncomment if/when https://github.com/dotnet/msbuild/issues/7956 is fixed. */
            // using var projectFactory = new ProjectFactory(new MsBuildGitFileSystem(repository, commit), new ProjectCollection());
            // return projectFactory.FileSystem.FileExists(pathToFile)
            //     ? projectFactory.CreateProject(pathToFile)
            //     : null;

            /* Workaround for https://github.com/dotnet/msbuild/issues/7956
               For more information, see comments in EagerCachingMsBuildGitFileSystem
               TODO: Delete EagerCachingMsBuildGitFileSystem and this code if/when 7956 is fixed. */
            using var fs = new EagerCachingMsBuildGitFileSystem(repository, commit);
            return fs.FileExists(pathToFile) ? fs.CreateProjectAndEagerLoadChildren(pathToFile) : null;
        }

        private Project? LoadProjectLegacy(string directory, string pathToFile, string? commitRef, bool fallbackToHead)
        {
            Commit? commit;

            using var repository = new Repository(directory);

            if (string.IsNullOrWhiteSpace(commitRef))
                commit = fallbackToHead ? repository.Head.Tip : null;
            else
                commit = GetCommitOrThrow(repository, commitRef);

            Stream GenerateStreamFromString(string s)
            {
                var stream = new MemoryStream();
                var writer = new StreamWriter(stream);
                writer.Write(s);
                writer.Flush();
                stream.Position = 0;
                return stream;
            }

            var projectCollection = new ProjectCollection();

            if (commit is null)
            {
                var path = Path.Combine(directory, pathToFile);
                if (!File.Exists(path)) return null;

                using var reader = new XmlTextReader(GenerateStreamFromString(File.ReadAllText(path)));
                var projectRootElement = ProjectRootElement.Create(reader);
                projectRootElement.FullPath = pathToFile;
                return Project.FromProjectRootElement(projectRootElement, new ProjectOptions
                {
                    LoadSettings = ProjectLoadSettings.Default, ProjectCollection = projectCollection,
                });
            }
            else
            {
                var path = IsWindows
                    ? Path.GetRelativePath(directory, pathToFile)
                        .Replace('\\', '/')
                    : Path.GetRelativePath(directory, pathToFile);
                var treeEntry = commit[path];
                if (treeEntry == null) return null;

                var blob = (Blob)treeEntry.Target;

                using var content = new StreamReader(blob.GetContentStream(), Encoding.UTF8);
                using var reader = new XmlTextReader(GenerateStreamFromString(content.ReadToEnd()));
                var projectRootElement = ProjectRootElement.Create(reader);
                projectRootElement.FullPath = pathToFile;
                return Project.FromProjectRootElement(projectRootElement, new ProjectOptions
                {
                    LoadSettings = ProjectLoadSettings.Default, ProjectCollection = projectCollection,
                });
            }
        }

        private static (Commit? From, Commit To) ParseRevisionRanges(
            Repository repository,
            string from,
            string to)
        {
            // Find the To Commit or use HEAD.
            var toCommit = GetCommitOrHead(repository, to);

            // No from: compare against working directory
            if (string.IsNullOrWhiteSpace(from))
            {
                // this.WriteLine($"Finding changes from working directory against {to}");
                return (null, toCommit);
            }

            var fromCommit = GetCommitOrThrow(repository, @from);
            return (fromCommit, toCommit);
        }

        private static T GetChangesForRange<T>(
            Repository repository,
            string from,
            string to)
            where T : class, IDiffResult
        {
            var (fromCommit, toCommit) = ParseRevisionRanges(repository, from, to);

            return fromCommit is null
                ? GetChangesAgainstWorkingDirectory<T>(repository, toCommit.Tree)
                : GetChangesBetweenTrees<T>(repository, fromCommit.Tree, toCommit.Tree);
        }

        private static T GetChangesAgainstWorkingDirectory<T>(
            Repository repository,
            Tree tree,
            IEnumerable<string>? files = null)
            where T : class, IDiffResult
        {
            return repository.Diff.Compare<T>(
                tree,
                DiffTargets.Index | DiffTargets.WorkingDirectory,
                files);
        }

        private static T GetChangesBetweenTrees<T>(
            Repository repository,
            Tree fromTree,
            Tree toTree,
            IEnumerable<string>? files = null)
            where T : class, IDiffResult
        {
            return repository.Diff.Compare<T>(
                fromTree,
                toTree,
                files);
        }

        private static Commit GetCommitOrHead(Repository repository, string name)
        {
            return string.IsNullOrWhiteSpace(name) ? repository.Head.Tip : GetCommitOrThrow(repository, name);
        }

        private static Commit GetCommitOrThrow(Repository repo, string name)
        {
            var commit = repo.Lookup<Commit>(name);
            if (commit != null)
            {
                return commit;
            }

            var branch = repo.Branches[name];
            if (branch != null)
            {
                return branch.Tip;
            }

            throw new InvalidOperationException(
                $"Couldn't find Git Commit or Branch with name {name} in repository {repo.Info.Path}");
        }

        private static IEnumerable<string> TreeChangesToPaths(
            TreeChanges changes,
            string repositoryRootPath)
        {
            foreach (var change in changes)
            {
                if (change == null) continue;

                var currentPath = Path.Combine(repositoryRootPath, change.Path);

                yield return currentPath;
            }
        }
    }
}
