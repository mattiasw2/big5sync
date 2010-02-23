using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
namespace Syncless.Helper
{
    internal static class RegistryHelper
    {
        /// <summary>
        /// Create the context Menu for Syncless
        /// </summary>
        /// <param name="path">The location of Syncless</param>
        public static void CreateRegistry(string path)
        {
            CreateRegistryForFile(path);
            CreateRegistryForFolder(path);
        }

        private static void CreateRegistryForFile(string path)
        {

            RegistryKey tagKey = Registry.ClassesRoot.CreateSubKey(@"*\shell\Tag");
            tagKey.SetValue(null, "Tag");

            RegistryKey tagKeyCommand = Registry.ClassesRoot.CreateSubKey(@"*\shell\Tag\command");
            tagKeyCommand.SetValue(null, path + " -TFile %1");

            RegistryKey untagKey = Registry.ClassesRoot.CreateSubKey(@"*\shell\Untag");
            untagKey.SetValue(null, "Untag");

            RegistryKey untagKeyCommand = Registry.ClassesRoot.CreateSubKey(@"*\shell\Untag\command");
            untagKeyCommand.SetValue(null, path + " -UTFile %1");

        }

        private static void CreateRegistryForFolder(string path)
        {
            RegistryKey tagKey = Registry.ClassesRoot.CreateSubKey(@"Folder\shell\Tag");
            tagKey.SetValue(null, "Tag");

            RegistryKey tagKeyCommand = Registry.ClassesRoot.CreateSubKey(@"Folder\shell\Tag\command");
            tagKeyCommand.SetValue(null, path + " -TFolder %1");

            RegistryKey untagKey = Registry.ClassesRoot.CreateSubKey(@"Folder\shell\Untag");
            untagKey.SetValue(null, "Untag");

            RegistryKey untagKeyCommand = Registry.ClassesRoot.CreateSubKey(@"Folder\shell\Untag\command");
            untagKeyCommand.SetValue(null, path + " -UTFolder %1");
        }
        /// <summary>
        /// Remove all the Registry Entry
        /// </summary>
        public static void RemoveRegistry()
        {
           
            Registry.ClassesRoot.DeleteSubKeyTree(@"*\shell\Tag");
            Registry.ClassesRoot.DeleteSubKeyTree(@"*\shell\Untag");            
            Registry.ClassesRoot.DeleteSubKeyTree(@"Folder\shell\Tag");
            Registry.ClassesRoot.DeleteSubKeyTree(@"Folder\shell\Untag");
        }
    }
}
