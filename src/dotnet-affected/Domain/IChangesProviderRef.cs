namespace Affected.Cli
{
    /// <summary>
    /// Gets a reference to the current <see cref="IChangesProvider"/>
    /// </summary>
    public interface IChangesProviderRef
    {
        /// <summary>
        /// Gets the current <see cref="IChangesProvider"/>.
        /// </summary>
        IChangesProvider Value { get; }
    }
}
