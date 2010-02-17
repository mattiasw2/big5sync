using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Tagging
{
    public class TaggedPath
    {
        private string _logicalid;

        public string LogicalDriveId
        {
            get { return _logicalid; }
            set { _logicalid = value; }
        }

        private string _path;

        public string Path
        {
            get { return _path; }
            set { _path = value;}
        }

        private long _lastUpdated;

        public long LastUpdated
        {
            get { return _lastUpdated; }
            set { _lastUpdated = value; }
        }

        public string Append(string trailingPath)
        {
            if (_path.EndsWith("\\"))
            {
                return (_path + trailingPath);
            }
            else
            {
                return (_path + "\\" + trailingPath);
            }
        }
    }
}
