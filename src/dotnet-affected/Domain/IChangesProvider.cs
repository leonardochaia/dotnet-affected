using System.Collections.Generic;

namespace Affected.Cli
{
    public interface IChangesProvider
    {
        IEnumerable<string> GetChangedFiles(string directory, string from, string to);
    }
}
