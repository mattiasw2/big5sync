using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
namespace Syncless.Profiling
{
    public class ProfilingLayer
    {
        private static ProfilingLayer _instance;
        public ProfilingLayer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ProfilingLayer();
                }
                return _instance;
            }
        }
        private Profile _profile;
        private bool _saved;
        private ProfilingLayer()
        {
            _profile = new Profile("Unnamed");
            _saved = false;
        }
        /// <summary>
        /// Convert a Logical address to a Physical address
        ///    will replace 001:/Lectures with C:/Lectures
        /// </summary>
        /// <param name="path">The Logical Address to be converted</param>
        /// <returns>The Physical Address</returns>
        
        public string ConvertLogicalToPhysical(string path)
        {
            Debug.Assert(path != null);
            Debug.Assert(_profile != null);
            return _profile.FindPhysicalFromLogical(path);                
        }
        /// <summary>
        /// Convert a Physical address to a Logical address
        ///    will replace C:/Lectures with 001:/Lectures
        /// </summary>
        /// <param name="path">The Physical Address to be converted</param>
        /// <returns>The Logical Address</returns>
        public string ConvertPhysicalToLogical(string path)
        {
            Debug.Assert(path != null);
            Debug.Assert(_profile != null);
            return _profile.FindLogicalFromPhysical(path);
        }
        /// <summary>
        /// Get the relative path of a path.
        ///    001:/Lectures will return /Lectures
        ///    C:/Lectures will return /Lectures
        ///    C:/ will return /
        ///    path assert not null.
        /// </summary>
        /// <param name="path">The path to process</param>
        /// <returns>The Relative path</returns>
        public string GetRelativePath(string path)
        {
            Debug.Assert(path != null);
            Debug.Assert(path.IndexOf(':') != -1);
            return path.Substring(path.IndexOf(':') + 1);

        }
        /// <summary>
        /// Take in a list of logical address , convert them to physical and return only those that are currently available.
        /// </summary>
        /// <param name="pathList">The list of logical address to convert</param>
        /// <returns>The list of available Physical Address.</returns>
        public List<string> ConvertAndFilterToPhysical(List<string> pathList)
        {
            return null;
        }
        /// <summary>
        /// Get the Logical Id for a Drive.
        /// </summary>
        /// <param name="info">The Drive</param>
        /// <returns>The logical Id Assigned to the drive. null - if drive is not assigned.</returns>
        public string GetLogicalId(DriveInfo info)
        {
            return null;
        }
        public bool LoadProfile(string path, bool merge)
        {
            _saved = true;
            return true;
        }
        public bool SaveProfile()
        {
            _saved = true;
            return true;
        }
        private Profile LoadMapping(string path)
        {
            return null;
        }
        
    }
}
