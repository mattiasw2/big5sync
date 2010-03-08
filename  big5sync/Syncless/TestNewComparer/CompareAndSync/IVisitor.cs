using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestNewComparer.CompareAndSync
{
    public interface IVisitor
    {
        void Visit(FileCompareObject file, int level,string[] currentPath);
        void Visit(FolderCompareObject folder,int level, string[] currentPath);
        void Visit(RootCompareObject root);
    }
}
