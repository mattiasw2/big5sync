using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.CompareAndSync
{
    public class FolderCompareResult : CompareResult
    {
        //Folders: Used for delete        
        private FolderCompareResult(FileChangeType changeType, string from) :
            base(changeType, from)
        {
        }

        //Folders: Used for rename/create
        private FolderCompareResult(FileChangeType changeType, string from, string to) :
            base(changeType, from, to)
        {
        }

        public static FolderCompareResult NewFolderCompareResult(string from, string to)
        {
            return new FolderCompareResult(FileChangeType.Create, from, to);
        }

        public static FolderCompareResult RenameFolderCompareResult(string from, string to)
        {
            return new FolderCompareResult(FileChangeType.Rename, from, to);
        }

        public static FolderCompareResult DeleteFolderCompareResult(string from)
        {
            return new FolderCompareResult(FileChangeType.Delete, from);
        }

    }
}
