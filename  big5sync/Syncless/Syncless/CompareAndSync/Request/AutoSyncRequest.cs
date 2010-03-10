using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.CompareAndSync.Request
{
    public class AutoSyncRequest : Request
    {

        private string _source;
        private List<string> _destinations;
        private bool? _isFolder;

        public AutoSyncRequest(string source, List<string> destinations)
        {
            _source = source;
            _destinations = destinations;
        }

        public AutoSyncRequest(string sourceFile, List<string> destinations, bool? isFolder)
            : this(sourceFile, destinations)
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
    }
}
