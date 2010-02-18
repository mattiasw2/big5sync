using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Syncless.Tagging;
using Syncless.CompareAndSync;
using Syncless.Monitor;
using Syncless.Profiling;
using System.Diagnostics;
namespace Syncless.Core
{
    internal class SystemLogicLayer : IUIControllerInterface,IMonitorControllerInterface
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
            return TaggingLayer.Instance.AllTagList;
        }

        public List<Tag> GetAllTags(FileInfo file)
        {
            return null;
            //return TaggingLayer.Instance.RetrieveFileTagByPath(file.FullName);
        }

        public List<Tag> GetAllTags(DirectoryInfo info)
        {
            return null;
            //return TaggingLayer.Instance.RetrieveFolderTagByPath(info.FullName);
        }

        public FileTag CreateFileTag(string tagname)
        {
            return TaggingLayer.Instance.CreateFileTag(tagname);
        }

        public FolderTag CreateFolderTag(string tagname)
        {
            return TaggingLayer.Instance.CreateFolderTag(tagname);
        }

        public FileTag TagFile(string tagname, FileInfo file)
        {
            string path = ProfilingLayer.Instance.ConvertPhysicalToLogical(file.FullName,true);
            return TaggingLayer.Instance.TagFile(path, tagname);
        }

        public FileTag TagFile(FileTag tag, FileInfo file)
        {
            return TagFile(tag.TagName, file);
        }

        public FolderTag TagFolder(string tagname, DirectoryInfo folder)
        {
            string path = ProfilingLayer.Instance.ConvertPhysicalToLogical(folder.FullName, true);
            return TaggingLayer.Instance.TagFolder(path, tagname);
        }

        public FolderTag TagFolder(FolderTag tag, DirectoryInfo folder)
        {
            return TagFolder(tag.TagName, folder);
        }

        public int UntagFile(FileTag tag, FileInfo file)
        {
            string path = ProfilingLayer.Instance.ConvertPhysicalToLogical(file.FullName, true);
            return TaggingLayer.Instance.UntagFile(path, tag.TagName);
        }

        public int UntagFolder(FolderTag tag, DirectoryInfo folder)
        {
            string path = ProfilingLayer.Instance.ConvertPhysicalToLogical(folder.FullName, true);
            return TaggingLayer.Instance.UntagFolder(path, tag.TagName);
        }

        public bool DeleteTag(FolderTag tag)
        {
            FolderTag folderTag =  TaggingLayer.Instance.RemoveFolderTag(tag.TagName);
            if (folderTag != null)
            {
                return true;
            }
            return false;
        }

        public bool DeleteTag(FileTag tag)
        {
            FileTag fileTag = TaggingLayer.Instance.RemoveFileTag(tag.TagName);
            if (fileTag != null)
            {
                return true;
            }
            return false;
        }
        #region DO NOT IMPLEMENT
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
        public bool SetTagBidirectional(FileTag tag)
        {
            return false;
        }

        public bool SetTagBidirectional(FolderTag tag)
        {
            return false;
        }

        #endregion

        public bool StartManualSync(FileTag tagname)
        {
            FileTag fileTag = TaggingLayer.Instance.RetrieveFileTag(tagname.TagName);
            List<string> paths = fileTag.PathStringList;
            List<string> convertedPath = ProfilingLayer.Instance.ConvertAndFilterToPhysical(paths);
            SyncRequest syncRequest = new SyncRequest(convertedPath, false);
            CompareSyncController.Instance.Sync(syncRequest);
            return true;
        }

        public bool StartManualSync(FolderTag tagname)
        {
            FolderTag folderTag = TaggingLayer.Instance.RetrieveFolderTag(tagname.TagName);
            List<string> paths = folderTag.PathStringList;
            
            List<string> convertedPath = ProfilingLayer.Instance.ConvertAndFilterToPhysical(paths);
            SyncRequest syncRequest = new SyncRequest(convertedPath, true);
            return true;
        }

        public bool MonitorTag(Tag tag, bool mode)
        {
            //may need to return a list of error.
            List<string> pathList = new List<string>();
            foreach (TaggedPath path in tag.PathList)
            {
                pathList.Add(path.Path);
            }
            List<string> convertedPath = ProfilingLayer.Instance.ConvertAndFilterToPhysical(pathList);
            if (mode)
            {
                foreach (string path in convertedPath)
                {
                    try
                    {
                        MonitorLayer.Instance.MonitorPath(path);
                    }
                    catch (Exception)
                    {

                    }
                }
            }
            else
            {
                foreach (string path in convertedPath)
                {
                    try
                    {
                        MonitorLayer.Instance.UnMonitorPath(path);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            return true;
        }

        
        public List<CompareResult> PreviewSync(FolderTag tag)
        {
            FolderTag folderTag = TaggingLayer.Instance.RetrieveFolderTag(tag.TagName);
            List<string> paths = folderTag.PathStringList;
            List<string> convertedPath = ProfilingLayer.Instance.ConvertAndFilterToPhysical(paths);
            CompareRequest compareRequest = new CompareRequest(convertedPath, true);
            return CompareSyncController.Instance.Compare(compareRequest);
        }

        public List<CompareResult> PreviewSync(FileTag tag)
        {
            FileTag fileTag = TaggingLayer.Instance.RetrieveFileTag(tag.TagName);
            List<string> paths = fileTag.PathStringList;
            List<string> convertedPath = ProfilingLayer.Instance.ConvertAndFilterToPhysical(paths);
            CompareRequest compareRequest = new CompareRequest(convertedPath, true);
            return CompareSyncController.Instance.Compare(compareRequest);
        }

        public bool PrepareForTermination()
        {
            return true;
        }

        public bool Terminate()
        {
            DeviceWatcher.Instance.Terminate();
            return false;
        }

        public bool Initiate()
        {
            ProfilingLayer.Instance.Init(ProfilingLayer.RELATIVE_PROFILING_ROOT_SAVE_PATH);
            TaggingLayer.Instance.Init(TaggingLayer.RELATIVE_TAGGING_ROOT_SAVE_PATH);
            DeviceWatcher.Instance.ToString();
            return true;
        }

        #endregion



        #region IMonitorControllerInterface Members

        public void HandleFileChange(FileChangeEvent fe)
        {
            string logicalPath = ProfilingLayer.Instance.ConvertPhysicalToLogical(fe.OldPath.FullName, false);
            Debug.Assert(logicalPath != null);
            List<string> logicalSimilarPaths = TaggingLayer.Instance.FindSimilarPathForFile(logicalPath);
            List<string> physicalSimilarPaths = ProfilingLayer.Instance.ConvertAndFilterToPhysical(logicalSimilarPaths);
            if (fe.Event == EventChangeType.CREATED)
            {
                MonitorSyncRequest syncRequest = new MonitorSyncRequest(fe.OldPath.FullName, physicalSimilarPaths, FileChangeType.Create,false);
               
            }
            else if (fe.Event == EventChangeType.MODIFIED)
            {
                MonitorSyncRequest syncRequest = new MonitorSyncRequest(fe.OldPath.FullName, physicalSimilarPaths, FileChangeType.Create, false);

            }
            else if (fe.Event == EventChangeType.RENAMED)
            {
                MonitorSyncRequest syncRequest = new MonitorSyncRequest(fe.OldPath.FullName, physicalSimilarPaths, FileChangeType.Create, false);
            }
        }

        public void HandleFolderChange(FolderChangeEvent fe)
        {
            string logicalPath = ProfilingLayer.Instance.ConvertPhysicalToLogical(fe.OldPath.FullName, false);
            Debug.Assert(logicalPath != null);
            List<string> logicalSimilarPaths = TaggingLayer.Instance.FindSimilarPathForFolder(logicalPath);
            List<string> physicalSimilarPaths = ProfilingLayer.Instance.ConvertAndFilterToPhysical(logicalSimilarPaths);
            if (fe.Event == EventChangeType.CREATED)
            {
                MonitorSyncRequest syncRequest = new MonitorSyncRequest(fe.OldPath.FullName, physicalSimilarPaths, FileChangeType.Create,true);
                CompareSyncController.Instance.Sync(syncRequest);
            }
            else if (fe.Event == EventChangeType.RENAMED)
            {
                MonitorSyncRequest syncRequest = new MonitorSyncRequest(fe.OldPath.FullName, physicalSimilarPaths, FileChangeType.Rename,true);
                CompareSyncController.Instance.Sync(syncRequest);
            }
        }

        public void HandleDriveChange(DriveChangeEvent dce)
        {
            if (dce.Type == DriveChangeType.DRIVE_IN)
            {
                ProfilingLayer.Instance.UpdateDrive(dce.Info);
                string logical = ProfilingLayer.Instance.GetLogicalIdFromDrive(dce.Info);
                List<Tag> tagList = TaggingLayer.Instance.RetrieveTagByLogicalId(logical);
                
                foreach (Tag t in tagList)
                {
                    List<string> rawPaths = t.PathStringList;
                    List<string> paths = ProfilingLayer.Instance.ConvertAndFilterToPhysical(rawPaths);
                    SyncRequest syncRequest = new SyncRequest(paths, (t is FolderTag));
                    CompareSyncController.Instance.Sync(syncRequest);
                }
                List<string> pathList = TaggingLayer.Instance.RetrievePathByLogicalId(logical);

                foreach (string path in pathList)
                {
                    MonitorLayer.Instance.MonitorPath(path);
                }
            }
            else
            {
                
                string logical = ProfilingLayer.Instance.GetLogicalIdFromDrive(dce.Info);
                List<string> pathList = TaggingLayer.Instance.RetrievePathByLogicalId(logical);

                foreach (string path in pathList)
                {
                    MonitorLayer.Instance.MonitorPath(path);
                }
                ProfilingLayer.Instance.RemoveDrive(dce.Info);
            }
        }

        public void HandleDeleteChange(DeleteChangeEvent dce)
        {

        }

        #endregion

        
    }
}
