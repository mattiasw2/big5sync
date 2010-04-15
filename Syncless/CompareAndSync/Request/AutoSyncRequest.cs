/*
 * 
 * Author: Soh Yuan Chin
 * 
 */

using System.Collections.Generic;
using Syncless.CompareAndSync.Enum;

namespace Syncless.CompareAndSync.Request
{
    /// <summary>
    /// <c>AutoSyncRequest</c> is used for auto/seamless requests/jobs.
    /// </summary>
    public class AutoSyncRequest : Request
    {

        private readonly string _sourceName, _sourceParent, _oldName;
        private readonly List<string> _destinations;
        private readonly AutoSyncRequestType? _requestType;
        private readonly bool? _isFolder;
        private readonly SyncConfig _syncConfig;
        private readonly List<string> _tagList;

        /// <summary>
        /// Instantiates an instance of <c>AutoSyncRequest</c> for create and update purposes.
        /// </summary>
        /// <param name="sourceName">The name of the source file or folder.</param>
        /// <param name="sourceParent">The parent of the source file or folder.</param>
        /// <param name="destinations">The list of destinations to propagate the source to.</param>
        /// <param name="isFolder"></param>
        /// <param name="requestType">The type of request, eg. create, update, delete or rename.</param>
        /// <param name="syncConfig">The sync configuration passed in.</param>
        /// <param name="tagList">The list of tags related to this request.</param>
        public AutoSyncRequest(string sourceName, string sourceParent, List<string> destinations, bool? isFolder, AutoSyncRequestType requestType, SyncConfig syncConfig, List<string> tagList)
            : this(sourceName, sourceParent, destinations, requestType, syncConfig, tagList)
        {
            _isFolder = isFolder;
            _tagList = tagList;
        }

        /// <summary>
        /// Instantiates an instance of <c>AutoSyncRequest</c> for delete purposes.
        /// </summary>
        /// <param name="sourceName">The name of the source file or folder.</param>
        /// <param name="sourceParent">The parent of the source file or folder.</param>
        /// <param name="destinations">The list of destinations to propagate the source to.</param>
        /// <param name="requestType">The type of request, eg. create, update, delete or rename.</param>
        /// <param name="syncConfig">The sync configuration passed in.</param>
        /// <param name="tagList">The list of tags related to this request.</param>
        public AutoSyncRequest(string sourceName, string sourceParent, List<string> destinations, AutoSyncRequestType requestType, SyncConfig syncConfig, List<string> tagList)
        {
            _sourceName = sourceName;
            _sourceParent = sourceParent;
            _destinations = destinations;
            _requestType = requestType;
            _syncConfig = syncConfig;
            _tagList = tagList;
        }

        /// <summary>
        /// Instantiates an instance of <c>AutoSyncRequest</c> for rename purposes.
        /// </summary>
        /// <param name="oldName">The old name of the object.</param>
        /// <param name="newName">The new name of the object.</param>
        /// <param name="sourceParent">The parent of the source file or folder.</param>
        /// <param name="destinations">The list of destinations to propagate the source to.</param>
        /// <param name="isFolder"></param>
        /// <param name="requestType">The type of request, eg. create, update, delete or rename.</param>
        /// <param name="syncConfig">The sync configuration passed in.</param>
        /// <param name="tagList">The list of tags related to this request.</param>
        public AutoSyncRequest(string oldName, string newName, string sourceParent, List<string> destinations, bool? isFolder, AutoSyncRequestType requestType, SyncConfig syncConfig, List<string> tagList)
            : this(newName, sourceParent, destinations, isFolder, requestType, syncConfig, tagList)
        {
            _oldName = oldName;
        }

        /// <summary>
        /// Gets or sets the source name.
        /// </summary>
        public string SourceName
        {
            get { return _sourceName; }
        }

        /// <summary>
        /// Gets the new name of this file or folder request if the <see cref="AutoSyncRequestType"/> is Rename. Otherwise, return null.
        /// </summary>
        public string NewName
        {
            get { return _requestType == AutoSyncRequestType.Rename ? _sourceName : null; }
        }

        /// <summary>
        /// Gets the old name of this file or folder.
        /// </summary>
        public string OldName
        {
            get { return _oldName; }
        }

        /// <summary>
        /// Gets the parent of the request source.
        /// </summary>
        public string SourceParent
        {
            get { return _sourceParent; }
        }

        /// <summary>
        /// Gets the list of destination folders to propagate the source to.
        /// </summary>
        public List<string> DestinationFolders
        {
            get { return _destinations; }
        }

        /// <summary>
        /// Gets whether the request type is a folder type or not. Returns null if unknown.
        /// </summary>
        public bool? IsFolder
        {
            get { return _isFolder; }
        }

        /// <summary>
        /// Gets the AutoSyncRequestType.
        /// </summary>
        public AutoSyncRequestType? ChangeType
        {
            get { return _requestType; }
        }

        /// <summary>
        /// Gets the sync configuration of this request.
        /// </summary>
        public SyncConfig Config
        {
            get { return _syncConfig; }
        }

        /// <summary>
        /// Gets the list of tags associated with this request.
        /// </summary>
        public List<string> TagList
        {
            get { return _tagList; }
        }
    }
}
