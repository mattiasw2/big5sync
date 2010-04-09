using System.Collections.Generic;
using System.IO;

namespace Syncless.Helper
{
    /// <summary>
    /// Cleaner class to clean the .syncless meta data
    /// </summary>
    public static class Cleaner
    {
        /// <summary>
        /// Delete all the .syncless metadata inside a directory
        /// </summary>
        /// <param name="info">The DirectoryInfo object representing the folder</param>
        /// <returns>the number of folder deleted</returns>
        public static int CleanSynclessMeta(DirectoryInfo info)
        {
            DirectoryInfo[] infos = info.GetDirectories("*", SearchOption.AllDirectories);
            int count = 0;
            foreach (DirectoryInfo i in infos)
            {
                if (i.Name.ToLower().Equals(".syncless"))
                {
                    try
                    {
                        i.Delete(true);
                        count++;
                    }
                    catch (IOException)
                    {
                    }
                }
            }
            return count;
        }
        /// <summary>
        /// Delete all the .syncless metadata inside a directory, ignoring a list of subfolder
        /// </summary>
        /// <param name="info">The DirectoryInfo object representing the folder</param>
        /// <param name="childPaths">The list of child paths to ignore</param>
        /// <returns>the number of folder deleted</returns>
        public static int CleanSynclessMeta(DirectoryInfo info, List<string> childPaths)
        {
            if (!info.Exists)
                return 0;

            try
            {
                DirectoryInfo[] infos = info.GetDirectories("*", SearchOption.AllDirectories);
                int count = 0;

                foreach (DirectoryInfo i in infos)
                {
                    if (i.Name.ToLower().Equals(".syncless"))
                    {
                        bool isChild = false;
                        foreach (string path in childPaths)
                        {
                            if (i.FullName.ToLower().Contains(path.ToLower()))
                            {
                                isChild = true;
                                break;
                            }
                        }
                        if (isChild)
                        {
                            continue;
                        }

                        try
                        {
                            i.Delete(true);
                            count++;
                        }
                        catch (IOException)
                        {
                        }
                    }
                }

                return count;
            }
            catch (DirectoryNotFoundException)
            {
                return 0;
            }

        }
    }
}
