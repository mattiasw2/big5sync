using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.CompareAndSync.Enum;

namespace Syncless.CompareAndSync.Request
{
    public class AutoSyncRequest : Request
    {

        private string _sourceName, _sourceParent, _oldName;
        private List<string> _destinations;
        private AutoSyncRequestType? _requestType;
        private bool? _isFolder;

        public AutoSyncRequest(string sourceName, string sourceParent, List<string> destinations, AutoSyncRequestType requestType)
        {
            _sourceName = sourceName;
            _sourceParent = sourceParent;
            _destinations = destinations;
            _requestType = requestType;
        }

        public AutoSyncRequest(string sourceName, string sourceParent, List<string> destinations, bool? isFolder, AutoSyncRequestType requestType)
            : this(sourceName, sourceParent, destinations, requestType)
        {
            _isFolder = isFolder;
        }

        public AutoSyncRequest(string oldName, string newName, string sourceParent, List<string> destinations, bool? isFolder, AutoSyncRequestType requestType)
            : this(oldName, sourceParent, destinations, isFolder, requestType)
        {
            _oldName = oldName;
        }

        public string SourceName
        {
            get { return _sourceName; }
        }

        public string NewName
        {
            get
            {
                if (_requestType == AutoSyncRequestType.Rename)
                    return _sourceName;
                else
                    return null;
            }
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
    }
}
