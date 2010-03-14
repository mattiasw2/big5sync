using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.CompareAndSync.CompareObject;
using Syncless.CompareAndSync.Enum;
using Syncless.CompareAndSync.Exceptions;

namespace Syncless.CompareAndSync.Visitor
{
    public class IgnoreMetaDataVisitor : IVisitor
    {
        #region IVisitor Members

        public void Visit(FileCompareObject file, string[] currentPaths)
        {
            for (int i = 0; i < currentPaths.Length; i++)
                file.ChangeType[i] = MetaChangeType.New;
        }

        public void Visit(FolderCompareObject folder, string[] currentPaths)
        {
            for (int i = 0; i < currentPaths.Length; i++)
                folder.ChangeType[i] = MetaChangeType.New;
        }

        public void Visit(RootCompareObject root)
        {
            //Do nothing
        }

        #endregion
    }
}
