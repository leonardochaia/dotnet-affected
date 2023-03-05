using LibGit2Sharp;
using Microsoft.Build.Evaluation;
using Microsoft.Build.FileSystem;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Enumeration;
using System.Text;

namespace DotnetAffected.Core.FileSystem
{
    /// <summary>
    /// An git <see cref="MSBuildFileSystemBase">MSBuild file system</see> implementation for project evaluation. <para/>
    /// It is a <b>readonly</b> filesystem which exposes the filesystem in the state of the commit provided to it. <para/>
    ///
    /// <see cref="MsBuildGitFileSystem"/> is required for project imports references, e.g:
    ///
    /// <code>&lt;Import Project="path" /&gt;</code>
    /// 
    /// Where the <see cref="Project"/> build instance will dynamically load the referenced project at <b>path</b> which requires a file system
    /// to load it from, in a git commit we need a virtual file system.
    /// </summary>
    /// <remarks>When commit is null the file system represents the current working directory.</remarks>
    internal class MsBuildGitFileSystem : MSBuildFileSystemBase
    {
        private readonly Repository _repository;
        private readonly Commit? _commit;

        public MsBuildGitFileSystem(Repository repository, Commit? commit)
        {
            _repository = repository;
            _commit = commit;
        }

        private string NormalizePathToWorkDir(string path)
            => Path.Combine(_repository.Info.WorkingDirectory, path);

        private string NormalizePathToGitDir(string path)
            => GitChangesProvider.IsWindows
                ? Path.GetRelativePath(_repository.Info.WorkingDirectory, path)
                    .Replace('\\', '/')
                : Path.GetRelativePath(_repository.Info.WorkingDirectory, path);

        /// <summary>
        /// Returns <b>true</b> when the path belongs to the file system and not to a commit. <br/>
        /// Returns <b>false</b> when the path belongs to the commit. <para/>
        ///
        /// When a <paramref name="path"/> belongs to the file system it means that we will get file content for this
        /// path from the file system. If not, it belongs to the commit so we will get the content from the commit. <para/>
        /// 
        /// There are 2 scenarios where a <paramref name="path"/> qualify as belonging to the file system:
        ///
        /// <list type="number">
        ///   <item>There is no Commit (commit is null), i.e. this file system is pointing at the working directory. <br/> OR</item>
        ///   <item>There is a Commit but the <paramref name="path"/> is outside of the boundaries of the repository.</item>
        /// </list>
        ///
        /// The 2nd scenario states we're in file system representing the commit however, the <paramref name="path"/> might
        /// reference a location on the disk not part of the repository so in that case we get the files from the file system.
        ///
        /// For example, If our project is located in `a/b/c/d/proj.csproj` <br/>
        /// Any <paramref name="path"/> referencing `a/b/c/d/**.*` does not belong to the file system. <br/>
        /// However, if it point to `a/b/w/p.props` it is not part of the repository so we will load it from the file system. <br/>
        ///
        /// MSBuild search for `Directory.Build.props` up to the root of the drive and also in SDK folders, outside of the
        /// repository. Regardless of the commit, those files must serve from the disk.
        /// </summary>
        /// <param name="path"></param>
        protected bool UseFileSystem(string path)
            => _commit is null || !path.StartsWith(_repository.Info.WorkingDirectory);

        private Stream GetFileStreamGit(Commit commit, string path)
        {
            var treeEntry = commit[NormalizePathToGitDir(path)];
            var blob = (Blob)treeEntry.Target;
            return blob.GetContentStream();
        }

        /// <summary>
        /// Use this for var sr = new StreamReader(path)
        /// </summary>
        public override TextReader ReadFile(string path)
        {
            // Read from filesystem or from the commit
            return UseFileSystem(path)
                ? new StreamReader(NormalizePathToWorkDir(path))
                : new StreamReader(GetFileStreamGit(_commit!, path), Encoding.UTF8);
        }

        /// <summary>
        /// Use this for new FileStream(path, mode, access, share)
        /// </summary>
        public override Stream GetFileStream(string path, FileMode mode, FileAccess access, FileShare share)
        {
            if (UseFileSystem(path))
                return File.Open(NormalizePathToWorkDir(path), mode, access, share);

            switch (mode)
            {
                case FileMode.CreateNew:
                case FileMode.Create:
                case FileMode.Truncate:
                case FileMode.Append:
                case FileMode.OpenOrCreate:
                    throw new InvalidOperationException(
                        $"Git virtual filesystem is readonly. [FileMode: {mode.ToString()}]");
                case FileMode.Open:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }

            switch (access)
            {
                case FileAccess.Write:
                case FileAccess.ReadWrite:
                    throw new InvalidOperationException(
                        $"Git virtual filesystem is readonly. [FileAccess: {access.ToString()}]");
                case FileAccess.Read:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(access), access, null);
            }

            return GetFileStreamGit(_commit!, path);
        }

        /// <summary>
        /// Use this for File.ReadAllText(path)
        /// </summary>
        public override string ReadFileAllText(string path)
        {
            if (UseFileSystem(path))
                return File.ReadAllText(NormalizePathToWorkDir(path));

            using var tr = ReadFile(path);
            return tr.ReadToEnd();
        }

        /// <summary>
        /// Use this for File.ReadAllBytes(path)
        /// </summary>
        public override byte[] ReadFileAllBytes(string path) => Encoding.UTF8.GetBytes(ReadFileAllText(path));

