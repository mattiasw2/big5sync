using System.Collections.Generic;
using Syncless.Filters;

namespace Syncless.CompareAndSync.Request
{
    /// <summary>
    /// <c>ManualCompareRequest</c> is used for handling compare/preview requests.
    /// </summary>
    public class ManualCompareRequest : ManualRequest
    {
        /// <summary>
        /// Instantiates an instance of <c>ManualCompareRequest</c>.
        /// </summary>
        /// <param name="paths">The list of paths to compare and preview.</param>
        /// <param name="filters">The list of filters to pass in.</param>
        /// <param name="syncConfig">The sync configuration to pass in.</param>
        /// <param name="tagName">The tag name to compare and preview.</param>
        public ManualCompareRequest(string[] paths, List<Filter> filters, SyncConfig syncConfig, string tagName)
            : base(paths, filters, syncConfig, tagName)
        {
        }
    }
}
