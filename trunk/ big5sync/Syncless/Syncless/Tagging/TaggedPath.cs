using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Tagging
{
    public class TaggedPath
    {
        private string _logicalid;
        private string _path;
        private long _lastUpdated;
        private long _created;
        private bool _isDeleted;
        private long _deletedDate;

        public string LogicalDriveId
        {
            get { return _logicalid; }
            set { _logicalid = value; }
        }
        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }
        public long LastUpdated
        {
            get { return _lastUpdated; }
            set { _lastUpdated = value; }
        }
        public long Created
        {
            get { return _created; }
            set { _created = value; }
        }
        public bool IsDeleted
        {
            get { return _isDeleted; }
            set { _isDeleted = value; }
        }
        public long DeletedDate
        {
            get { return _deletedDate; }
            set { _deletedDate = value; }
        }

        public TaggedPath(string path, long created)
        {
            this._logicalid = TaggingHelper.GetLogicalID(path);
            this._path = path;
            this._lastUpdated = created;
            this._created = created;
            this._isDeleted = false;
            this._deletedDate = 0;
        }

        public void Remove(long deletedDate)
        {
            _isDeleted = true;
            _deletedDate = deletedDate;
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

        public void Replace(string oldPath, string newPath)
        {
            _path = _path.Replace(oldPath, newPath);
        }
    }
}
