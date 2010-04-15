/*
 * 
 * Author: Steve Teo Wai Ming
 * 
 */

using System.IO;
using Shell32;

namespace SynclessUI.Helper
{
    /// <summary>
    /// Helper class to help the UI to handle the checking of files and folders for the purpose of validation
    /// </summary>
    public static class FileFolderHelper
    {
        /// <summary>
        /// Syncless Folder Types To Detect
        /// </summary>
        private const string SYNCLESS = ".syncless";
        private const string ARCHIVE = "_synclessarchive";
        private const string CONFLICT = "_synclessconflict";

        /// <summary>
        /// Check if a path is a file
        /// </summary>
        /// <param name="path">Path to check for</param>
        /// <returns>True if it is a file, false if it is not a file</returns>
        public static bool IsFile(string path)
        {
            try
            {
                FileInfo fi = new FileInfo(path);
                if (fi.Exists)
                    return true;

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the shortcut link behind the given path
        /// Credits of http://www.saunalahti.fi/janij/blog/2006-12.html
        /// </summary>
        /// <param name="shortcutPath">The path of the shortcut to check up on</param>
        /// <returns>The link of the shortcut</returns>
        public static string GetShortcutTargetFile(string shortcutPath)
        {
            string extension = Path.GetExtension(shortcutPath);

            // Check if the file extension is that of a shortcut file
            if (extension.ToLower() == ".lnk")
            {
                string pathOnly = Path.GetDirectoryName(shortcutPath);
                string filenameOnly = Path.GetFileName(shortcutPath);

                Shell shell = new ShellClass();
                Folder folder = shell.NameSpace(pathOnly);
                FolderItem folderItem = folder.ParseName(filenameOnly);
                if (folderItem != null)
                {
                    ShellLinkObject link =
                        (ShellLinkObject) folderItem.GetLink;
                    return link.Path;
                }
            }

            return null; // not found
        }

        /// <summary>
        /// Check if the given path is a Syncless Folder
        /// </summary>
        /// <param name="path">Path to check for</param>
        /// <returns>True if path is a Syncless Folder; otherwise False</returns>
        public static bool IsSynclessFolder(string path)
        {
            string[] tokens = path.Split(new char[] { '\\' });
            foreach (string token in tokens)
            {
                if (token.ToLower().Equals(SYNCLESS) || token.ToLower().Equals(ARCHIVE) || token.ToLower().Equals(CONFLICT))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check if a path is a CDRom Drive
        /// </summary>
        /// <param name="path">Path to check for</param>
        /// <returns>True if the path is a CD Rom Drive, False if not.</returns>
        public static bool IsCDRomDrive(string path)
        {
            DirectoryInfo d = new DirectoryInfo(path);
            DriveInfo drive = new DriveInfo(d.Root.FullName);
            if (drive.DriveType == DriveType.CDRom)
            {
                return true;
            }

            return false;
        }
    }
}