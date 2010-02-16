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
            throw new NotImplementedException();
        }

        public List<Tag> GetAllTags(FileInfo file)
        {
            throw new NotImplementedException();
        }

        public List<Tag> GetAllTags(DirectoryInfo info)
        {
            throw new NotImplementedException();
        }

        public FileTag CreateFileTag(string tagname)
        {
            throw new NotImplementedException();
        }

        public FolderTag CreateFolderTag(string tagname)
        {
            throw new NotImplementedException();
        }

        public FileTag TagFile(string tagname, FileInfo file)
        {
            throw new NotImplementedException();
        }

        public FileTag TagFile(FileTag tag, FileInfo file)
        {
            throw new NotImplementedException();
        }

        public FolderTag TagFolder(string tagname, DirectoryInfo folder)
        {
            throw new NotImplementedException();
        }

        public FolderTag TagFolder(FolderTag tag, DirectoryInfo file)
        {
            throw new NotImplementedException();
        }

        public FileTag UntagFile(FileTag tag, FileInfo file)
        {
            throw new NotImplementedException();
        }

        public FolderTag UntagFolder(FolderTag tag, DirectoryInfo folder)
        {
            throw new NotImplementedException();
        }

        public bool DeleteTag(FolderTag tag)
        {
            throw new NotImplementedException();
        }

        public bool DeleteTag(FileTag tag)
        {
            throw new NotImplementedException();
        }

        public bool DeleteAllTags()
        {
            throw new NotImplementedException();
        }

        public bool DeleteAllTags(FileInfo file)
        {
            throw new NotImplementedException();
        }

        public bool DeleteAllTags(DirectoryInfo folder)
        {
            throw new NotImplementedException();
        }

        public bool StartManualSync(FileTag tagname)
        {
            throw new NotImplementedException();
        }

        public bool StartManualSync(FolderTag tagname)
        {
            throw new NotImplementedException();
        }

        public bool MonitorTag(FileTag tag, bool mode)
        {
            throw new NotImplementedException();
        }

        public bool SetTagBidirectional(FileTag tag)
        {
            throw new NotImplementedException();
        }

        public bool SetTagBidirectional(FolderTag tag)
        {
            throw new NotImplementedException();
        }

        public CompareResult PreviewSync(FolderTag tag)
        {
            throw new NotImplementedException();
        }

        public CompareResult PreviewSync(FileTag tag)
        {
            throw new NotImplementedException();
        }

        public bool PrepareForTermination()
        {
            throw new NotImplementedException();
        }

        public bool Terminate()
        {
            throw new NotImplementedException();
        }

        public bool Initiate()
        {
            throw new NotImplementedException();
        }

        public Tag TagPath(string tagname, string path)
        {
            throw new NotImplementedException();
        }

        #endregion



        #region IMonitorControllerInterface Members

        public void HandleFileChange(FileChangeEvent fe)
        {
            throw new NotImplementedException();
        }

        public void HandleFolderChange(FolderChangeEvent fe)
        {
            throw new NotImplementedException();
        }

        public void HandleDriveChange(DriveChangeEvent dce)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
