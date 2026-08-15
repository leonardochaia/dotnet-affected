namespace DotnetAffected.Core
{
    /// <summary>
    /// The outcome of discovering projects: the ones to work with, and the ones deliberately left
    /// out of it.
    /// </summary>
    internal class ProjectDiscoveryResult
    {
        public ProjectDiscoveryResult(string[] projects, string[] excludedProjects)
        {
            Projects = projects;
            ExcludedProjects = excludedProjects;
        }

        /// <summary>
        /// Projects to build the graph from.
        /// </summary>
        public string[] Projects { get; }

        /// <summary>
        /// Projects kept out of the graph, which are therefore never evaluated. Paths only: there
        /// is no evaluated project to describe them with, which is the entire point.
        /// </summary>
        public string[] ExcludedProjects { get; }
    }
}
