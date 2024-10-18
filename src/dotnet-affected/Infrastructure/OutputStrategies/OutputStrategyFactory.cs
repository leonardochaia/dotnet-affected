using Affected.Cli.Commands;
using System;
using System.Collections.Generic;

namespace Affected.Cli
{
    internal class OutputStrategyFactory
    {
        private AffectedCommandOutputOptions Options { get; }
        
        public OutputStrategyFactory(AffectedCommandOutputOptions options)
        {
            Options = options;
        }
        
        public IOutputStrategy CreateOutputStrategy(IEnumerable<IProjectInfo> projects)
        {
            return Options.OutputStrategy switch
            {
                OutputStrategies.Combined => new CombinedOutputStrategy(Options.OutputName, Options.OutputDir, projects),
                OutputStrategies.Split => new SplitOutputStrategy(Options.OutputName, Options.OutputDir, projects),
                _ => throw new InvalidOperationException($"Unknown output strategy: {Options.OutputStrategy}")
            };
        }
    }
}
