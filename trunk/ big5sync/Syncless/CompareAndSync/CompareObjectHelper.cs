using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CompareAndSync.CompareObject;
using CompareAndSync.Visitor;

namespace CompareAndSync
{
    public class CompareObjectHelper
    {
        public static void PreTraverseFolder(RootCompareObject root, IVisitor visitor)
        {
            visitor.Visit(root);
            List<BaseCompareObject> objectList = root.Contents;
            foreach (BaseCompareObject o in objectList)
            {
                string[] newCurrentPath = root.Paths;
                if (o is FolderCompareObject)
                {
                    PreTraverseFolder((FolderCompareObject)o, 1, newCurrentPath, visitor);
                }
                else
                {
                    visitor.Visit((FileCompareObject)o, 1, newCurrentPath);
                }
            }
        }

        public static void PreTraverseFolder(FolderCompareObject folder, int level, string[] currentPath, IVisitor visitor)
        {
            visitor.Visit(folder, level, currentPath);
            List<BaseCompareObject> objectList = folder.Contents;
            foreach (BaseCompareObject o in objectList)
            {
                string[] newCurrentPath = new string[currentPath.Length];
                for (int i = 0; i < currentPath.Length; i++)
                {
                    newCurrentPath[i] = currentPath[i] + @"\" + folder.Name;
                }
                if (o is FolderCompareObject)
                {
                    PreTraverseFolder((FolderCompareObject)o, level + 1, newCurrentPath, visitor);
                }
                else
                {
                    visitor.Visit((FileCompareObject)o, level + 1, newCurrentPath);
                }
            }
        }
    }
}
