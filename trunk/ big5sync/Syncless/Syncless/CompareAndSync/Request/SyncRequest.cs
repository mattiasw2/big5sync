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

        public SyncRequest(List<string> paths)
        {
            base._paths = paths;

        }

        public SyncRequest(List<string> paths, List<CompareResult> results)
            : this(paths)
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
