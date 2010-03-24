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
            RegistryKey tagKey = Registry.CurrentUser.CreateSubKey(@"Software\Classes\Folder\shell\SynclessTag");
            tagKey.SetValue(null, "Syncless - Tag");

            RegistryKey tagKeyCommand = Registry.CurrentUser.CreateSubKey(@"Software\Classes\Folder\shell\SynclessTag\command");
            tagKeyCommand.SetValue(null, path + " -TFolder %1");

            RegistryKey untagKey = Registry.CurrentUser.CreateSubKey(@"Software\Classes\Folder\shell\SynclessUntag");
            untagKey.SetValue(null, "Syncless - Untag");

            RegistryKey untagKeyCommand = Registry.CurrentUser.CreateSubKey(@"Software\Classes\Folder\shell\SynclessUntag\command");
            untagKeyCommand.SetValue(null, path + " -UTFolder %1");

            RegistryKey cleanKey = Registry.CurrentUser.CreateSubKey(@"Software\Classes\Folder\shell\SynclessClean");
            cleanKey.SetValue(null, "Syncless - Clean");

            RegistryKey cleanKeyCommand = Registry.CurrentUser.CreateSubKey(@"Software\Classes\Folder\shell\SynclessClean\command");
            cleanKeyCommand.SetValue(null, path + " -CleanMeta %1");

            // To be removed
            try
            {
                Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\Folder\shell\SClean");
            }
            catch (ArgumentException)
            {
            }

            try
            {
                Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\Folder\shell\Untag");
            }
            catch (ArgumentException)
            {
            }

            try
            {
                Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\Folder\shell\Tag");
            }
            catch (ArgumentException)
            {
            }

        }
        public static void RemoveRegistry()
        {
            try
            {
                Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\Folder\shell\SynclessTag");
                Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\Folder\shell\SynclessUntag");
                Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\Folder\shell\SynclessClean");
            }
            catch (ArgumentException)
            {
            }
        }
    }
}