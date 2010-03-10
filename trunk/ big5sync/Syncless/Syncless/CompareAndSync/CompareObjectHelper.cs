using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.CompareAndSync.CompareObject;
using Syncless.CompareAndSync.Visitor;

namespace Syncless.CompareAndSync.CompareAndSync
{
    public class CompareObjectHelper
    {
        private enum TraverseType
        {
            Post,
            Pre
        }

        public static void PreTraverseFolder(RootCompareObject root, IVisitor visitor)
        {
            TraverseFolderHelper(root, visitor, TraverseType.Pre);
        }

        public static void PreTraverseFolder(FolderCompareObject folder, string[] currentPath, IVisitor visitor)
        {
            TraverseFolderHelper(folder, currentPath, visitor, TraverseType.Pre);
        }

        public static void PostTraverseFolder(RootCompareObject root, IVisitor visitor)
        {
            TraverseFolderHelper(root, visitor, TraverseType.Post);
        }

        public static void PostTraverseFolder(FolderCompareObject folder, string[] currentPath, IVisitor visitor)
        {
            TraverseFolderHelper(folder, currentPath, visitor, TraverseType.Post);
        }

        private static void TraverseFolderHelper(RootCompareObject root, IVisitor visitor, TraverseType type)
        {
            if (type == TraverseType.Pre)
                visitor.Visit(root);

            Dictionary<string, BaseCompareObject>.ValueCollection values = root.Contents.Values;
            foreach (BaseCompareObject o in values)
            {
                string[] newCurrentPath = root.Paths;
                if (o is FolderCompareObject)
                {
                    //TO BE REMOVED
                    if (o.Name != ".syncless")
                        TraverseFolderHelper((FolderCompareObject)o, newCurrentPath, visitor, type);
                }
                else
                {
                    visitor.Visit((FileCompareObject)o, newCurrentPath);
                }
            }

            if (type == TraverseType.Post)
                visitor.Visit(root);
        }

        private static void TraverseFolderHelper(FolderCompareObject folder, string[] currentPath, IVisitor visitor, TraverseType type)
        {
            if (type == TraverseType.Pre)
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
                        TraverseFolderHelper((FolderCompareObject)o, newCurrentPath, visitor, type);
                }
                else
                {
                    visitor.Visit((FileCompareObject)o, newCurrentPath);
                }
            }

            if (type == TraverseType.Post)
                visitor.Visit(folder, currentPath);
        }
    }
}
