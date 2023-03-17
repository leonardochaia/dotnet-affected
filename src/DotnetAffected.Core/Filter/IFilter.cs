
namespace DotnetAffected.Core.Filter
{
    /// <summary>
    /// Interface to Provide filter methods for a specific object type.
    /// </summary>
    /// <typeparam name="TObj">The type of the object which will be filtered</typeparam>
    public interface IFilter<in TObj>
    {
        /// <summary>
        /// Method to decide if object is matched by the filter.
        /// </summary>
        /// <param name="obj">The object which will be tested</param>
        /// <returns>True if the object is matched, False otherwise</returns>
        public bool Matches(TObj obj);
    }
}
