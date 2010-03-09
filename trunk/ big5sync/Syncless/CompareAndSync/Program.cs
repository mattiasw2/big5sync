using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CompareAndSync.Visitor;
using CompareAndSync.CompareObject;

namespace CompareAndSync
{
    class Program
    {
        static void Main(string[] args)
        {
            RootCompareObject rco = new RootCompareObject(new string[] { @"C:\Documents and Settings\Nil\Desktop\Test45", @"C:\Documents and Settings\Nil\Desktop\Test46", @"C:\Documents and Settings\Nil\Desktop\Test47" });
            CompareObjectHelper.PreTraverseFolder(rco, new BuilderVisitor());
            CompareObjectHelper.PreTraverseFolder(rco, new XMLMetadataVisitor());
            CompareObjectHelper.PreTraverseFolder(rco, new ComparerVisitor());
            CompareObjectHelper.PreTraverseFolder(rco, new XMLMetadataVisitor());
            CompareObjectHelper.PreTraverseFolder(rco, new SyncerVisitor());
            CompareObjectHelper.PreTraverseFolder(rco, new XMLWriterVisitor());
            CompareObjectHelper.PreTraverseFolder(rco, new PrinterVisitor());
            Console.Read();
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
