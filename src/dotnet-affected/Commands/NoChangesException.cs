using System;

namespace Affected.Cli.Commands
{
    internal class NoChangesException : Exception
    {
        public NoChangesException()
            : base("No affected projects where found for the current changes")
        {
        }
    }
}
