using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.CompareAndSync.CompareObject;
using Syncless.CompareAndSync.Enum;

namespace Syncless.CompareAndSync.Visitor
{
    public class FolderRenameVisitor : IVisitor
    {

        #region IVisitor Members

        public void Visit(FileCompareObject file, string[] currentPaths)
        {
            //Do nothing
        }

        public void Visit(FolderCompareObject folder, string[] currentPaths)
        {
            DetectFolderRename(folder, currentPaths);
        }

        public void Visit(RootCompareObject root)
        {
            //Do nothing
        }

        #endregion

        private void DetectFolderRename(FolderCompareObject folder, string[] currentPaths)
        {
            //1. If there exists a folder for which meta exists is true and exists is false, it is (aka changeType.delete)
            //highly probable that it is a folder rename
            //2. We check all folders which has the same meta name but different name as the non-existent folder
            //3. If the count is 1, we shall proceed to rename

            FolderCompareObject f = null;

            for (int i = 0; i < currentPaths.Length; i++)
            {
                if (folder.ChangeType[i] == MetaChangeType.Delete)
                {
                    f = folder.Parent.GetRenamedFolder(folder.Name, folder.CreationTime[i], i);

                    if (f != null)
                    {
                        int counter = 0;

                        for (int j = 0; j < f.ChangeType.Length; j++)
                        {
                            if (f.ChangeType[j].HasValue && f.ChangeType[j] == MetaChangeType.New)
                                counter++;
                        }

                        if (counter != 1)
                            return;

                        folder.NewName = f.Name;
                        folder.ChangeType[i] = MetaChangeType.Rename;
                        folder.AncestorOrItselfRenamed = true;
                        folder.Contents = f.Contents;
                        f.Contents = new Dictionary<string, BaseCompareObject>();
                        f.Invalid = true;  //Invalidate the new folder so it will not be processed 
                    }
                }
            }
        }
    }
}
