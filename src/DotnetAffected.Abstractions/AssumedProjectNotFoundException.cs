using System;

namespace DotnetAffected.Abstractions
{
    /// <summary>
    /// Thrown when an assumed change matches no project.
    /// </summary>
    /// <remarks>
    /// Reported as an error rather than an empty result on purpose: assuming changes to a project
    /// that cannot be found would otherwise report nothing as affected, which reads exactly like a
    /// correct answer.
    /// </remarks>
    public class AssumedProjectNotFoundException : Exception
    {
        /// <summary>
        /// Creates the exception for an assumption that matched no project.
        /// </summary>
        /// <param name="assumption">The assumed change that could not be resolved.</param>
        public AssumedProjectNotFoundException(string assumption)
            : base($"Couldn't find a project matching '{assumption}'. " +
                   "Assumed changes accept a project name, a project file name, " +
                   "or a path to a project file.")
        {
            Assumption = assumption;
        }

        /// <summary>
        /// Gets the assumed change that could not be resolved.
        /// </summary>
        public string Assumption { get; }
    }
}
