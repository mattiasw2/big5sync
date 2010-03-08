using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CompareAndSync.CompareObject;

namespace CompareAndSync.Visitor
{
    public class XMLWriterVisitor : IVisitor
    {
        #region IVisitor Members

        public void Visit(FileCompareObject file, int level, string[] currentPath)
        {
            throw new NotImplementedException();
        }

        public void Visit(FolderCompareObject folder, int level, string[] currentPath)
        {
            throw new NotImplementedException();
        }

        public void Visit(RootCompareObject root)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
