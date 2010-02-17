using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
namespace Syncless.Profiling
{
    internal static class ProfilingHelper
    {
        public static string ExtractRelativePath(string path)
        {
            Debug.Assert(path != null);
            Debug.Assert(path.IndexOf(':') != -1);
            return path.Substring(path.IndexOf(':') + 1);

        }
        public static string ExtractDriveName(string path)
        {
            Debug.Assert(path != null);
            Debug.Assert(path.IndexOf(':') != -1);
            return path.Substring(0, path.IndexOf(':'));
        }
        public static string ExtractDriveName(DriveInfo driveInfo)
        {
            Debug.Assert(driveInfo != null);
            return ExtractDriveName(driveInfo.RootDirectory.Name);
        }
    }
}
