using System.IO;

namespace SynclessUI.Helper
{
    public static class FileHelper
    {
        public static bool IsFile(string path)
        {
            try
            {
                FileInfo fi = new FileInfo(path);
                if(fi.Exists)
                    return true;

                return false;
            } catch
            {
                return false;
            }
        }

        // Credits of http://www.saunalahti.fi/janij/blog/2006-12.html

        public static string GetShortcutTargetFile(string shortcutFilename)
        {
            string extension = System.IO.Path.GetExtension(shortcutFilename);

            if(extension.ToLower() == ".lnk")
            {
                string pathOnly = System.IO.Path.GetDirectoryName(shortcutFilename);
                string filenameOnly = System.IO.Path.GetFileName(shortcutFilename);

                Shell32.Shell shell = new Shell32.ShellClass();
                Shell32.Folder folder = shell.NameSpace(pathOnly);
                Shell32.FolderItem folderItem = folder.ParseName(filenameOnly);
                if (folderItem != null)
                {
                    Shell32.ShellLinkObject link =
                (Shell32.ShellLinkObject)folderItem.GetLink;
                    return link.Path;
                }
            }

            return null; // not found
        }
    }
}