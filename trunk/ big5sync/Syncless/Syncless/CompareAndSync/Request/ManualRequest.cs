using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Filters;

namespace Syncless.CompareAndSync.Request
{
    public abstract class ManualRequest
    {
        private string[] _paths, _unavailablePaths;
        private List<Filter> _filters;

        public ManualRequest(string[] paths, string[] unavailablePaths, List<Filter> filters)
        {
            _paths = paths;
            _unavailablePaths = unavailablePaths;
            _filters = filters;
        }

        public string[] Paths
        {
            get { return _paths; }
        }

        public string[] UnavailablePaths
        {
            get { return _unavailablePaths; }
        }

        public List<Filter> Filters
        {
            get { return _filters; }
        }

    }
}
