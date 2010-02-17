using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Tagging;
namespace Syncless.CompareAndSync
{
    public class SyncRequest
    {
        private FileChangeType _changeType;
        public FileChangeType ChangeType
        {
            get { return _changeType; }
            set { this._changeType = value; }
        }

        private string _path;
        public string Path
        {
            get { return _path; }
            set { this._path = value; }
        }

        private List<string> _pathAffected;
        public List<string> PathAffected
        {
            get { return _pathAffected; }
            set { this._pathAffected = value; }
        }
            

    }
}
