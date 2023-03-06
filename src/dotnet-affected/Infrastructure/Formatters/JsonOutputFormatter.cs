using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace Affected.Cli.Formatters
{
    internal class JsonOutputFormatter : IOutputFormatter
    {
        public string Type => "json";

        public string NewFileExtension => ".json";

        public static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true
        };

        public Task<string> Format(IEnumerable<IProjectInfo> projects)
        {
            return Task.FromResult(JsonSerializer.Serialize(projects, SerializerOptions));
        }
    }
}