        /// <summary>
        /// Use this for Directory.EnumerateFiles(path, pattern, option)
        /// </summary>
        public override IEnumerable<string> EnumerateFiles(string path, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (UseFileSystem(path))
            {
                foreach (var entry in Directory.EnumerateDirectories(NormalizePathToWorkDir(path), searchPattern,
                             searchOption))
                    yield return entry;
            }
            else
            {
                var d = (Tree)_commit![NormalizePathToGitDir(path)]
                    .Target;
                foreach (var entry in d)
                {
                    if (entry.TargetType != TreeEntryTargetType.Blob)
                        continue;
                    if (FileSystemName.MatchesWin32Expression(searchPattern.AsSpan(), entry.Name, false))
                    {
                        yield return $"{path}{Path.DirectorySeparatorChar}{entry.Name}";
                        if (searchOption == SearchOption.AllDirectories)
                        {
                            foreach (var sub in EnumerateFileSystemEntries(
                                         $"{path}{Path.DirectorySeparatorChar}{entry.Name}", searchPattern,
                                         searchOption))
                                yield return sub;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Use this for Directory.EnumerateFolders(path, pattern, option)
        /// </summary>
        public override IEnumerable<string> EnumerateDirectories(string path, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (UseFileSystem(path))
            {
                foreach (var entry in Directory.EnumerateDirectories(NormalizePathToWorkDir(path), searchPattern,
                             searchOption))
                    yield return entry;
            }
            else
            {
                var d = (Tree)_commit![NormalizePathToGitDir(path)]
                    .Target;
                foreach (var entry in d)
                {
                    if (entry.TargetType != TreeEntryTargetType.Tree)
                        continue;
                    if (FileSystemName.MatchesWin32Expression(searchPattern.AsSpan(), entry.Name, false))
                    {
                        yield return $"{path}{Path.DirectorySeparatorChar}{entry.Name}";
                        if (searchOption == SearchOption.AllDirectories)
                        {
                            foreach (var sub in EnumerateFileSystemEntries(
                                         $"{path}{Path.DirectorySeparatorChar}{entry.Name}", searchPattern,
                                         searchOption))
                                yield return sub;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Use this for Directory.EnumerateFileSystemEntries(path, pattern, option)
        /// </summary>
        public override IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (UseFileSystem(path))
            {
                foreach (var entry in Directory.EnumerateFileSystemEntries(NormalizePathToWorkDir(path), searchPattern,
                             searchOption))
                    yield return entry;
            }
            else
            {
                var d = (Tree)_commit![NormalizePathToGitDir(path)]
                    .Target;
                foreach (var entry in d)
                {
                    if (FileSystemName.MatchesWin32Expression(searchPattern.AsSpan(), entry.Name, false))
                    {
                        yield return $"{path}{Path.DirectorySeparatorChar}{entry.Name}";
                        if (searchOption == SearchOption.AllDirectories)
                        {
                            foreach (var sub in EnumerateFileSystemEntries(
                                         $"{path}{Path.DirectorySeparatorChar}{entry.Name}", searchPattern,
                                         searchOption))
                                yield return sub;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Use this for File.GetAttributes()
        /// </summary>
        public override FileAttributes GetAttributes(string path) => DirectoryExists(path)
            ? FileAttributes.Directory
            : FileAttributes.Normal;

        /// <summary>
        /// Use this for File.GetLastWriteTimeUtc(path)
        /// </summary>
        public override DateTime GetLastWriteTimeUtc(string path)
        {
            return UseFileSystem(path)
                ? new FileInfo(NormalizePathToWorkDir(path)).LastWriteTimeUtc
                : _commit!.Author.When.UtcDateTime;
        }

        /// <summary>
        /// Use this for Directory.Exists(path)
        /// </summary>
        public override bool DirectoryExists(string path)
        {
            if (UseFileSystem(path))
                return File.Exists(NormalizePathToWorkDir(path));

            var treeEntry = _commit?[NormalizePathToGitDir(path)];
            if (treeEntry == null)
                return false;
            switch (treeEntry.Mode)
            {
                case Mode.Directory:
                    return true;
                case Mode.Nonexistent:
                case Mode.NonExecutableFile:
                case Mode.NonExecutableGroupWritableFile:
                case Mode.ExecutableFile:
                case Mode.SymbolicLink:
                case Mode.GitLink:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Use this for File.Exists(path)
        /// </summary>
        public override bool FileExists(string path)
        {
            if (UseFileSystem(path))
                return File.Exists(NormalizePathToWorkDir(path));

            var treeEntry = _commit?[NormalizePathToGitDir(path)];
            if (treeEntry == null)
                return false;
            switch (treeEntry.Mode)
            {
                case Mode.Nonexistent:
                case Mode.Directory:
                    return false;
                case Mode.NonExecutableFile:
                case Mode.NonExecutableGroupWritableFile:
                case Mode.ExecutableFile:
                case Mode.SymbolicLink:
                case Mode.GitLink:
                    return true;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Use this for File.Exists(path) || Directory.Exists(path)
        /// </summary>
        public override bool FileOrDirectoryExists(string path)
        {
            if (UseFileSystem(path))
            {
                var p = NormalizePathToWorkDir(path);
                return File.Exists(p) || Directory.Exists(p);
            }

            var treeEntry = _commit?[NormalizePathToGitDir(path)];
            if (treeEntry == null)
                return false;
            switch (treeEntry.Mode)
            {
                case Mode.Nonexistent:
                    return false;
                case Mode.Directory:
                case Mode.NonExecutableFile:
                case Mode.NonExecutableGroupWritableFile:
                case Mode.ExecutableFile:
                case Mode.SymbolicLink:
                case Mode.GitLink:
                    return true;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
