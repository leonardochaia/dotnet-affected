using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using System.Text;
using System.Xml;

namespace DotnetAffected.Core.FileSystem
{
    /// <summary>
    /// The working directory as it is on disk, with deleted files put back.
    ///
    /// A deleted file is not on disk any more, so it matches no glob and satisfies no
    /// <c>Exists()</c> condition. The project that referenced it therefore evaluates as if the
    /// file had never been there, and nothing attributes the deletion back to it.
    /// See https://github.com/leonardochaia/dotnet-affected/issues/84
    ///
    /// Everything except the deleted paths is served straight from disk. That keeps evaluation
    /// on the real file system, rather than reconstructing the whole tree from a commit.
    /// </summary>
    internal class DeletedFilesOverlayFileSystem : MSBuildFileSystemBase
    {
        private static readonly StringComparer PathComparer = GitChangesProvider.IsWindows
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;

        /// <summary>Contents of the files the diff removed, keyed by absolute path.</summary>
        private readonly IReadOnlyDictionary<string, byte[]> _deletedFiles;

        /// <summary>
        /// Directories that only exist because a deleted file lived in them. Enumerating a
        /// parent has to reach them, otherwise a recursive glob stops before the deleted file.
        /// </summary>
        private readonly HashSet<string> _deletedDirectories;

        public DeletedFilesOverlayFileSystem(
            string repositoryRoot,
            IReadOnlyDictionary<string, byte[]> deletedFiles)
        {
            _deletedFiles = deletedFiles;
            _deletedDirectories = new HashSet<string>(PathComparer);

            var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(repositoryRoot));
            foreach (var file in _deletedFiles.Keys)
            {
                var directory = Path.GetDirectoryName(file);
                while (!string.IsNullOrEmpty(directory)
                       && directory.Length > root.Length
                       && _deletedDirectories.Add(directory))
                {
                    directory = Path.GetDirectoryName(directory);
                }
            }
        }

        private ProjectCollection? _projectCollection;
        private readonly HashSet<string> _preloaded = new HashSet<string>(PathComparer);

        /// <summary>
        /// Registers the deleted imports in <paramref name="projectCollection"/> ahead of
        /// evaluation.
        ///
        /// MSBuild resolves an <c>Import</c> by reading it off the real disk rather than through
        /// the file system it was given (dotnet/msbuild#7956), so a deleted import would fail to
        /// resolve no matter what this file system reports. Registering it as a
        /// <see cref="ProjectRootElement"/> first means import resolution finds it in the
        /// collection's cache and never reaches the disk.
        /// </summary>
        public void AttachProjectCollection(ProjectCollection projectCollection)
        {
            _projectCollection = projectCollection;

            // Eagerly, not on demand: an unconditional Import never asks whether the file
            // exists, so waiting for a FileExists call would miss it entirely.
            foreach (var file in _deletedFiles.Keys)
                PreloadIfImportable(file);
        }

        private void PreloadIfImportable(string fullPath)
        {
            if (_projectCollection is null)
                return;
            if (!MsBuildFileExtensions.IsMsBuildProjectFile(fullPath))
                return;
            if (!_preloaded.Add(fullPath))
                return;

            using var stream = GetDeletedFileStream(fullPath);
            using var reader = new XmlTextReader(stream);

            var rootElement = ProjectRootElement.Create(reader, _projectCollection);
            rootElement.FullPath = fullPath;
        }

        private bool IsDeleted(string path) => _deletedFiles.ContainsKey(Path.GetFullPath(path));

        private Stream GetDeletedFileStream(string path)
            => new MemoryStream(_deletedFiles[Path.GetFullPath(path)], writable: false);

