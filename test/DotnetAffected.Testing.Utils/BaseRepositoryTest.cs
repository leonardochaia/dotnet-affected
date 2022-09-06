using System;

namespace DotnetAffected.Testing.Utils
{
    public abstract class BaseRepositoryTest
        : BaseMSBuildTest, IDisposable
    {
        protected TemporaryRepository Repository { get; } = new TemporaryRepository();

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool dispose)
        {
            if (!dispose) return;

            Repository?.Dispose();
        }
    }
}
