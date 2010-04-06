using System.Collections.Generic;
using Syncless.CompareAndSync.Enum;

namespace Syncless.CompareAndSync.Request
{
    public class AutoSyncRequest : Request
    {

        private readonly string _sourceName, _sourceParent, _oldName;
        private readonly List<string> _destinations;
        private readonly AutoSyncRequestType? _requestType;
        private readonly bool? _isFolder;
        private readonly SyncConfig _syncConfig;
        private readonly List<string> _tagList;
        public AutoSyncRequest(string sourceName, string sourceParent, List<string> destinations, AutoSyncRequestType requestType, SyncConfig syncConfig,List<string> tagList)
        {
            _sourceName = sourceName;
            _sourceParent = sourceParent;
            _destinations = destinations;
            _requestType = requestType;
            _syncConfig = syncConfig;
            _tagList = tagList;
        }

        public AutoSyncRequest(string sourceName, string sourceParent, List<string> destinations, bool? isFolder, AutoSyncRequestType requestType, SyncConfig syncConfig , List<string> tagList)
            : this(sourceName, sourceParent, destinations, requestType, syncConfig,tagList)
        {
            _isFolder = isFolder;
        }

        public AutoSyncRequest(string oldName, string newName, string sourceParent, List<string> destinations, bool? isFolder, AutoSyncRequestType requestType, SyncConfig syncConfig , List<string> tagList)
            : this(newName, sourceParent, destinations, isFolder, requestType, syncConfig,tagList)
        {
            _oldName = oldName;
        }

        public string SourceName
        {
            get { return _sourceName; }
        }

        public string NewName
        {   //not safe to return null , just return source names
            get { return _requestType == AutoSyncRequestType.Rename ? _sourceName : null; }
        }

        public string OldName
        {
            get { return _oldName; }
        }

        public string SourceParent
        {
            get { return _sourceParent; }
        }

        public List<string> DestinationFolders
        {
            get { return _destinations; }
        }

        public bool? IsFolder
        {
            get { return _isFolder; }
        }

        public AutoSyncRequestType? ChangeType
        {
            get { return _requestType; }
        }

        public SyncConfig Config
        {
            get { return _syncConfig; }
        }
    }
}
