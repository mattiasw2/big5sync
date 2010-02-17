using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Syncless.Tagging;
using Syncless.CompareAndSync;
using Syncless.Monitor;
namespace Syncless.Core
{
    public class SystemLogicLayer : IUIControllerInterface,IMonitorControllerInterface
    {
        private static SystemLogicLayer _instance;
        public static SystemLogicLayer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SystemLogicLayer();
                }
                return _instance;
            }
        }
        private SystemLogicLayer()
        {

        }


        #region IUIControllerInterface Members

        public List<Tag> GetAllTags()
        {
            return null;    
        }

        public List<Tag> GetAllTags(FileInfo file)
        {
            return null;
        }

        public List<Tag> GetAllTags(DirectoryInfo info)
        {
            return null;
        }

        public FileTag CreateFileTag(string tagname)
        {
            return null;
        }

        public FolderTag CreateFolderTag(string tagname)
        {
            return null;
        }

        public FileTag TagFile(string tagname, FileInfo file)
        {
            return null;
        }

        public FileTag TagFile(FileTag tag, FileInfo file)
        {
            return null;
        }

        public FolderTag TagFolder(string tagname, DirectoryInfo folder)
        {
            return null;
        }

        public FolderTag TagFolder(FolderTag tag, DirectoryInfo file)
        {
            return null;
        }

        public FileTag UntagFile(FileTag tag, FileInfo file)
        {
            return null;
        }

        public FolderTag UntagFolder(FolderTag tag, DirectoryInfo folder)
        {
            return null;
        }

        public bool DeleteTag(FolderTag tag)
        {
            return false;
        }

        public bool DeleteTag(FileTag tag)
        {
            return false;
        }

        public bool DeleteAllTags()
        {
            return false;
        }

        public bool DeleteAllTags(FileInfo file)
        {
            return false;
        }

        public bool DeleteAllTags(DirectoryInfo folder)
        {
            return false;
        }

        public bool StartManualSync(FileTag tagname)
        {
            return false;
        }

        public bool StartManualSync(FolderTag tagname)
        {
            return false;
        }

        public bool MonitorTag(FileTag tag, bool mode)
        {
            return false;
        }

        public bool SetTagBidirectional(FileTag tag)
        {
            return false;
        }

        public bool SetTagBidirectional(FolderTag tag)
        {
            return false;
        }

        public CompareResult PreviewSync(FolderTag tag)
        {
            return null;
        }

        public CompareResult PreviewSync(FileTag tag)
        {
            return null;
        }

        public bool PrepareForTermination()
        {
            return true;
        }

        public bool Terminate()
        {
            return false;
        }

        public bool Initiate()
        {
            return false;
        }

        public Tag TagPath(string tagname, string path)
        {
            return null;
        }

        #endregion



        #region IMonitorControllerInterface Members

        public void HandleFileChange(FileChangeEvent fe)
        {
            
        }

        public void HandleFolderChange(FolderChangeEvent fe)
        {
            
        }

        public void HandleDriveChange(DriveChangeEvent dce)
        {
            
        }

        #endregion
    }
}
