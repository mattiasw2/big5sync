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
            RootCompareObject rco = new RootCompareObject(new string[] { @"C:\Users\Wysie\Desktop\SyncTest\A", @"C:\Users\Wysie\Desktop\SyncTest\B", @"C:\Users\Wysie\Desktop\SyncTest\C" });
            CompareObjectHelper.PreTraverseFolder(rco, new BuilderVisitor());
            CompareObjectHelper.PreTraverseFolder(rco, new ComparerVisitor());
            CompareObjectHelper.PreTraverseFolder(rco, new SyncerVisitor());
            CompareObjectHelper.PreTraverseFolder(rco, new PrinterVisitor());
            Console.Read();
        }
        class PrinterVisitor : IVisitor
        {

            #region IVisitor Members

            public void Visit(FileCompareObject file, int level, string[] currentPaths)
            {
                for (int i = 0; i < currentPaths.Length; i++)
                {
                    Console.WriteLine(currentPaths[i] + @"\" + file.Name);
                    Console.WriteLine(file.Exists[i]);
                    Console.WriteLine(file.Priority[i]);
                }
            }

            public void Visit(FolderCompareObject folder, int level, string[] currentPaths)
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
