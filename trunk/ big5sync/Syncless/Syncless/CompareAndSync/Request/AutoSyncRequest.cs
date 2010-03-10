using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.CompareAndSync.Enum;

namespace Syncless.CompareAndSync.Request
{
    public class AutoSyncRequest : Request
    {

        private string _source;
        private List<string> _destinations;
        private AutoSyncRequestType? _requestType;
        private bool? _isFolder;

        public AutoSyncRequest(string source, List<string> destinations, AutoSyncRequestType requestType)
        {
            _source = source;
            _destinations = destinations;
            _requestType = requestType;
        }

        public AutoSyncRequest(string sourceFile, List<string> destinations, bool? isFolder, AutoSyncRequestType requestType)
            : this(sourceFile, destinations, requestType)
        {
            _isFolder = isFolder;
        }

        public string Source
        {
            get { return _source; }
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
