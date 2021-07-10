using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;

namespace Affected.Cli
{
    internal static class CommandLineBuilderExtensions
    {
        /// <summary>
        /// Configures the <typeparamref name="TCommand"/> to use the provided
        /// <typeparamref name="THandler"/>.
        /// The <typeparamref name="THandler"/> will be resolved from dependency injection.
        /// For this to work, the <see cref="CommandLineBuilder"/> must have been created using
        /// the <see cref="AffectedCommandLineBuilder"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <typeparam name="TCommand"></typeparam>
        /// <typeparam name="THandler"></typeparam>
        /// <returns></returns>
        public static CommandLineBuilder UseCommandHandler<TCommand, THandler>(
            this CommandLineBuilder builder)
            where TCommand : ICommand
            where THandler : ICommandHandler
        {
            var commandType = typeof(TCommand);
            var handlerType = typeof(THandler);
            return builder.UseMiddleware(invocation =>
            {
                // Only when we are invoking this command.
                if (invocation.ParseResult.CommandResult.Command is not Command command
                    || command.GetType() != commandType)
                {
                    return;
                }

                // When the command is invoked, it will try to resolve the Command's Handler
                // from the BindingContext's internal services, hence we register it there.
                invocation.BindingContext.AddService(handlerType,
                    sp =>
                    {
                        // Resolve the handler using our custom service collection.
                        var customServices = sp.GetRequiredService<StartupData>()
                            .ServiceCollection
                            .BuildServiceProvider(true);
                        return ActivatorUtilities.CreateInstance<THandler>(customServices);
                    });

                // If the command has already specified a handler no need to override it.
                // TODO: for some reason this is not applying to the RootCommand even when it does get executed for it.
                // Having it hard-codded on the RootCommand impl itself does work tho, so that's what we are doing for now.
                command.Handler ??= CommandHandler.Create(handlerType.GetMethod(nameof(ICommandHandler.InvokeAsync))!);
            });
        }

        public static CommandLineBuilder UseAffectedCli(
            this CommandLineBuilder builder,
            StartupData data)
        {
            return builder.UseMiddleware(invocation =>
            {
                // Register contextual stuff.
                var services = data.ServiceCollection;
                services.AddSingleton(invocation);
                services.AddSingleton(invocation.BindingContext);
                services.AddSingleton(invocation.Console);
                services.AddTransient(_ => invocation.InvocationResult!);
                services.AddTransient(_ => invocation.ParseResult);

                invocation.BindingContext
                    .AddService(_ => data);
            });
        }
    }
}
