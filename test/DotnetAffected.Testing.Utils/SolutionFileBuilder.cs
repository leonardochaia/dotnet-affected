// --------------------------------------------------------------------
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// --------------------------------------------------------------------

// --------------------------------------------------------------------
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// --------------------------------------------------------------------


// Simplified version due to internal symbols. Credits to:
// https://github.com/dotnet/msbuild/blob/9bcc06cbe19ae2482ab18eab90a82fd079b26897/src/Build.UnitTests/Graph/GraphTestingUtilities.cs#L185

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotnetAffected.Testing.Utils
{
    internal class SolutionFileBuilder
    {
        /// <summary>
        /// projectName -> projectPath
        /// </summary>
        public IReadOnlyDictionary<string, string> Projects { get; set; }

        private readonly struct ProjectInfo
        {
            public string Name { get; }
            public string Path { get; }
            public string ProjectTypeGuid { get; }
            public string Guid { get; }

            public ProjectInfo(string name, string path, string projectTypeGuid, string guid)
            {
                Name = name;
                Path = path;
                ProjectTypeGuid = projectTypeGuid;
                Guid = guid;
            }
        }

        public string BuildSolution()
        {
            var projectInfos = Projects.ToDictionary(
                kvp => kvp.Key,
                kvp => new ProjectInfo(
                    kvp.Key,
                    kvp.Value,
                    "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}",
                    Guid.NewGuid()
                        .ToString("B")));
            var sb = new StringBuilder();

            sb.AppendLine(@"
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio 15
VisualStudioVersion = 15.0.27004.2009
MinimumVisualStudioVersion = 10.0.40219.1");

            foreach (var project in projectInfos.Values)
            {
                sb.Append(@"
Project(""")
                    .Append(project.ProjectTypeGuid)
                    .Append(@""") = """)
                    .Append(project.Name)
                    .Append(@""", """)
                    .Append(project.Path)
                    .Append(@""", """)
                    .Append(project.Guid)
                    .AppendLine(@"""");
                sb.AppendLine(@"
EndProject");
            }

            sb.AppendLine("Global");

            sb.AppendLine("\tGlobalSection(SolutionConfigurationPlatforms) = preSolution");

            sb.Append("\t\t")
                .Append("Debug|AnyCPU")
                .Append(" = ")
                .AppendLine("Debug|AnyCPU");

            sb.AppendLine("\tEndGlobalSection");

            sb.AppendLine("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution");

            sb.AppendLine("\tEndGlobalSection");

            sb.AppendLine("EndGlobal");

            return sb.ToString();
        }
    }
}
