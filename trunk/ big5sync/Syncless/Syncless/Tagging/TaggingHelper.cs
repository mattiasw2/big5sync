using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using Syncless.Logging;
using Syncless.Helper;

namespace Syncless.Tagging
{
    static class TaggingHelper
    {
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

        internal static string FormatTagName(string tagname)
        {
            return tagname.ToLower();
        }

        internal static string GetLogicalID(string path)
        {
            string[] tokens = path.Split('\\');
            return (tokens[0].TrimEnd(':'));
        }

        internal static long GetCurrentTime()
        {
            return DateTime.Now.Ticks;
        }

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

        internal static int Match(string[] pathTokens, string[] pTokens)
        {
            int trailingIndex = 0;
            for (int i = 0; i < pTokens.Length; i++)
            {
                if (pTokens[i].Equals(pathTokens[i]))
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

        internal static void Logging(string message, params object[] list)
        {
            //to call ServiceLocator.GetLogger();
            Console.WriteLine(string.Format(message, list));
        }
    }
}
