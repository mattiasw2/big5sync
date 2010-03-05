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

        public SyncRequest(List<string> paths, List<CompareResult> results)
        {
            base._paths = paths;
            _results = results;
        }

        public List<CompareResult> Results
        {
            get { return _results; }
            set { _results = value; }
        }
    }
}
