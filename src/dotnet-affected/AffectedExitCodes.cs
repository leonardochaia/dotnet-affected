namespace Affected.Cli
{
    internal static class AffectedExitCodes
    {
        /// <summary>
        /// Exit code returned when there are no changed projects.
        /// </summary>
        public const int NothingChanged = 166;

        /// <summary>
        /// Exit code returned when the command cannot run as asked. <br/>
        /// Deliberately the conventional failure code, so that only
        /// <see cref="NothingChanged"/> needs special handling by callers.
        /// </summary>
        public const int Failure = 1;
    }
}
