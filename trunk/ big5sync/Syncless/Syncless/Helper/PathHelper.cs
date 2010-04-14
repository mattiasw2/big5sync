/*
 * 
 * Author: Eric Ng Jun Feng
 * 
 */
using System.Collections.Generic;

namespace Syncless.Helper
{
    /// <summary>
    /// PatHelper class provides operations to manipulate path string values
    /// </summary>
    public class PathHelper
    {
        /// <summary>
        /// Converts a path name to lower case and appends a '\' character to the end of the path name if it
        /// does not exist
        /// </summary>
        /// <param name="path">The string value that represents the path name to be formatted</param>
        /// <returns>the formatted path name</returns>
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

        /// <summary>
        /// Converts a path name to lower case
        /// </summary>
        /// <param name="path">The string value that represents the path name to be formatted</param>
        /// <returns>the formatted path name</returns>
        public static string FormatPath(string path)
        {
            return path.ToLower();
        }

        /// <summary>
        /// Removes the '\' character from the end of the path name if it exists
        /// </summary>
        /// <param name="path">The string value that represents the path name to be formatted</param>
        /// <returns>the formatted path name</returns>
        public static string RemoveTrailingSlash(string path)
        {
            if (path.EndsWith("\\"))
            {
                path = path.Substring(0, path.Length - 1);
            }
            return path;
        }

        /// <summary>
        /// Appends the '\' character to the end of the path name if it does not exists
        /// </summary>
        /// <param name="path">The string value that represents the path name to be formatted</param>
        /// <returns>the formatted path name</returns>
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

        /// <summary>
        /// Checks if the path that is passed as parameter exists in the list of paths that is passed
        /// as parameter
        /// </summary>
        /// <param name="pathlist">The list of strings that represents the list of paths</param>
        /// <param name="path">The string value that represents the path to be searched</param>
        /// <returns>true if the path exists in the list of paths; otherwise, false</returns>
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

        /// <summary>
        /// Checks if two paths that are passed as parameters are equal
        /// </summary>
        /// <param name="p1">The string value that represents the path to be compared</param>
        /// <param name="p2">The string value that represents the path to be compared</param>
        /// <returns>true if the two paths are equal; otherwise, false</returns>
        public static bool EqualsIgnoreCase(string p1, string p2)
        {
            if (FormatFolderPath(p1).Equals(FormatFolderPath(p2)))
            {
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Checks if the path name that is passed as first parameter has parent path that is equal to 
        /// the path name that is passed as second parameter
        /// </summary>
        /// <param name="childpath">The string value that represents the longer path name</param>
        /// <param name="parentpath">The string value that represents the shorter path name</param>
        /// <returns>true if the path name that is passed as first parameter has parent path that is
        /// equal to the path name that is passed as second parameter; otherwise, false</returns>
        public static bool StartsWithIgnoreCase(string childpath, string parentpath)
        {
            return FormatFolderPath(childpath).StartsWith(FormatFolderPath(parentpath));
        }
    }
}
