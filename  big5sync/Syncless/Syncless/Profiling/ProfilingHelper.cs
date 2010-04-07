using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
namespace Syncless.Profiling
{
    /// <summary>
    /// ProfilingHelper provides some common methods which may be used by other classes under the 
    /// Profiling namespace
    /// </summary>
    internal static class ProfilingHelper
    {
        /// <summary>
        /// Retrieve the path relative to the drive in the given path
        /// eg. Given path is D:\A\B, return \A\B
        /// </summary>
        /// <param name="path">The path for which the relative path is to be retrieved</param>
        /// <returns>The relative path</returns>
        internal static string ExtractRelativePath(string path)
        {
            Debug.Assert(path != null);
            Debug.Assert(path.IndexOf(':') != -1);
            return path.Substring(path.IndexOf(':') + 1);
        }

        /// <summary>
        /// Retrieve the drive name of the given path
        /// </summary>
        /// <param name="path">The path for which the relative path is to be retrieved</param>
        /// <returns>The drive name</returns>
        internal static string ExtractDriveName(string path)
        {
            Debug.Assert(path != null);
            Debug.Assert(path.IndexOf(':') != -1);
            return path.Substring(0, path.IndexOf(':'));
        }

        /// <summary>
        /// Retrieve the drive name of the path in the given DriveInfo
        /// </summary>
        /// <param name="driveInfo">The DriveInfo that contains the path for which the drive name
        /// is to be retrieved</param>
        /// <returns>The drive name</returns>
        internal static string ExtractDriveName(DriveInfo driveInfo)
        {
            Debug.Assert(driveInfo != null);
            return ExtractDriveName(driveInfo.RootDirectory.Name);
        }
    }
}
