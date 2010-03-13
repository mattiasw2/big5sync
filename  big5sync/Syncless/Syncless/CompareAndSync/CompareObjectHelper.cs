using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.CompareAndSync.CompareObject;
using Syncless.CompareAndSync.Visitor;

namespace Syncless.CompareAndSync
{
    public class CompareObjectHelper
    {
        private enum TraverseType
        {
            Post,
            Pre
        }

        public static void LevelOrderTraverseFolder(RootCompareObject root, IVisitor visitor)
        {
            LevelOrderTraverseFolder(root, root.Paths, visitor);
        }

        private static void LevelOrderTraverseFolder(RootCompareObject root, string[] currentPath, IVisitor visitor)
        {
            Queue<BaseCompareObject> levelQueue = new Queue<BaseCompareObject>();
            BaseCompareObject currObj = null;
            RootCompareObject rt = null;
            FolderCompareObject folder = null;
            Dictionary<string, BaseCompareObject>.ValueCollection values = null;

            levelQueue.Enqueue(root);

            while (levelQueue.Count > 0)
            {
                currObj = levelQueue.Dequeue();

                if ((rt = currObj as RootCompareObject) != null)
                    visitor.Visit(rt);
                else if ((folder = currObj as FolderCompareObject) != null)
                    visitor.Visit(folder, currentPath);
                else
                    visitor.Visit(currObj as FileCompareObject, currentPath);

                if (rt != null)
                {
                    values = rt.Contents.Values;
                    foreach (BaseCompareObject o in values)
                        levelQueue.Enqueue(o);
                }
                else if (folder != null)
                {
                    values = folder.Contents.Values;
                    foreach (BaseCompareObject o in values)
                        levelQueue.Enqueue(o);
                }

            }
        }

        public static void PreTraverseFolder(RootCompareObject root, IVisitor visitor)
        {
            TraverseFolderHelper(root, visitor, TraverseType.Pre);
        }

        private static void PreTraverseFolder(FolderCompareObject folder, string[] currentPath, IVisitor visitor)
        {
            TraverseFolderHelper(folder, currentPath, visitor, TraverseType.Pre);
        }

        public static void PostTraverseFolder(RootCompareObject root, IVisitor visitor)
        {
            TraverseFolderHelper(root, visitor, TraverseType.Post);
        }

        private static void PostTraverseFolder(FolderCompareObject folder, string[] currentPath, IVisitor visitor)
        {
            TraverseFolderHelper(folder, currentPath, visitor, TraverseType.Post);
        }

        private static void TraverseFolderHelper(RootCompareObject root, IVisitor visitor, TraverseType type)
        {
            if (type == TraverseType.Pre)
                visitor.Visit(root);

            Dictionary<string, BaseCompareObject>.ValueCollection values = root.Contents.Values;
            FolderCompareObject fco = null;
            foreach (BaseCompareObject o in values)
            {
                string[] newCurrentPath = root.Paths;
                if ((fco = o as FolderCompareObject) != null)
                    TraverseFolderHelper(fco, newCurrentPath, visitor, type);
                else
                    visitor.Visit(o as FileCompareObject, newCurrentPath);
            }

            if (type == TraverseType.Post)
                visitor.Visit(root);
        }

        private static void TraverseFolderHelper(FolderCompareObject folder, string[] currentPath, IVisitor visitor, TraverseType type)
        {
            if (type == TraverseType.Pre)
                visitor.Visit(folder, currentPath);

            Dictionary<string, BaseCompareObject>.ValueCollection values = folder.Contents.Values;
            FolderCompareObject fco = null;
            foreach (BaseCompareObject o in values)
            {
                string[] newCurrentPath = new string[currentPath.Length];
                for (int i = 0; i < currentPath.Length; i++)
                    newCurrentPath[i] = currentPath[i] + @"\" + folder.Name;
                if ((fco = o as FolderCompareObject) != null)
                    TraverseFolderHelper(fco, newCurrentPath, visitor, type);
                else
                    visitor.Visit(o as FileCompareObject, newCurrentPath);
            }

            if (type == TraverseType.Post)
                visitor.Visit(folder, currentPath);
        }
    }
}
