using System.Collections.Generic;
using Syncless.CompareAndSync.CompareObject;
using Syncless.CompareAndSync.Visitor;
using Syncless.Notification;

namespace Syncless.CompareAndSync
{
    public class CompareObjectHelper
    {
        private enum TraverseType
        {
            Post,
            Pre
        }

        public static void LevelOrderTraverseFolder(RootCompareObject root, IVisitor visitor, SyncProgress syncProgress)
        {
            LevelOrderTraverseFolder(root, root.Paths.Length, visitor, syncProgress);
        }

        private static void LevelOrderTraverseFolder(RootCompareObject root, int numOfPaths, IVisitor visitor, SyncProgress syncProgress)
        {
            Queue<BaseCompareObject> levelQueue = new Queue<BaseCompareObject>();
            RootCompareObject rt;
            FolderCompareObject folder = null;

            levelQueue.Enqueue(root);

            while (levelQueue.Count > 0)
            {
                if (syncProgress.State == SyncState.Cancelled)
                    return;

                BaseCompareObject currObj = levelQueue.Dequeue();

                if ((rt = currObj as RootCompareObject) != null)
                    visitor.Visit(rt);
                else if ((folder = currObj as FolderCompareObject) != null)
                    visitor.Visit(folder, numOfPaths);
                else
                    visitor.Visit(currObj as FileCompareObject, numOfPaths);

                Dictionary<string, BaseCompareObject>.ValueCollection values;
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

        public static void PreTraverseFolder(RootCompareObject root, IVisitor visitor, SyncProgress syncProgress)
        {
            TraverseFolderHelper(root, visitor, TraverseType.Pre, syncProgress);
        }

        public static void PostTraverseFolder(RootCompareObject root, IVisitor visitor, SyncProgress syncProgress)
        {
            TraverseFolderHelper(root, visitor, TraverseType.Post, syncProgress);
        }

        private static void TraverseFolderHelper(RootCompareObject root, IVisitor visitor, TraverseType type, SyncProgress syncProgress)
        {
            if (syncProgress.State == SyncState.Cancelled)
                return;

            if (type == TraverseType.Pre)
                visitor.Visit(root);

            Dictionary<string, BaseCompareObject>.ValueCollection values = root.Contents.Values;
            foreach (BaseCompareObject o in values)
            {
                FolderCompareObject fco;
                if ((fco = o as FolderCompareObject) != null)
                    TraverseFolderHelper(fco, root.Paths.Length, visitor, type, syncProgress);
                else
                    visitor.Visit(o as FileCompareObject, root.Paths.Length);
            }

            if (type == TraverseType.Post)
                visitor.Visit(root);
        }

        private static void TraverseFolderHelper(FolderCompareObject folder, int numOfPaths, IVisitor visitor, TraverseType type, SyncProgress syncProgress)
        {
            if (syncProgress.State == SyncState.Cancelled)
                return;

            if (type == TraverseType.Pre)
                visitor.Visit(folder, numOfPaths);

            Dictionary<string, BaseCompareObject>.ValueCollection values = folder.Contents.Values;
            foreach (BaseCompareObject o in values)
            {
                FolderCompareObject fco;
                if ((fco = o as FolderCompareObject) != null)
                    TraverseFolderHelper(fco, numOfPaths, visitor, type, syncProgress);
                else
                    visitor.Visit(o as FileCompareObject, numOfPaths);
            }

            if (type == TraverseType.Post)
                visitor.Visit(folder, numOfPaths);
        }
    }
}
