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
            try
            {
                RegistryKey tagKey = Registry.CurrentUser.CreateSubKey(@"Software\Classes\Folder\shell\SynclessTag");
                tagKey.SetValue(null, "Syncless - Tag");
            }
            catch
            {
            }

            try
            {
                RegistryKey tagKeyCommand = Registry.CurrentUser.CreateSubKey(@"Software\Classes\Folder\shell\SynclessTag\command");
                tagKeyCommand.SetValue(null, path + " -TFolder %1");
            }
            catch
            {
            }

            try
            {
                RegistryKey untagKey = Registry.CurrentUser.CreateSubKey(@"Software\Classes\Folder\shell\SynclessUntag");
                untagKey.SetValue(null, "Syncless - Untag");
            }
            catch
            {
            }

            try
            {
                RegistryKey untagKeyCommand = Registry.CurrentUser.CreateSubKey(@"Software\Classes\Folder\shell\SynclessUntag\command");
                untagKeyCommand.SetValue(null, path + " -UTFolder %1");
            }
            catch
            {
            }

            try
            {
                RegistryKey cleanKey = Registry.CurrentUser.CreateSubKey(@"Software\Classes\Folder\shell\SynclessClean");
                cleanKey.SetValue(null, "Syncless - Clean");
            }
            catch
            {
            }

            try
            {
                RegistryKey cleanKeyCommand = Registry.CurrentUser.CreateSubKey(@"Software\Classes\Folder\shell\SynclessClean\command");
                cleanKeyCommand.SetValue(null, path + " -CleanMeta %1");
            }
            catch
            {
            }
        }

        public static void RemoveRegistry()
        {
            try
            {
                Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\Folder\shell\SynclessTag");
            }
            catch
            {
            }

            try
            {
                Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\Folder\shell\SynclessUntag");
            }
            catch
            {
            }

            try
            {
                Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\Folder\shell\SynclessClean");
            } catch
            {
            }
        }
    }
}