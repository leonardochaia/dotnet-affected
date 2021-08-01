using System;

namespace Affected.Cli
{
    internal class NoChangesException : Exception
    {
        public NoChangesException()
            : base("No affected nor changed projects where found.")
        {
        }
    }
}
