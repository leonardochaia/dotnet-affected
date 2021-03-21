using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Rendering;
using System.CommandLine.Rendering.Views;

namespace Affected.Cli.Views
{
    internal class ViewRenderingContext
    {
        public ViewRenderingContext(
            IConsole console,
            InvocationContext invocationContext)
        {
            this.Console = console;
            this.ConsoleRenderer = new ConsoleRenderer(
                this.Console,
                mode: invocationContext.BindingContext.OutputMode(),
                resetAfterRender: true);
        }

        public ConsoleRenderer ConsoleRenderer { get; }

        public IConsole Console { get; }

        public void Render(View rootView)
        {
            var screen = new ScreenView(renderer: this.ConsoleRenderer, this.Console);
            screen.Child = rootView;
            screen.Render(Region.EntireTerminal);
        }
    }
}
