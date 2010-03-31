using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Core.View
{
    public class PathView
    {
        private string _path;
        private bool _isDeleted;
        private bool _isAvailable;
        private bool _isMissing;

        public bool IsAvailable
        {
            get { return _isAvailable; }
            set { _isAvailable = value; }
        }

        public bool IsDeleted
        {
            get { return _isDeleted; }
            set { _isDeleted = value; }
        }

        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }

        public bool IsMissing
        {
            get { return _isMissing; }
            set { _isMissing = value; }
        }

        public PathView(string path)
        {
            _path = path;
            _isAvailable = true;
            _isDeleted = false;
            _isMissing = false;
        }
    }
}
