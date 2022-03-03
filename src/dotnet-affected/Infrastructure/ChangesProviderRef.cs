using Affected.Cli.Commands;
using System.Linq;

namespace Affected.Cli
{
    /// <summary>
    /// Resolves which <see cref="IChangesProvider"/> implementation to use based on user input.
    /// </summary>
    internal class ChangesProviderRef : IChangesProviderRef
    {
        private readonly GitChangesProvider _gitChangesProvider;
        private readonly AssumptionChangesProvider _assumptionChangesProvider;
        private readonly CommandExecutionData _data;

        public ChangesProviderRef(
            GitChangesProvider gitChangesProvider,
            AssumptionChangesProvider assumptionChangesProvider,
            CommandExecutionData data)
        {
            _gitChangesProvider = gitChangesProvider;
            _assumptionChangesProvider = assumptionChangesProvider;
            _data = data;
        }

        public IChangesProvider Value => this.DetermineChangesProvider();

        private IChangesProvider DetermineChangesProvider()
        {
            // REMARKS: we could improve this logic in the future.
            if (_data.AssumeChanges.Any())
            {
                return this._assumptionChangesProvider;
            }

            return this._gitChangesProvider;
        }
    }
}
