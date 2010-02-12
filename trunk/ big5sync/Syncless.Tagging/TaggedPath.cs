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
        private string _date;
        public string Date
        {
            get { return _date; }
            set { _date = value; }
        }

    }
}
