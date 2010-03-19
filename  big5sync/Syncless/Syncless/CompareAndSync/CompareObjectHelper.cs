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
            LevelOrderTraverseFolder(root, root.Paths.Length, visitor);
        }

        private static void LevelOrderTraverseFolder(RootCompareObject root, int numOfPaths, IVisitor visitor)
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
                    visitor.Visit(folder, numOfPaths);
                else
                    visitor.Visit(currObj as FileCompareObject, numOfPaths);

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

        public static void PostTraverseFolder(RootCompareObject root, IVisitor visitor)
        {
            TraverseFolderHelper(root, visitor, TraverseType.Post);
        }

        private static void TraverseFolderHelper(RootCompareObject root, IVisitor visitor, TraverseType type)
        {
            if (type == TraverseType.Pre)
                visitor.Visit(root);

            Dictionary<string, BaseCompareObject>.ValueCollection values = root.Contents.Values;
            FolderCompareObject fco = null;
            foreach (BaseCompareObject o in values)
            {
                if ((fco = o as FolderCompareObject) != null)
                    TraverseFolderHelper(fco, root.Paths.Length, visitor, type);
                else
                    visitor.Visit(o as FileCompareObject, root.Paths.Length);
            }

            if (type == TraverseType.Post)
                visitor.Visit(root);
        }

        private static void TraverseFolderHelper(FolderCompareObject folder, int numOfPaths, IVisitor visitor, TraverseType type)
        {
            if (type == TraverseType.Pre)
                visitor.Visit(folder, numOfPaths);

            Dictionary<string, BaseCompareObject>.ValueCollection values = folder.Contents.Values;
            FolderCompareObject fco = null;
            foreach (BaseCompareObject o in values)
            {
                if ((fco = o as FolderCompareObject) != null)
                    TraverseFolderHelper(fco, numOfPaths, visitor, type);
                else
                    visitor.Visit(o as FileCompareObject, numOfPaths);
            }

            if (type == TraverseType.Post)
                visitor.Visit(folder, numOfPaths);
        }
    }
}
