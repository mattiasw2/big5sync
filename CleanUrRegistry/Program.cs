using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
namespace CleanUrRegistry
{
    class Program
    {
        static void Main(string[] args)
        {
            RemoveRegistry();
        }
        public static void RemoveRegistry()
        {

            Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\*\shell\Tag");
            Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\*\shell\Untag");
            Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\Folder\shell\Tag");
            Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\Folder\shell\Untag");
        }
    }
}
