using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
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
        private ProfilingLayer()
        {

        }
        /// <summary>
        /// Convert a Logical address to a Physical address
        ///    will replace 001:/Lectures with C:/Lectures
        /// </summary>
        /// <param name="path">The Logical Address to be converted</param>
        /// <returns>The Physical Address</returns>
        public string ConvertLogicalToPhysical(string path)
        {
            return null;
        }
        /// <summary>
        /// Convert a Physical address to a Logical address
        ///    will replace C:/Lectures with 001:/Lectures
        /// </summary>
        /// <param name="path">The Physical Address to be converted</param>
        /// <returns>The Logical Address</returns>
        public string ConvertPhysicalToLogical(string path)
        {
            return null;
        }
        /// <summary>
        /// Get the relative path of a path.
        ///    001:/Lectures will return /Lectures
        /// </summary>
        /// <param name="path">The path to process</param>
        /// <returns>The Relative path</returns>
        public string GetRelativePath(string path)
        {
            return null;
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

    }
}
