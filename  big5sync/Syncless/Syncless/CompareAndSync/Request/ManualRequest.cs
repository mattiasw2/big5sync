/*
 * 
 * Author: Soh Yuan Chin
 * 
 */

using System.Collections.Generic;
using Syncless.Filters;

namespace Syncless.CompareAndSync.Request
{
    /// <summary>
    /// Abstract class for all manual requests.
    /// </summary>
    public abstract class ManualRequest : Request
    {
        private readonly string[] _paths;
        private readonly List<Filter> _filters;
        private readonly SyncConfig _syncConfig;
        private readonly string _tagName;

        /// <summary>
        /// Instantiates an instance of <c>ManualRequest</c>.
        /// </summary>
        /// <param name="paths">The list of paths to compare and preview.</param>
        /// <param name="filters">The list of filters to pass in.</param>
        /// <param name="syncConfig">The sync configuration to pass in.</param>
        /// <param name="tagName">The tag name to compare and preview.</param>
        protected ManualRequest(string[] paths, List<Filter> filters, SyncConfig syncConfig, string tagName)
        {
            _paths = paths;
            _filters = filters;
            _syncConfig = syncConfig;
            _tagName = tagName;
        }

        /// <summary>
        /// Gets the array of paths.
        /// </summary>
        public string[] Paths
        {
            get { return _paths; }
        }

        /// <summary>
        /// Gets the list of filters.
        /// </summary>
        public List<Filter> Filters
        {
            get { return _filters; }
        }

        /// <summary>
        /// Gets the sync configuration.
        /// </summary>
        public SyncConfig Config
        {
            get { return _syncConfig; }
        }

        /// <summary>
        /// Gets the tag name.
        /// </summary>
        public string TagName
        {
            get { return _tagName; }
        }

    }
}
