using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using Syncless.Core;
namespace Syncless.Profiling
{
    internal static class ProfilingGUIDHelper
    {
        #region GUID Generation

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
            ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write("GUID Created at " + path + " .");
            return guidString;
        }
        
        #endregion
    }
}
