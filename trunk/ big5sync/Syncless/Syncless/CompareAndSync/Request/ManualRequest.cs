using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Filters;

namespace Syncless.CompareAndSync.Request
{
    public abstract class ManualRequest
    {
        private string[] _paths;
        private List<Filter> _filters;

        public ManualRequest(string[] paths, List<Filter> filters)
        {
            _paths = paths;
            _filters = filters;
        }

        public string[] Paths
        {
            get { return _paths; }
        }

        public List<Filter> Filters
        {
            get { return _filters; }
        }

    }
}
