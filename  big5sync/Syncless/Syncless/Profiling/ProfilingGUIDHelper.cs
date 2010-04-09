using System;
using System.IO;
using System.Diagnostics;

namespace Syncless.Profiling
{
    /// <summary>
    /// ProfilingGUIDHelper provides some common methods to manipulate GUID which may be used by 
    /// other classes under the Tagging namespace
    /// </summary>
    internal static class ProfilingGUIDHelper
    {
        #region GUID Generation
        /// <summary>
        /// Retrieve the GUID based on the given drive id
        /// </summary>
        /// <param name="driveid">The drive id for which the GUID is to be retrieved</param>
        /// <returns>The GUID that is stored in the GUID file in the given drive, else the GUID created
        /// based on the given drive id</returns>
        internal static string GetGUID(string driveid)
        {
            FileInfo fileInfo = new FileInfo(driveid + ":\\" + ProfilingLayer.RELATIVE_GUID_SAVE_PATH);
            if (fileInfo.Exists)
            {
                return ReadGUID(fileInfo);
            }
            else
            {
                return CreateGUID(fileInfo.FullName);
            }
        }
        
        /// <summary>
        /// Read the GUID from the file that can be opened by the given FileInfo
        /// </summary>
        /// <param name="fileInfo">The FileInfo which contains the file which contains the GUID</param>
        /// <returns>The GUID stored in the file given by FileInfo</returns>
        internal static string ReadGUID(FileInfo fileInfo)
        {
            Debug.Assert(fileInfo.Exists);
            FileStream fs = fileInfo.Open(FileMode.Open);
            StreamReader reader = new StreamReader(fs);
            string guid = reader.ReadLine();
            try
            {
                reader.Close();
            }
            catch (IOException)
            {
            }
            return guid;

        }
        
        /// <summary>
        /// Create a GUID, write the GUID to a file and store the file using the path given
        /// </summary>
        /// <param name="path">The path for which the GUID file is to be saved</param>
        /// <returns>The GUID that is created</returns>
        private static string CreateGUID(string path)
        {
            Guid guid = Guid.NewGuid();
            string guidString = guid.ToString();
            FileInfo fileInfo = new FileInfo(path);
            Debug.Assert(!fileInfo.Exists);

            FileStream fs = null;
            try
            {
                fs = fileInfo.Create();
            }
            catch (DirectoryNotFoundException)
            {
                DirectoryInfo info = Directory.CreateDirectory(fileInfo.Directory.FullName);
                info.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
                fs = fileInfo.Create();
            }

            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine(guidString);
            sw.Flush();
            try
            {
                sw.Close();
            }
            catch (IOException)
            {

            }
            return guidString;
        }
        #endregion
    }
}
