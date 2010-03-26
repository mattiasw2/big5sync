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

        public AutoSyncRequest(string sourceName, string sourceParent, List<string> destinations, AutoSyncRequestType requestType, SyncConfig syncConfig)
        {
            _sourceName = sourceName;
            _sourceParent = sourceParent;
            _destinations = destinations;
            _requestType = requestType;
            _syncConfig = syncConfig;
        }

        public AutoSyncRequest(string sourceName, string sourceParent, List<string> destinations, bool? isFolder, AutoSyncRequestType requestType, SyncConfig syncConfig)
            : this(sourceName, sourceParent, destinations, requestType, syncConfig)
        {
            _isFolder = isFolder;
        }

        public AutoSyncRequest(string oldName, string newName, string sourceParent, List<string> destinations, bool? isFolder, AutoSyncRequestType requestType, SyncConfig syncConfig)
            : this(newName, sourceParent, destinations, isFolder, requestType, syncConfig)
        {
            _oldName = oldName;
        }

        public string SourceName
        {
            get { return _sourceName; }
        }

        public string NewName
        {
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
