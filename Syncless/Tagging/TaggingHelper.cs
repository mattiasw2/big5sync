using System;
using Syncless.Logging;
using Syncless.Helper;
using Syncless.Core;

namespace Syncless.Tagging
{
    /// <summary>
    /// TaggingHelper provides some common methods which may be used by other classes under the 
    /// Tagging namespace
    /// </summary>
    internal static class TaggingHelper
    {
        /// <summary>
        /// Removes the trailing element of a string array if it contains an empty string
        /// </summary>
        /// <param name="tempPathTokens">The string array to be trimed</param>
        /// <returns>the trimed string array if its trailing element contains an empty string;
        /// otherwise, the string array that is passed as parameter</returns>
        internal static string[] TrimEnd(string[] tempPathTokens)
        {
            string[] pathTokens = new string[tempPathTokens.Length - 1];
            if (tempPathTokens[tempPathTokens.Length - 1].Equals(""))
            {
                for (int i = 0; i < pathTokens.Length; i++)
                {
                    pathTokens[i] = tempPathTokens[i];
                }
                return pathTokens;
            }
            return tempPathTokens;
        }

        /// <summary>
        /// Converts a tag name to lower case
        /// </summary>
        /// <param name="tagname">The string value that represents the tag name to be formatted</param>
        /// <returns>the tag name that is converted to lower case</returns>
        internal static string FormatTagName(string tagname)
        {
            return tagname.ToLower();
        }

        /// <summary>
        /// Gets the logical ID of the path that is passed as parameter
        /// </summary>
        /// <param name="path">The string value that represents the path whose logical ID is to be retrieved
        /// </param>
        /// <returns>the logical ID of the path that is passed as parameter</returns>
        internal static string GetLogicalID(string path)
        {
            string[] tokens = path.Split('\\');
            return (tokens[0].TrimEnd(':'));
        }

        /// <summary>
        /// Gets the number of ticks that represents the current date time
        /// </summary>
        /// <returns>the current date time in long format</returns>
        internal static long GetCurrentTime()
        {
            return DateTime.UtcNow.Ticks;
        }

        /// <summary>
        /// Gets the string format of the current date time
        /// </summary>
        /// <returns>the current date time in string format</returns>
        /// <remarks>DD/MM/YY HH:MM:SS</remarks>
        internal static string GetCurrentTimeString()
        {
            DateTime currenttime = DateTime.Now;
            string day = (currenttime.Day < 10) ? ("0" + currenttime.Day.ToString()) : currenttime.Day.ToString();
            string month = (currenttime.Month < 10) ? ("0" + currenttime.Month.ToString()) : currenttime.Month.ToString();
            string year = currenttime.Year.ToString();
            string hour = (currenttime.Hour < 10) ? ("0" + currenttime.Hour.ToString()) : currenttime.Hour.ToString();
            string minute = (currenttime.Minute < 10) ? ("0" + currenttime.Minute.ToString()) : currenttime.Minute.ToString();
            string second = (currenttime.Second < 10) ? ("0" + currenttime.Second.ToString()) : currenttime.Second.ToString();
            string datestring = string.Format("{0}/{1}/{2} {3}:{4}:{5}", day, month, year, hour, minute, second);
            return datestring;
        }

        /// <summary>
        /// Creates a path from a string array starting from the element at position indicated by the trailing
        /// index that is passed as parameter
        /// </summary>
        /// <param name="trailingIndex">The integer value that represents the index of the element of the
        /// string array at which the path is to be created from</param>
        /// <param name="pathTokens">The string array that contains string values which will be used
        /// to create the path string</param>
        /// <returns>the string value that is created from the string array</returns>
        internal static string CreatePath(int trailingIndex, string[] pathTokens)
        {
            string trailingPath = null;
            for (int i = trailingIndex; i < pathTokens.Length - 1; i++)
            {
                trailingPath += (PathHelper.AddTrailingSlash(pathTokens[i]));
            }
            trailingPath += pathTokens[pathTokens.Length - 1];
            return trailingPath;
        }

        /// <summary>
        /// Checks whether a tag contains a tagged path whose full path name is parent path or a child path
        /// of the path that is passed as parameter
        /// </summary>
        /// <param name="tag">The <see cref="Tag">Tag</see> object that represents the tag whose list
        /// of tagged paths is to be checked</param>
        /// <param name="path">The string value that represents the path that is to be used to check</param>
        /// <returns>true if the tag contains a tagged path whose full path name is a parent path or a 
        /// child path of the path that is passed as parameter; otherwise, false</returns>
        internal static bool CheckRecursiveDirectory(Tag tag, string path)
        {
            foreach (TaggedPath p in tag.FilteredPathList)
            {
                if (TrimEnd(path.Split('\\')).Length != TrimEnd(p.PathName.Split('\\')).Length)
                {
                    if (p.PathName.StartsWith(path) || path.StartsWith(p.PathName))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Checks the last position of two string arrays who have the first few elements containing the
        /// same string value
        /// </summary>
        /// <param name="childPathTokens">The string array with same number or less number of string
        /// elements</param>
        /// <param name="parentPathTokens">The string array with same number or more number of string
        /// elements</param>
        /// <returns>the index at which the elements at position before this index are equal in both
        /// string arrays</returns>
        internal static int Match(string[] childPathTokens, string[] parentPathTokens)
        {
            int trailingIndex = 0;
            for (int i = 0; i < parentPathTokens.Length; i++)
            {
                if (parentPathTokens[i].Equals(childPathTokens[i]))
                {
                    trailingIndex++;
                }
                else
                {
                    break;
                }
            }
            return trailingIndex;
        }

        /// <summary>
        /// Logs a message to a log file through <see cref="ServiceLocator.GetLogger">ServiceLocator.GetLogger
        /// </see>
        /// </summary>
        /// <param name="eventType">The <see cref="LogEventType">LogEventType</see> of the log message</param>
        /// <param name="message">The string value that represents the message to be logged</param>
        /// <param name="list">The list of parameters that will replace the format item in the message
        /// string that is passed as parameter</param>
        internal static void Logging(LogEventType eventType, string message, params object[] list)
        {
            ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(eventType, string.Format(message, list)));
        }
    }
}
