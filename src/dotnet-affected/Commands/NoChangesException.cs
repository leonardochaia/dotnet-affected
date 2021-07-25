using System;

namespace Affected.Cli.Commands
{
    internal class NoChangesException : Exception
    {
        public NoChangesException()
            : base("No affected nor changed projects where found.")
        {
        }
    }
}
