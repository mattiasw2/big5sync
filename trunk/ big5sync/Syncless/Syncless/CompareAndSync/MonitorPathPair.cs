using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.CompareAndSync
{
    public class MonitorPathPair
    {
        private List<string> _origin;
        private string _fullPath;        

        public MonitorPathPair(List<string> origin, string fullPath)
        {
            _origin = origin;
            _fullPath = fullPath;
        }

        public List<string> Origin
        {
            get { return _origin; }
        }

        public string FullPath
        {
            get { return _fullPath; }
        }
    }
}
