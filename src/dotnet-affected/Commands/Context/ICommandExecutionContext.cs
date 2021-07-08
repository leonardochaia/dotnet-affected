using Microsoft.Build.Graph;
using System.Collections.Generic;

namespace Affected.Cli.Commands
{
    internal interface ICommandExecutionContext
    {
        IEnumerable<ProjectGraphNode> NodesWithChanges { get; }

        IEnumerable<ProjectGraphNode> FindAffectedProjects();
    }
}
