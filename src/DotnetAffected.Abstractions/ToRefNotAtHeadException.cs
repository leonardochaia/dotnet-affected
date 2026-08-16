using System;

namespace DotnetAffected.Abstractions
{
    /// <summary>
    /// Thrown when a 'to' ref was given that the working tree is not checked out at.
    /// </summary>
    /// <remarks>
    /// Reported as an error rather than analysed on a best effort basis on purpose: the project
    /// graph is built from the working tree, so it is the only revision whose project structure
    /// can be analysed. Comparing it against the files that changed up to some other revision
    /// mixes two revisions, and the result reads exactly like a correct answer.
    /// </remarks>
    public class ToRefNotAtHeadException : Exception
    {
        /// <summary>
        /// Creates the exception for a 'to' ref the working tree is not checked out at.
        /// </summary>
        /// <param name="toRef">The 'to' ref that was asked for.</param>
        /// <param name="headSha">The commit the working tree is checked out at, if any.</param>
        public ToRefNotAtHeadException(string toRef, string? headSha)
            : base($"--to was given '{toRef}', but the working tree is checked out at " +
                   $"{headSha ?? "no commit"}. Projects are discovered and evaluated from the " +
                   "working tree, so that is the only revision whose project structure can be " +
                   $"analysed: a project that exists at '{toRef}' but not on disk would be " +
                   "counted among the files that changed while being reported under no project " +
                   $"at all. Check out '{toRef}' before running and drop --to to compare " +
                   "against the working tree.")
        {
            ToRef = toRef;
            HeadSha = headSha;
        }

        /// <summary>
        /// Gets the 'to' ref that was asked for.
        /// </summary>
        public string ToRef { get; }

        /// <summary>
        /// Gets the commit the working tree is checked out at, or null when there is none.
        /// </summary>
        public string? HeadSha { get; }
    }
}
