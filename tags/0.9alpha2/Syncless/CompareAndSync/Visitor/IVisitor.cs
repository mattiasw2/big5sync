using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.CompareAndSync.CompareObject;

namespace Syncless.CompareAndSync.Visitor
{
    public interface IVisitor
    {
        void Visit(FileCompareObject file, string[] currentPaths);
        void Visit(FolderCompareObject folder, string[] currentPaths);
        void Visit(RootCompareObject root);
    }
}
