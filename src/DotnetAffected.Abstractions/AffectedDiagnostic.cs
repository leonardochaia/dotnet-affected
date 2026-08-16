namespace DotnetAffected.Abstractions
{
    /// <summary>
    /// How much attention a <see cref="AffectedDiagnostic"/> deserves.
    /// </summary>
    public enum AffectedDiagnosticSeverity
    {
        /// <summary>
        /// Worth knowing when looking into a result, not worth interrupting anyone over.
        /// </summary>
        Info = 0,

        /// <summary>
        /// The result is probably not the one that was wanted, but it is still a result: the
        /// run reports it and carries on.
        /// </summary>
        Warning = 1,
    }

    /// <summary>
    /// Something worth saying about a run that is not part of its result.
    /// </summary>
    /// <remarks>
    /// Carried on the summary rather than written out where it is found, because the same
    /// analysis runs behind the CLI and behind the MSBuild task, and the two report to
    /// different places.
    /// </remarks>
    public class AffectedDiagnostic
    {
        /// <summary>
        /// Creates a diagnostic.
        /// </summary>
        /// <param name="severity"></param>
        /// <param name="message"></param>
        public AffectedDiagnostic(AffectedDiagnosticSeverity severity, string message)
        {
            Severity = severity;
            Message = message;
        }

        /// <summary>
        /// Gets how much attention the diagnostic deserves.
        /// </summary>
        public AffectedDiagnosticSeverity Severity { get; }

        /// <summary>
        /// Gets the message to report.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Creates a <see cref="AffectedDiagnosticSeverity.Warning"/>.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static AffectedDiagnostic Warning(string message)
            => new AffectedDiagnostic(AffectedDiagnosticSeverity.Warning, message);

        /// <summary>
        /// Creates an <see cref="AffectedDiagnosticSeverity.Info"/>.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static AffectedDiagnostic Info(string message)
            => new AffectedDiagnostic(AffectedDiagnosticSeverity.Info, message);
    }
}
