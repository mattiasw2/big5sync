using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.CompareAndSync
{
    public class CompareResult
    {
        private string _fromPath;
        private string _toPath;

        public string FromPath
        {
            get { return _fromPath; }
            set { this._fromPath = value; }
        }

        public string ToPath
        {
            get { return _toPath; }
            set { this._toPath = value; }
        }

        private FileChangeType _change;
        public FileChangeType Change
        {
            get { return _change; }
            set { _change = value; }
        }

    }
    
}
