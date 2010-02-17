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
            
        }

        public List<Tag> GetAllTags(FileInfo file)
        {
            
        }

        public List<Tag> GetAllTags(DirectoryInfo info)
        {
            
        }

        public FileTag CreateFileTag(string tagname)
        {
            
        }

        public FolderTag CreateFolderTag(string tagname)
        {
            
        }

        public FileTag TagFile(string tagname, FileInfo file)
        {
            
        }

        public FileTag TagFile(FileTag tag, FileInfo file)
        {
            
        }

        public FolderTag TagFolder(string tagname, DirectoryInfo folder)
        {
            
        }

        public FolderTag TagFolder(FolderTag tag, DirectoryInfo file)
        {
            
        }

        public FileTag UntagFile(FileTag tag, FileInfo file)
        {
            
        }

        public FolderTag UntagFolder(FolderTag tag, DirectoryInfo folder)
        {
            
        }

        public bool DeleteTag(FolderTag tag)
        {
            
        }

        public bool DeleteTag(FileTag tag)
        {
            
        }

        public bool DeleteAllTags()
        {
            
        }

        public bool DeleteAllTags(FileInfo file)
        {
            
        }

        public bool DeleteAllTags(DirectoryInfo folder)
        {
            
        }

        public bool StartManualSync(FileTag tagname)
        {
            
        }

        public bool StartManualSync(FolderTag tagname)
        {
            
        }

        public bool MonitorTag(FileTag tag, bool mode)
        {
            
        }

        public bool SetTagBidirectional(FileTag tag)
        {
            
        }

        public bool SetTagBidirectional(FolderTag tag)
        {
            
        }

        public CompareResult PreviewSync(FolderTag tag)
        {
            
        }

        public CompareResult PreviewSync(FileTag tag)
        {
            
        }

        public bool PrepareForTermination()
        {
            
        }

        public bool Terminate()
        {
            
        }

        public bool Initiate()
        {
            
        }

        public Tag TagPath(string tagname, string path)
        {
            
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
