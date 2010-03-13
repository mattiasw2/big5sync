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
            path = path.ToLower();
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

        public static string RemoveTrailingSlash(string path)
        {
            if (path.EndsWith("\\"))
            {
                path = path.Substring(0, path.Length - 1);
            }
            return path;
        }
    }
}
