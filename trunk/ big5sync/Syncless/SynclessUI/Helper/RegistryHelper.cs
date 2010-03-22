using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
namespace SynclessUI.Helper
{
    internal static class RegistryHelper
    {
        /// <summary>
        /// Create the context Menu for Syncless
        /// </summary>
        /// <param name="path">The location of Syncless</param>
        public static void CreateRegistry(string path)
        {
            CreateRegistryForFolder(path);
        }

        public static void CreateRegistryForFolder(string path)
        {
            RegistryKey tagKey = Registry.CurrentUser.CreateSubKey(@"Software\Classes\Folder\shell\Tag");
            tagKey.SetValue(null, "Syncless - Tag");

            RegistryKey tagKeyCommand = Registry.CurrentUser.CreateSubKey(@"Software\Classes\Folder\shell\Tag\command");
            tagKeyCommand.SetValue(null, path + " -TFolder %1");

            RegistryKey untagKey = Registry.CurrentUser.CreateSubKey(@"Software\Classes\Folder\shell\Untag");
            untagKey.SetValue(null, "Syncless - Untag");

            RegistryKey untagKeyCommand = Registry.CurrentUser.CreateSubKey(@"Software\Classes\Folder\shell\Untag\command");
            untagKeyCommand.SetValue(null, path + " -UTFolder %1");

            RegistryKey cleanKey = Registry.CurrentUser.CreateSubKey(@"Software\Classes\Folder\shell\SClean");
            cleanKey.SetValue(null, "Syncless - Clean");

            RegistryKey cleanKeyCommand = Registry.CurrentUser.CreateSubKey(@"Software\Classes\Folder\shell\SClean\command");
            cleanKeyCommand.SetValue(null, path + " -CleanMeta %1");

            // To be removed
            try
            {
                Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\Folder\shell\Clean");
            }
            catch (ArgumentException)
            {
            }

        }
        public static void RemoveRegistry()
        {
            Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\Folder\shell\Tag");
            Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\Folder\shell\Untag");
            Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\Folder\shell\SClean");
        }
    }
}