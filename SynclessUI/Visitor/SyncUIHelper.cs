using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Syncless.CompareAndSync.Manual.CompareObject;

namespace SynclessUI.Visitor
{
    public class SyncUIHelper
    {
        public static void TraverseFolderHelper(RootCompareObject root, IVisitor visitor)
        {
            visitor.Visit(root);

            Console.WriteLine("zz");

            Dictionary<string, BaseCompareObject>.ValueCollection values = root.Contents.Values;
            FolderCompareObject fco = null;
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

            Console.WriteLine("zz");

            Dictionary<string, BaseCompareObject>.ValueCollection values = folder.Contents.Values;
            FolderCompareObject fco = null;
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
