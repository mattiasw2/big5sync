using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Syncless.Tagging;
using Syncless.Profiling;
using System.IO;
using Syncless.Profiling.Exceptions;
using Syncless.Logging;

namespace Syncless.Core
{
    /// <summary>
    /// SaveLoadHelper help to keep track of all the location that drive and tag profile needs to load from and Init all the layer respectively.
    /// </summary>
    internal class SaveLoadHelper
    {
        /// <summary>
        /// Save the application data to all the place that it is required.
        /// </summary>
        /// <param name="appPath">The path of the current Syncless</param>
        /// <returns>true if the save is successful, false if it does not</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static bool SaveAll(string appPath)
        {
            SaveProfiling(appPath);
            SaveTagging(appPath);
            return true;
        }
        /// <summary>
        /// Load the application from a path
        /// </summary>
        /// <param name="appPath">The path of the current Syncless</param>
        /// <returns>true if the load successful. false if it does not</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static bool LoadAll(string appPath)
        {
            try
            {
                LoadProfiling(appPath);
                LoadTagging(appPath);
            }
            catch (ProfileLoadException)
            {
                ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.UNKNOWN, "Profile Load fail"));
                return false;
            }
            return true;
        }
        /// <summary>
        /// Load the Tagging Profile
        /// </summary>
        /// <param name="appPath">The path of the root of Syncless</param>
        #region private methods for Loading Tagging and Profiling.
        private static void LoadTagging(string appPath)
        {
            //Create the list of location
            //index 0 will be the root location
            //index 1 - n will be the other drive that contains a guid
            List<string> locations = new List<string>();
            string rootLocation = appPath + @"\" + TaggingLayer.RELATIVE_TAGGING_ROOT_SAVE_PATH;

            locations.Add(rootLocation);
            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (DriveInfo d in drives)
            {

                string guid = d.Name + @"\" + @".syncless\guid.id";
                if (File.Exists(guid))
                {
                    //if drive contain guid.
                    string profilingxml = d.Name + @"\" + @".syncless\tagging.xml";
                    if (File.Exists(profilingxml))
                    {
                        locations.Add(profilingxml);
                    }
                }
            }
            //Init
            TaggingLayer.Instance.Init(locations);
        }
        /// <summary>
        /// Load the Drive Profile
        /// </summary>
        /// <param name="appPath">The path of the root of Syncless</param>
        private static void LoadProfiling(string appPath)
        {
            //set the location to load from
            //index 0 is always the root drive.
            List<string> locations = new List<string>();
            string rootLocation = appPath + @"\" + ProfilingLayer.RELATIVE_PROFILING_ROOT_SAVE_PATH;

            locations.Add(rootLocation);
            //add to the list if the drive contains a guid.
            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (DriveInfo d in drives)
            {
                string guid = d.Name + @"\" + @".syncless\guid.id";
                if (File.Exists(guid))
                {
                    //if drive contain guid.
                    string profilingxml = d.Name + @"\" + @".syncless\profiling.xml";
                    if (File.Exists(profilingxml))
                    {
                        locations.Add(profilingxml);
                    }
                }

            }
            //Init the Profiling.
            ProfilingLayer.Instance.Init(locations);
        }
        #endregion

        #region private methods for Saving of Tagging and Profiling.
        /// <summary>
        /// Save the Tagging Layer.
        /// </summary>
        /// <param name="appPath">The root path for Syncless</param>
        private static void SaveTagging(string appPath)
        {
            //Create a list of location
            //index 0 is always the root path.
            List<string> savedLocation = new List<string>();
            string rootPath = appPath + @"\" + TaggingLayer.RELATIVE_TAGGING_ROOT_SAVE_PATH;

            savedLocation.Add(rootPath);

            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (DriveInfo d in drives)
            {
                string guid = d.Name + @"\" + @".syncless\guid.id";
                if (File.Exists(guid))
                {
                    string profilingxml = d.Name + @"\" + @".syncless\tagging.xml";
                    savedLocation.Add(profilingxml);
                }
            }

            TaggingLayer.Instance.SaveTo(savedLocation);

        }
        /// <summary>
        /// Save the Drive Profiles
        /// </summary>
        /// <param name="appPath">The root path for the Syncless</param>
        private static void SaveProfiling(string appPath)
        {
            //Create a list of location
            //index 0 is always the root path.
            List<string> savedLocation = new List<string>();
            string rootPath = appPath + @"\" + ProfilingLayer.RELATIVE_PROFILING_ROOT_SAVE_PATH;

            savedLocation.Add(rootPath);

            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (DriveInfo d in drives)
            {

                string guid = d.Name + @"\" + @".syncless\guid.id";
                if (File.Exists(guid))
                {

                    string profilingxml = d.Name + @"\" + @".syncless\profiling.xml";
                    savedLocation.Add(profilingxml);
                }
            }
            ProfilingLayer.Instance.SaveTo(savedLocation);
        }
        #endregion
    }
}
