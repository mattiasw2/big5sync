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

        public static void CreateRegistryForFile(string path)
        {


            RegistryKey tagKey = Registry.CurrentUser.CreateSubKey(@"Software\Classes\*\shell\Tag");
            tagKey.SetValue(null, "Tag");

            RegistryKey tagKeyCommand = Registry.CurrentUser.CreateSubKey(@"Software\Classes\*\shell\Tag\command");
            tagKeyCommand.SetValue(null, path + " -TFile %1");

            RegistryKey untagKey = Registry.CurrentUser.CreateSubKey(@"Software\Classes\*\shell\Untag");
            untagKey.SetValue(null, "Untag");

            RegistryKey untagKeyCommand = Registry.CurrentUser.CreateSubKey(@"Software\Classes\*\shell\Untag\command");
            untagKeyCommand.SetValue(null, path + " -UTFile %1");

        }

        public static void CreateRegistryForFolder(string path)
        {
            RegistryKey tagKey = Registry.CurrentUser.CreateSubKey(@"Software\Classes\Folder\shell\Tag");
            tagKey.SetValue(null, "Tag");

            RegistryKey tagKeyCommand = Registry.CurrentUser.CreateSubKey(@"Software\Classes\Folder\shell\Tag\command");
            tagKeyCommand.SetValue(null, path + " -TFolder %1");

            RegistryKey untagKey = Registry.CurrentUser.CreateSubKey(@"Software\Classes\Folder\shell\Untag");
            untagKey.SetValue(null, "Untag");

            RegistryKey untagKeyCommand = Registry.CurrentUser.CreateSubKey(@"Software\Classes\Folder\shell\Untag\command");
            untagKeyCommand.SetValue(null, path + " -UTFolder %1");
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
