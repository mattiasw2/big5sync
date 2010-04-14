/*
 * 
 * Author: Steve Teo Wai Ming
 * 
 */

using System.Collections.Generic;
using Syncless.CompareAndSync.Manual.CompareObject;
using Syncless.CompareAndSync.Manual.Visitor;

namespace SynclessUI.Visitor
{
    /// <summary>
    /// Helper class to Traverse Files & Folder to visit the tree
    /// </summary>
    public class SyncUIHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <param name="visitor"></param>
        public static void TraverseFolderHelper(RootCompareObject root, IVisitor visitor)
        {
            visitor.Visit(root);

            Dictionary<string, BaseCompareObject>.ValueCollection values = root.Contents.Values;
            FolderCompareObject fco;
            foreach (BaseCompareObject o in values)
            {
                if ((fco = o as FolderCompareObject) != null)
                    TraverseFolderHelper(fco, root.Paths.Length, visitor);
                else
                    visitor.Visit(o as FileCompareObject, root.Paths.Length);
            }
        }

        private static void TraverseFolderHelper(FolderCompareObject folder, int numOfPaths, IVisitor visitor)
        {
            visitor.Visit(folder, numOfPaths);

            Dictionary<string, BaseCompareObject>.ValueCollection values = folder.Contents.Values;
            FolderCompareObject fco;
            foreach (BaseCompareObject o in values)
            {
                if ((fco = o as FolderCompareObject) != null)
                    TraverseFolderHelper(fco, numOfPaths, visitor);
                else
                    visitor.Visit(o as FileCompareObject, numOfPaths);
            }
        }
    }
}
