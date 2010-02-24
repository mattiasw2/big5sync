using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.CompareAndSync
{
    public class FileCompareResult : CompareResult
    {
        private string _newHash = null;

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
        private FileCompareResult(FileChangeType changeType, string from, string to, string newHash) :
            this(changeType, from, to)
        {
            _newHash = newHash;
        }

        public string NewHash
        {
            get
            {
                return _newHash;
            }
        }

        public static FileCompareResult CreateFileCompareResult(string from, string to, string newHash)
        {
            return new FileCompareResult(FileChangeType.Create, from, to, newHash);
        }

        public static FileCompareResult UpdateFileCompareResult(string from, string to, string newHash)
        {
            return new FileCompareResult(FileChangeType.Update, from, to, newHash);
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
