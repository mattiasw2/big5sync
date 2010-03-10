using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.CompareAndSync.Visitor;
using Syncless.CompareAndSync.CompareObject;

namespace Syncless.CompareAndSync
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                RootCompareObject rco = new RootCompareObject(new string[] { @"C:\Users\Wysie\Desktop\SyncTest\A", @"C:\Users\Wysie\Desktop\SyncTest\B", @"C:\Users\Wysie\Desktop\SyncTest\C" });
                CompareObjectHelper.PreTraverseFolder(rco, new BuilderVisitor());
                CompareObjectHelper.PreTraverseFolder(rco, new XMLMetadataVisitor());
                CompareObjectHelper.PostTraverseFolder(rco, new ComparerVisitor());
                CompareObjectHelper.PreTraverseFolder(rco, new SyncerVisitor());
                CompareObjectHelper.PreTraverseFolder(rco, new XMLWriterVisitor());
                CompareObjectHelper.PreTraverseFolder(rco, new PrinterVisitor());

                if (Console.Read() == 'q')
                    break;
            }
        }
        class PrinterVisitor : IVisitor
        {

            #region IVisitor Members

            public void Visit(FileCompareObject file, string[] currentPaths)
            {
                for (int i = 0; i < currentPaths.Length; i++)
                {
                    Console.WriteLine(file.Name);
                    
                }
            }

            public void Visit(FolderCompareObject folder, string[] currentPaths)
            {
                for (int i = 0; i < currentPaths.Length; i++)
                {
                    Console.WriteLine(currentPaths[i] + @"\" + folder.Name);
                    Console.WriteLine(folder.Exists[i]);
                    Console.WriteLine(folder.Priority[i]);
                }
            }

            public void Visit(RootCompareObject root)
            {
            }

            #endregion
        }
    }
}
