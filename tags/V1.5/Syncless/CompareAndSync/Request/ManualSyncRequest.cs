using System.Collections.Generic;
using Syncless.Filters;

namespace Syncless.CompareAndSync.Request
{
    /// <summary>
    /// Class to store information for a manual synchronization request.
    /// </summary>
    public class ManualSyncRequest : ManualRequest
    {
        private readonly bool _notify;

        /// <summary>
        /// Instantiates an instance of <c>ManualSyncRequest</c>.
        /// </summary>
        /// <param name="paths">The list of paths to compare and sync.</param>
        /// <param name="filters">The list of filters to pass in.</param>
        /// <param name="syncConfig">The sync configuration to pass in.</param>
        /// <param name="tagName">The tag name to compare and sync.</param>
        /// <param name="notify">Determines whether or not to notify.</param>
        public ManualSyncRequest(string[] paths, List<Filter> filters, SyncConfig syncConfig, string tagName, bool notify)
            : base(paths, filters, syncConfig, tagName)
        {
            _notify = notify;
        }

        /// <summary>
        /// Gets whether to notify or not.
        /// </summary>
        public bool Notify
        {
            get { return _notify; }
        }
    }
}
