using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Affected.Cli.Formatters
{
    internal class TextOutputFormatter : IOutputFormatter
    {
        public string Type => "text";
        
        public string NewFileExtension => ".txt";

        public Task<string> Format(IEnumerable<IProjectInfo> projects)
        {
            var builder = new StringBuilder();

            foreach (var project in projects)
            {
                builder.AppendLine(project.FilePath);
            }

            return Task.FromResult(builder.ToString());
        }
    }
}
