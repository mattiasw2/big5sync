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
                    //TO BE REMOVED
                    if (o.Name != ".syncless")
                        PreTraverseFolder((FolderCompareObject)o, newCurrentPath, visitor);
                }
                else
                {
                    visitor.Visit((FileCompareObject)o, newCurrentPath);
                }
            }
        }

        public static void PreTraverseFolder(FolderCompareObject folder, string[] currentPath, IVisitor visitor)
        {
            visitor.Visit(folder, currentPath);
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
                    //TO BE REMOVED (HANDLE USING FILTERS)
                    if (o.Name != ".syncless")
                        PreTraverseFolder((FolderCompareObject)o, newCurrentPath, visitor);
                }
                else
                {
                    visitor.Visit((FileCompareObject)o, newCurrentPath);
                }
            }
        }



        public static void PostTraverseFolder(RootCompareObject root, IVisitor visitor)
        {
            
            Dictionary<string, BaseCompareObject>.ValueCollection values = root.Contents.Values;
            foreach (BaseCompareObject o in values)
            {
                string[] newCurrentPath = root.Paths;
                if (o is FolderCompareObject)
                {
                    //TO BE REMOVED
                    if (o.Name != ".syncless")
                        PostTraverseFolder((FolderCompareObject)o, newCurrentPath, visitor);
                }
                else
                {
                    visitor.Visit((FileCompareObject)o, newCurrentPath);
                }
            }
            visitor.Visit(root);
        }

        public static void PostTraverseFolder(FolderCompareObject folder, string[] currentPath, IVisitor visitor)
        {
            
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
                    //TO BE REMOVED (HANDLE USING FILTERS)
                    if (o.Name != ".syncless")
                        PostTraverseFolder((FolderCompareObject)o, newCurrentPath, visitor);
                }
                else
                {
                    visitor.Visit((FileCompareObject)o, newCurrentPath);
                }
            }
            visitor.Visit(folder, currentPath);
        }
    }
}
