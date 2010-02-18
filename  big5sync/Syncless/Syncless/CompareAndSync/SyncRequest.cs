using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Tagging;
namespace Syncless.CompareAndSync
{
    public class SyncRequest : Request
    {
        List<CompareResult> _results = null;

        public SyncRequest(string tagName, List<string> paths, bool isFolder)
        {
            base._tagName = tagName;
            base._paths = paths;
            base._isFolder = isFolder;
        }

        public SyncRequest(string tagName, List<string> paths, bool isFolder, List<CompareResult> results) :
            this(tagName, paths, isFolder)
        {
            _results = results;
        }

        public List<CompareResult> Results
        {
            get { return _results; }
            set { _results = value; }
        }
    }
}
