using System;
using System.IO;
using Microsoft.Win32;

namespace SynclessUI.Helper
{
    internal static class RegistryHelper
    {
        private static string _appPath;

        /// <summary>
        /// Create the context Menu for Syncless
        /// </summary>
        /// <param name="path">The location of Syncless</param>
        public static void CreateRegistry(string path)
        {
            _appPath = path;

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
                RegistryKey tagKeyCommand =
                    Registry.CurrentUser.CreateSubKey(@"Software\Classes\Folder\shell\SynclessTag\command");
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
                RegistryKey untagKeyCommand =
                    Registry.CurrentUser.CreateSubKey(@"Software\Classes\Folder\shell\SynclessUntag\command");
                untagKeyCommand.SetValue(null, path + " -UTFolder %1");
            }
            catch
            {
            }

            if (CheckDebugModeOn())
            {
                try
                {
                    RegistryKey cleanKey =
                        Registry.CurrentUser.CreateSubKey(@"Software\Classes\Folder\shell\SynclessClean");
                    cleanKey.SetValue(null, "Syncless - Clean");
                }
                catch
                {
                }

                try
                {
                    RegistryKey cleanKeyCommand =
                        Registry.CurrentUser.CreateSubKey(@"Software\Classes\Folder\shell\SynclessClean\command");
                    cleanKeyCommand.SetValue(null, path + " -CleanMeta %1");
                }
                catch
                {
                }
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

            if (CheckDebugModeOn())
            {
                try
                {
                    Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\Folder\shell\SynclessClean");
                }
                catch
                {
                }
            }
        }

        public static bool CheckDebugModeOn()
        {
            FileInfo fi1 = null;
            bool exists = false;

            try
            {
                fi1 = new FileInfo(_appPath);
            }
            catch {}

            if(fi1 != null)
            {
                string directoryPath = fi1.DirectoryName;

                string path = directoryPath + "\\debug";

                try
                {
                    var fi = new FileInfo(path);
                    exists = fi.Exists;
                } catch {}
            }

            return exists;
        }
    }
}