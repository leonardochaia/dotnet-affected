using LibGit2Sharp;
using Microsoft.Build.Evaluation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DotnetAffected.Core.FileSystem
{
    /// <summary>
    /// A wrapper around the git filesystem that works around the MSBuild issue https://github.com/dotnet/msbuild/issues/7956 <br/>
    /// The build process does not use the file system to ge the content of a file when it needs to load a nested project. <br/>
    /// It will only use the file system to query if a file exists or not, if it exist it will just load the file using the file system <para/>
    ///
    /// To workaround it we simply listen to all FileExists requests and if the file exists and it's part of the <see cref="MsBuildGitFileSystem.UseFileSystem">commit</see>
    /// we will eager load it on the spot, before the build will try to load it, so by the time the build loads it, it's in the cache. <br/>
    ///
    /// We only listen before we process the root file and stop right after the root is loaded.
    /// </summary>
    internal class EagerCachingMsBuildGitFileSystem : MsBuildGitFileSystem, IDisposable
    {
        private readonly Commit? _commit;
        private ProjectFactory? _projectFactory;
        private Action<string>? _onEagerCacheRequired;

        public EagerCachingMsBuildGitFileSystem(Repository repository, Commit? commit) : base(repository, commit)
        {
            _commit = commit;
        }
        
        /// <summary>
        /// Extensions MSBuild uses for projects and imports.
        ///
        /// Imports are not required to use one of these, but recognising them by extension keeps
        /// this off the read path: the git file system does no caching, so deciding by content
        /// would cost an extra blob read for every file MSBuild probes for.
        ///
        /// Imports using a non standard extension are therefore deliberately not eager loaded.
        /// If that ever needs supporting, an option carrying extra extensions to recognise is a
        /// better trade than inspecting the contents of everything that gets probed.
        /// </summary>
        private static readonly HashSet<string> MsBuildProjectExtensions =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".props", ".targets", ".proj", ".csproj", ".fsproj", ".vbproj",
                ".vcxproj", ".projitems", ".shproj", ".tasks", ".overridetasks", ".user",
            };

        /// <summary>
        /// Use this for File.Exists(path)
        /// </summary>
        public override bool FileExists(string path)
        {
            var result = base.FileExists(path);
            if (result && _onEagerCacheRequired is not null && !UseFileSystem(path) && IsMsBuildProjectFile(path))
                _onEagerCacheRequired.Invoke(path);
            return result;
        }

        /// <summary>
        /// MSBuild probes for the existence of plenty of files that are not MSBuild projects:
        /// <c>global.json</c> while resolving the SDK, or a <c>.slnx</c> reached through
        /// <c>[MSBuild]::GetDirectoryNameOfFileAbove</c>. Eager loading those as projects throws,
        /// so only eager load what MSBuild could actually import.
        /// See https://github.com/leonardochaia/dotnet-affected/issues/155
        /// </summary>
        private static bool IsMsBuildProjectFile(string path)
            => MsBuildProjectExtensions.Contains(Path.GetExtension(path));

        public Project CreateProjectAndEagerLoadChildren(string path)
        {
            // Do not alter, the list is used to maintain a reference for projects created internally
            // because the ProjectCollection cache use's a WeakMap to manage cached projects loaded as nested project of a root.
            var projects = new List<Project>();
            _projectFactory ??= new ProjectFactory(this, new ProjectCollection());

            _onEagerCacheRequired = s => projects.Add(_projectFactory.CreateProject(s));
            projects.Add(_projectFactory.CreateProject(path));
            _onEagerCacheRequired = null;

            return projects.Last();
        }

        public void Dispose()
        {
            _projectFactory?.Dispose();
        }
    }
}
