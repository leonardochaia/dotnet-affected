using System.CommandLine.Rendering.Views;

namespace Affected.Cli
{
    internal record RenderingErrorConfig
    {
        public RenderingErrorConfig(int exitCode)
        {
            this.ExitCode = exitCode;
        }

        public RenderingErrorConfig(int exitCode, View errorView)
            : this(exitCode)
        {
            this.ErrorView = errorView;
        }

        public int ExitCode { get; } = 1;

        public View? ErrorView { get; }
    }
}
