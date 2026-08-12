using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using System.IO;
using System.Xml;

namespace DotnetAffected.Core
{
    /// <summary>
    /// Creates and loads MSBuild Traversal Sdk projects.
    /// </summary>
    public static class TraversalProject
    {
        /// <summary>
        /// Name of the MSBuild Sdk used by traversal projects.
        /// </summary>
        public const string SdkName = "Microsoft.Build.Traversal";

        /// <summary>
        /// Version of the <see cref="SdkName"/> Sdk declared by created traversal projects.
        /// </summary>
        public const string SdkVersion = "4.1.82";

        /// <summary>
        /// Creates an empty, in memory traversal project declaring the
        /// <see cref="SdkName"/>/<see cref="SdkVersion"/> Sdk.
        /// </summary>
        /// <remarks>
        /// REMARKS: The returned project is never evaluated, so the Sdk is never resolved and no NuGet round
        /// trip takes place. Authoring <c>ProjectReference</c> items and writing out
        /// <see cref="ProjectRootElement.RawXml"/> are both purely XML operations, so there is nothing to
        /// evaluate: the Sdk only needs to be named in the output for whoever builds it later.
        /// </remarks>
        /// <returns>The traversal project's XML.</returns>
        public static ProjectRootElement Create()
        {
            var projectRootElement = $@"<Project Sdk=""{SdkName}/{SdkVersion}""></Project>";

            using var stringReader = new StringReader(projectRootElement);
            using var xmlReader = new XmlTextReader(stringReader);

            return ProjectRootElement.Create(xmlReader);
        }

        /// <summary>
        /// Loads and evaluates an existing traversal project from disk.
        /// </summary>
        /// <remarks>
        /// REMARKS: Evaluation is required here so that globs, properties and conditions written by whoever
        /// authored the project resolve. The Traversal Sdk itself is not required: it only attaches metadata
        /// to <c>ProjectReference</c> items through an ItemDefinitionGroup, and we only read their includes.
        /// Ignoring missing imports keeps evaluation working when the Sdk cannot be resolved, so that
        /// discovery does not fail when NuGet is unreachable, slow, or the feed is restricted.
        /// </remarks>
        /// <param name="path">Path to the traversal project.</param>
        /// <returns>The evaluated <see cref="Project"/>.</returns>
        public static Project Load(string path)
        {
            return new Project(
                path,
                null,
                null,
                ProjectCollection.GlobalProjectCollection,
                ProjectLoadSettings.IgnoreMissingImports);
        }
    }
}
