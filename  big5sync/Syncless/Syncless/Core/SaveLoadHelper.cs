using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Tagging;
using Syncless.Profiling;
namespace Syncless.Core
{
    internal class SaveLoadHelper
    {
        public static void SaveAll(string appPath)
        {
            SaveTagging(appPath);
            SaveProfiling(appPath);
        }

        public static void LoadAll(string appPath)
        {
            LoadTagging(appPath);
            LoadProfiling(appPath);
        }

        #region private methods for Loading Tagging and Profiling.
        private static void LoadTagging(string appPath)
        {
            List<string> locations = new List<string>();
            string rootLocation = appPath + @"\" + TaggingLayer.RELATIVE_TAGGING_ROOT_SAVE_PATH;

            locations.Add(rootLocation);
            TaggingLayer.Instance.Init(locations);
        }

        private static void LoadProfiling(string appPath)
        {
            List<string> locations = new List<string>();
            string rootLocation = appPath + @"\" + ProfilingLayer.RELATIVE_PROFILING_ROOT_SAVE_PATH;

            locations.Add(rootLocation);
            ProfilingLayer.Instance.Init(locations);
        }
        #endregion
        
        #region private methods for Saving of Tagging and Profiling.
        private static void SaveTagging(string appPath)
        {
            List<string> savedLocation = new List<string>();
            string rootPath = appPath + @"\" + TaggingLayer.RELATIVE_TAGGING_ROOT_SAVE_PATH;

            savedLocation.Add(rootPath);
            TaggingLayer.Instance.SaveTo(savedLocation);

        }
        private static void SaveProfiling(string appPath)
        {
            List<string> savedLocation = new List<string>();
            string rootPath = appPath + @"\" + ProfilingLayer.RELATIVE_PROFILING_ROOT_SAVE_PATH;

            savedLocation.Add(rootPath);
            ProfilingLayer.Instance.SaveTo(savedLocation);
        }
        #endregion
    }
}
