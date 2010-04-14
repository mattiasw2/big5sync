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
    /// Helper class to Traverse Files & Folder traverse the whole tree so as to allow the visitor to visit each node
    /// </summary>
    public class SyncUIHelper
    {
        /// <summary>
        /// Helper Method to traverse a rootcompareobject and all foldercompareobjects and filecompareobjects under it
        /// </summary>
        /// <param name="root">RootCompareObject from the previous visitor in order to start the traversal</param>
        /// <param name="visitor">A particular visit to visit the RCOs. In this case, it will be the PreviewVisitor</param>
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

        /// <summary>
        /// Helper Method to traverse a FolderCompareObjects and all foldercompareobjects and filecompareobjects under it
        /// </summary>
        /// <param name="folder">FolderCompareObject to start the traversal</param>
        /// <param name="numOfPaths">No. of paths available</param>
        /// <param name="visitor">A particular visit to visit the RCOs. In this case, it will be the PreviewVisitor</param>
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
