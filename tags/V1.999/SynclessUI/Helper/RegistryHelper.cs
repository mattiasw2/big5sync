/*
 * 
 * Author: Steve Teo Wai Ming
 * 
 */

using System.IO;
using Microsoft.Win32;

namespace SynclessUI.Helper
{
    internal static class RegistryHelper
    {
        private static string _appPath;

        #region Registry Creation
        /// <summary>
        /// Modifies the Registry to Enable Syncless Shell Integration, by creating registry keys/values
        /// </summary>
        /// <param name="path">Location of Syncless Executable</param>
        public static void CreateRegistry(string path)
        {
            _appPath = path;

            CreateTagRegistryKey(path);
            CreateUntagRegistryKey(path);
            CreateCleanMetaDataRegistryKey(path);
        }

        /// <summary>
        /// // Creates the Clean Meta Data Registry Entry, only if debug mode is on
        /// </summary>
        /// <param name="path">Location of Syncless Executable</param>
        private static void CreateCleanMetaDataRegistryKey(string path)
        {
            if (CheckDebugModeOn())
            {
                try
                {
                    RegistryKey cleanKey =
                        Registry.CurrentUser.CreateSubKey(@"Software\Classes\Folder\shell\SynclessClean");
                    cleanKey.SetValue(null, "Syncless - Clean");
                }
                catch { }

                try
                {
                    RegistryKey cleanKeyCommand =
                        Registry.CurrentUser.CreateSubKey(@"Software\Classes\Folder\shell\SynclessClean\command");
                    cleanKeyCommand.SetValue(null, path + " -CleanMeta %1");
                }
                catch { }
            }
        }

        /// <summary>
        /// Creates the Untag Registry Entry
        /// </summary>
        /// <param name="path">Location of Syncless Executable</param>
        private static void CreateUntagRegistryKey(string path)
        {
            try
            {
                RegistryKey untagKey = Registry.CurrentUser.CreateSubKey(@"Software\Classes\Folder\shell\SynclessUntag");
                untagKey.SetValue(null, "Syncless - Untag");
            }
            catch { }

            try
            {
                RegistryKey untagKeyCommand =
                    Registry.CurrentUser.CreateSubKey(@"Software\Classes\Folder\shell\SynclessUntag\command");
                untagKeyCommand.SetValue(null, path + " -UTFolder %1");
            }
            catch { }
        }

        /// <summary>
        /// Creates the Tag Registry Entry
        /// </summary>
        /// <param name="path">Location of Syncless Executable</param>
        private static void CreateTagRegistryKey(string path)
        {
            try
            {
                RegistryKey tagKey = Registry.CurrentUser.CreateSubKey(@"Software\Classes\Folder\shell\SynclessTag");
                tagKey.SetValue(null, "Syncless - Tag");
            }
            catch { }

            try
            {
                RegistryKey tagKeyCommand =
                    Registry.CurrentUser.CreateSubKey(@"Software\Classes\Folder\shell\SynclessTag\command");
                tagKeyCommand.SetValue(null, path + " -TFolder %1");
            }
            catch { }
        } 
        #endregion

        /// <summary>
        /// Removes Tag/Untag registry keys and Clean Meta Data if debug mode is on
        /// </summary>
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

        /// <summary>
        /// Checks if the debug file is in the application folder
        /// </summary>
        /// <returns></returns>
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
                    FileInfo fi = new FileInfo(path);
                    exists = fi.Exists;
                } catch {}
            }

            return exists;
        }
    }
}