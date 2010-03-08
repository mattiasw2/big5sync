using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompareAndSync.Visitor
{
    public class CompareResultVisitor : IVisitor
    {
        #region IVisitor Members

        public void Visit(CompareAndSync.CompareObject.FileCompareObject file, int level, string[] currentPaths)
        {
            throw new NotImplementedException();
        }

        public void Visit(CompareAndSync.CompareObject.FolderCompareObject folder, int level, string[] currentPaths)
        {
            throw new NotImplementedException();
        }

        public void Visit(CompareAndSync.CompareObject.RootCompareObject root)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
