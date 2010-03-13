using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Helper;

namespace Syncless.Tagging
{
    public class TaggedPath
    {
        private string _logicalid;
        private string _pathName;
        private long _lastUpdatedDate;
        private long _createdDate;
        private bool _isDeleted;
        private long _deletedDate;

        public string LogicalDriveId
        {
            get { return _logicalid; }
            set { _logicalid = value; }
        }
        public string PathName
        {
            get { return _pathName; }
            set { _pathName = value; }
        }
        public long LastUpdatedDate
        {
            get { return _lastUpdatedDate; }
            set { _lastUpdatedDate = value; }
        }
        public long CreatedDate
        {
            get { return _createdDate; }
            set { _createdDate = value; }
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
            this._pathName = PathHelper.FormatFolderPath(path);
            this._lastUpdatedDate = created;
            this._createdDate = created;
            this._isDeleted = false;
            this._deletedDate = 0;
        }

        public void Remove(long deletedDate)
        {
            _isDeleted = true;
            _lastUpdatedDate = deletedDate;
            _deletedDate = deletedDate;
        }

        public string Append(string trailingPath)
        {
            if (_pathName.EndsWith("\\"))
            {
                return (_pathName + trailingPath);
            }
            else
            {
                return (_pathName + "\\" + trailingPath);
            }
        }

        public void Replace(string oldPath, string newPath)
        {
            _pathName = _pathName.Replace(PathHelper.FormatFolderPath(oldPath), PathHelper.FormatFolderPath(newPath));
        }
    }
}