        /// <summary>
        /// Deleted entries directly in, or underneath, <paramref name="path"/>.
        /// </summary>
        private IEnumerable<string> DeletedEntriesUnder(
            IEnumerable<string> candidates,
            string path,
            string searchPattern,
            SearchOption searchOption)
        {
            var directory = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
            var prefix = directory + Path.DirectorySeparatorChar;

            foreach (var candidate in candidates)
            {
                if (!candidate.StartsWith(prefix, StringComparison.Ordinal))
                    continue;

                if (searchOption == SearchOption.TopDirectoryOnly
                    && !PathComparer.Equals(Path.GetDirectoryName(candidate), directory))
                    continue;

                var name = Path.GetFileName(candidate);
                if (FileSystemName.MatchesWin32Expression(searchPattern.AsSpan(), name, false))
                    yield return candidate;
            }
        }

        private IEnumerable<string> Combine(
            IEnumerable<string> fromDisk,
            IEnumerable<string> fromDeleted)
        {
            var seen = new HashSet<string>(PathComparer);
            foreach (var entry in fromDisk)
            {
                if (seen.Add(Path.GetFullPath(entry)))
                    yield return entry;
            }

            foreach (var entry in fromDeleted)
            {
                if (seen.Add(entry))
                    yield return entry;
            }
        }

        public override bool FileExists(string path)
        {
            if (File.Exists(path))
                return true;

            if (!IsDeleted(path))
                return false;

            // Get the deleted file into the collection's cache before MSBuild tries to import it.
            PreloadIfImportable(Path.GetFullPath(path));
            return true;
        }

        public override bool DirectoryExists(string path)
            => Directory.Exists(path) || _deletedDirectories.Contains(Path.GetFullPath(path));

        public override bool FileOrDirectoryExists(string path)
            => FileExists(path) || DirectoryExists(path);

        public override TextReader ReadFile(string path)
            => IsDeleted(path)
                ? new StreamReader(GetDeletedFileStream(path), Encoding.UTF8)
                : new StreamReader(path);

        public override Stream GetFileStream(string path, FileMode mode, FileAccess access, FileShare share)
        {
            if (!IsDeleted(path))
                return File.Open(path, mode, access, share);

            if (mode != FileMode.Open || access != FileAccess.Read)
                throw new InvalidOperationException(
                    $"Deleted files are readonly. [FileMode: {mode}, FileAccess: {access}]");

            return GetDeletedFileStream(path);
        }

        public override string ReadFileAllText(string path)
        {
            if (!IsDeleted(path))
                return File.ReadAllText(path);

            using var reader = ReadFile(path);
            return reader.ReadToEnd();
        }

        public override byte[] ReadFileAllBytes(string path)
            => IsDeleted(path)
                ? Encoding.UTF8.GetBytes(ReadFileAllText(path))
                : File.ReadAllBytes(path);

        public override IEnumerable<string> EnumerateFiles(string path, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
            => Combine(
                Directory.Exists(path)
                    ? Directory.EnumerateFiles(path, searchPattern, searchOption)
                    : Enumerable.Empty<string>(),
                DeletedEntriesUnder(_deletedFiles.Keys, path, searchPattern, searchOption));

        public override IEnumerable<string> EnumerateDirectories(string path, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
            => Combine(
                Directory.Exists(path)
                    ? Directory.EnumerateDirectories(path, searchPattern, searchOption)
                    : Enumerable.Empty<string>(),
                DeletedEntriesUnder(_deletedDirectories, path, searchPattern, searchOption));

        public override IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
            => Combine(
                Directory.Exists(path)
                    ? Directory.EnumerateFileSystemEntries(path, searchPattern, searchOption)
                    : Enumerable.Empty<string>(),
                DeletedEntriesUnder(_deletedFiles.Keys.Concat(_deletedDirectories), path, searchPattern, searchOption));

        public override FileAttributes GetAttributes(string path)
            => DirectoryExists(path) ? FileAttributes.Directory : FileAttributes.Normal;

        public override DateTime GetLastWriteTimeUtc(string path)
            // Deleted files have no meaningful timestamp. Nothing in evaluation depends on it,
            // and reporting "just now" keeps any up to date check from treating them as stale.
            => IsDeleted(path)
                ? DateTime.UtcNow
                : new FileInfo(path).LastWriteTimeUtc;
    }
}
