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
            Dictionary<string, BaseCompareObject>.ValueCollection values = root.Contents.Values;
            foreach (BaseCompareObject o in values)
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
            Dictionary<string, BaseCompareObject>.ValueCollection values = folder.Contents.Values;
            foreach (BaseCompareObject o in values)
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
