using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Helper
{
    public class PathHelper
    {
        public static string FormatFolderPath(string path)
        {
            if (path.EndsWith("\\"))
            {
                return path.ToLower();
            }
            else
            {
                path += "\\";
                return path.ToLower();
            }
        }

        public static string FormatPath(string path)
        {
            return path.ToLower();
        }

        public static string RemoveTrailingSlash(string path)
        {
            if (path.EndsWith("\\"))
            {
                path = path.Substring(0, path.Length - 1);
            }
            return path;
        }

        public static string AddTrailingSlash(string path)
        {
            if (path.EndsWith("\\"))
            {
                return path;
            }
            else
            {
                path += "\\";
                return path;
            }
        }

        public static bool ContainsIgnoreCase(List<string> pathlist, string path)
        {
            foreach (string p in pathlist)
            {
                if (FormatFolderPath(p).Equals(FormatFolderPath(path)))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool EqualsIgnoreCase(string p1, string p2)
        {
            if (FormatFolderPath(p1).Equals(FormatFolderPath(p2)))
            {
                return true;
            }
            return false;
        }

        public static bool StartsWithIgnoreCase(string childpath, string parentpath)
        {
            return FormatFolderPath(childpath).StartsWith(FormatFolderPath(parentpath));
        }
    }
}
