using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.CompareAndSync
{
    public class FileCompareResult : CompareResult
    {
        private string _newHash = null;
        private long _creationTime, _lastWriteTime, _length;

        //Files: Used for delete        
        private FileCompareResult(FileChangeType changeType, string from) :
            base(changeType, from)
        {
        }
        
        //Files: Used for rename
        private FileCompareResult(FileChangeType changeType, string from, string to) :
            base(changeType, from, to)
        {
        }

        //Files: Used for create/update
        private FileCompareResult(FileChangeType changeType, string from, string to, string newHash, long creationTime, long lastWriteTime, long length) :
            this(changeType, from, to)
        {
            _newHash = newHash;
            _creationTime = creationTime;
            _lastWriteTime = lastWriteTime;
            _length = length;
        }

        public string NewHash
        {
            get
            {
                return _newHash;
            }
        }

        public long CreationTime
        {
            get
            {
                return _creationTime;
            }
        }

        public long LastWriteTime
        {
            get
            {
                return _lastWriteTime;
            }
        }

        public long Length
        {
            get
            {
                return _length;
            }
        }

        public static FileCompareResult CreateFileCompareResult(string from, string to, string newHash, long creationTime, long lastWriteTime, long length)
        {
            return new FileCompareResult(FileChangeType.Create, from, to, newHash, creationTime, lastWriteTime, length);
        }

        public static FileCompareResult UpdateFileCompareResult(string from, string to, string newHash, long creationTime, long lastWriteTime, long length)
        {
            return new FileCompareResult(FileChangeType.Update, from, to, newHash, creationTime, lastWriteTime, length);
        }

        public static FileCompareResult RenameFileCompareResult(string from, string to)
        {
            return new FileCompareResult(FileChangeType.Rename, from, to);
        }

        public static FileCompareResult DeleteFileCompareResult(string from)
        {
            return new FileCompareResult(FileChangeType.Delete, from);
        }
    }
}
