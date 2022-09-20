using LibGit2Sharp;
using System;

namespace DotnetAffected.Testing.Utils
{
    public class TemporaryRepository : IDisposable
    {
        public TemporaryRepository()
        {
            Directory = new TempWorkingDirectory();

            // git init a repo at the path
            Repository.Init(Directory.Path);

            // open the repository
            Repository = new Repository(Directory.Path);

            // We expose the path from a single place to maintain consistency across operating systems.
            // E.G in OSX calling "System.IO.Path.GetTempPath" will return "/var/x/y/z" which we will
            // supply to "Repository", however "Repository.Info.WorkingDirectory" will return "/private/var/x/y/z" which
            // is the same but causes issues when comparing or evaluating.
            Path = System.IO.Path.TrimEndingDirectorySeparator(Repository.Info.WorkingDirectory);

            // Create the first commit
            Commit("Initial Commit");
        }

        public Repository Repository { get; }

        public string Path { get; }

        private TempWorkingDirectory Directory { get; }


        public Commit StageAndCommit(string message = null)
        {
            this.StageAll();
            return this.Commit(message);
        }

        public void StageAll()
        {
            LibGit2Sharp.Commands.Stage(Repository, "*");
        }

        public Commit Commit(string message = null)
        {
            message ??= Guid.NewGuid()
                .ToString("N");
            var author = new Signature("Leo", "lchaia@outlook.com", DateTime.Now);
            var committer = author;

            // Commit to the repository
            return this.Repository.Commit(message, author, committer);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool dispose)
        {
            if (!dispose) return;
            this.Repository.Dispose();
            this.Directory.Dispose();
        }
    }
}
