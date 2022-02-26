using LibGit2Sharp;
using System;

namespace Affected.Cli.Tests
{
    public class TemporaryRepository : IDisposable
    {
        public TemporaryRepository()
        {
            this.Directory = new TempWorkingDirectory();
            
            // git init a repo at the path
            Repository.Init(this.Path);

            // open the repository
            this.Repository = new Repository(this.Path);

            // Create the first commit
            this.Commit("Initial Commit");
        }

        public Repository Repository { get; }

        public TempWorkingDirectory Directory { get; }

        public string Path => this.Directory.Path;

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
            message ??= Guid.NewGuid().ToString("N");
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
