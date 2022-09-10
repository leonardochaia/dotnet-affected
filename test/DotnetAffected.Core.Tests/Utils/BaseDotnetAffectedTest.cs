using DotnetAffected.Abstractions;
using DotnetAffected.Testing.Utils;
using System;

namespace DotnetAffected.Core.Tests
{
    public class BaseDotnetAffectedTest : BaseRepositoryTest
    {
        private readonly Lazy<AffectedSummary> _affectedSummaryLazy;

        public BaseDotnetAffectedTest()
        {
            this._affectedSummaryLazy = new Lazy<AffectedSummary>(() =>
            {
                var executor = new AffectedExecutor(Options, changesProvider: ChangesProvider);
                return executor.Execute();
            });
        }

        protected virtual AffectedOptions Options => new AffectedOptions(Repository.Path);

        protected virtual IChangesProvider ChangesProvider => new GitChangesProvider();

        protected AffectedSummary AffectedSummary => _affectedSummaryLazy.Value;
    }
}
