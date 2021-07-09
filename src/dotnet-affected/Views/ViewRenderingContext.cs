using System.CommandLine;
using System.CommandLine.Rendering;
using System.CommandLine.Rendering.Views;

namespace Affected.Cli.Views
{
    internal class ViewRenderingContext
    {
        public ViewRenderingContext(IConsole console)
        {
            this.Console = console;
            this.ConsoleRenderer = new ConsoleRenderer(
                this.Console,
                mode: OutputMode.PlainText,
                resetAfterRender: true);
        }

        public ConsoleRenderer ConsoleRenderer { get; }

        public IConsole Console { get; }

        public void Render(View rootView)
        {
            rootView.Render(this.ConsoleRenderer, Region.EntireTerminal);
        }
    }
}
