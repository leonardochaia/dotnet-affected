using Microsoft.Build.Graph;
using System.Collections.Generic;

namespace Affected.Cli.Commands
{
    public interface ICommandExecutionContext
    {
        IEnumerable<ProjectGraphNode> NodesWithChanges { get; }

        IEnumerable<ProjectGraphNode> FindAffectedProjects();
    }
}
