using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TestNewComparer.CompareAndSync;

namespace TestNewComparer
{
    class Program
    {
        static void Main(string[] args)
        {
            BuilderVisitor visitor = new BuilderVisitor();
            RootCompareObject rco = new RootCompareObject(new string[3] { @"C:\Documents and Settings\Nil\Desktop\Test45", 
                    @"C:\Documents and Settings\Nil\Desktop\Test46", @"C:\Documents and Settings\Nil\Desktop\Test47" });
            CompareObjectHelper.PreTraverseFolder(rco, visitor);
            CompareObjectHelper.PreTraverseFolder(rco, new PrinterVisitor());
            Console.Read();
        }
        class PrinterVisitor : IVisitor
        {

            #region IVisitor Members

            public void Visit(FileCompareObject file, int level, string[] currentPath)
            {
                
                Console.WriteLine(level+" "+file.Name);

                int[] states = file.State;
                foreach (int i in states)
                {
                    Console.WriteLine(i);
                }
            }

            public void Visit(FolderCompareObject folder, int level, string[] currentPath)
            {
                Console.WriteLine(level+" "+folder.Name);
            }

            public void Visit(RootCompareObject root)
            {
            }

            #endregion
        }
    }
}
