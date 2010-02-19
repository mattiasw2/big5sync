using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Syncless.Tagging;
using Syncless.CompareAndSync;
using Syncless.Monitor;
using Syncless.Profiling;
using Syncless.Logging;
using System.Diagnostics;
namespace Syncless.Core
{
    internal class SystemLogicLayer :IUIControllerInterface,IMonitorControllerInterface
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





        #region IMonitorControllerInterface Members

        public void HandleFileChange(FileChangeEvent fe)
        {
             
            string logicalOldPath = ProfilingLayer.Instance.ConvertPhysicalToLogical(fe.OldPath.FullName, false);
            if(logicalOldPath == null){
                return;
            }
            List<string> oldLogicalOrigin = TaggingLayer.Instance.RetrieveParentByPath(logicalOldPath);
            List<string> oldPhysicalOrigin = ProfilingLayer.Instance.ConvertAndFilterToPhysical(oldLogicalOrigin);
            MonitorPathPair oldPair = new MonitorPathPair(oldPhysicalOrigin,fe.OldPath.FullName);

            List<string> logicalSimilarPaths = TaggingLayer.Instance.FindSimilarPathForFile(logicalOldPath);
            List<MonitorPathPair> monitorPair = new List<MonitorPathPair>();
            foreach (string logical in logicalSimilarPaths)
            {
                List<string> logicalOrigins = TaggingLayer.Instance.RetrieveParentByPath(logical);
                string physical = ProfilingLayer.Instance.ConvertLogicalToPhysical(logical);
                List<string> physicalOrigins = ProfilingLayer.Instance.ConvertAndFilterToPhysical(logicalOrigins);
                monitorPair.Add(new MonitorPathPair(physicalOrigins,physical));
            }

            List<string> physicalSimilarPaths = ProfilingLayer.Instance.ConvertAndFilterToPhysical(logicalSimilarPaths);
            
            if (fe.Event == EventChangeType.CREATED)
            {
                MonitorSyncRequest syncRequest = new MonitorSyncRequest(oldPair,monitorPair,FileChangeType.Create,false);
                CompareSyncController.Instance.Sync(syncRequest);
            }
            else if (fe.Event == EventChangeType.MODIFIED)
            {
                MonitorSyncRequest syncRequest = new MonitorSyncRequest(oldPair, monitorPair, FileChangeType.Update, false);
                CompareSyncController.Instance.Sync(syncRequest);
            }
            else if (fe.Event == EventChangeType.RENAMED)
            {
                string physicalNewPath = fe.NewPath.FullName;
                string logicalNewPath = ProfilingLayer.Instance.ConvertPhysicalToLogical(physicalNewPath,false);
                List<string> newLogicalOrigin = TaggingLayer.Instance.RetrieveParentByPath(logicalNewPath);
                List<string> newPhysicalOrigin = ProfilingLayer.Instance.ConvertAndFilterToPhysical(newLogicalOrigin);
                MonitorPathPair newPair = new MonitorPathPair(newPhysicalOrigin, physicalNewPath);
                Debug.Assert(logicalNewPath != null);
                MonitorSyncRequest syncRequest = new MonitorSyncRequest(oldPair, newPair, monitorPair, FileChangeType.Rename, false);
                
                TaggingLayer.Instance.RenameFile(logicalOldPath, logicalNewPath);
                CompareSyncController.Instance.Sync(syncRequest);
            }
            
        }

        public void HandleFolderChange(FolderChangeEvent fe)
        {
            /*
            string logicalPath = ProfilingLayer.Instance.ConvertPhysicalToLogical(fe.OldPath.FullName, false);
            if (logicalPath == null)
            {
                return;
            }
            List<string> logicalSimilarPaths = TaggingLayer.Instance.FindSimilarPathForFolder(logicalPath);
            List<string> physicalSimilarPaths = ProfilingLayer.Instance.ConvertAndFilterToPhysical(logicalSimilarPaths);
            if (fe.Event == EventChangeType.CREATED)
            {
                MonitorSyncRequest syncRequest = new MonitorSyncRequest(fe.OldPath.FullName, physicalSimilarPaths, FileChangeType.Create,true);
                CompareSyncController.Instance.Sync(syncRequest);

            }
            else if (fe.Event == EventChangeType.RENAMED)
            {
                string physicalNewPath = fe.NewPath.FullName;
                string logicalNewPath = ProfilingLayer.Instance.ConvertPhysicalToLogical(physicalNewPath, false);
                MonitorSyncRequest syncRequest = new MonitorSyncRequest(fe.OldPath.FullName, physicalSimilarPaths, FileChangeType.Rename,true);
                CompareSyncController.Instance.Sync(syncRequest);
                TaggingLayer.Instance.RenameFolder(logicalPath, logicalNewPath);
            }
             */
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

        #region Logging
        /*
        public Logger GetLogger(string type)
        {
            return LoggingLayer.Instance.GetLogger(type);
        }
        */
        #endregion

        #region IUIControllerInterface Members

        public List<string> GetAllTags()
        {
            List<Tag> tagList= TaggingLayer.Instance.AllTagList;
            List<string> tagNames = new List<string>();
            foreach(Tag t in tagList){
                tagNames.Add(t.TagName);
            }
            return tagNames;
        }
        public TagView GetTag(string tagname)
        {
            Tag t = TaggingLayer.Instance.RetrieveTag(tagname);
            if (t is FolderTag)
            {
                return ConvertToFolderTagView((FolderTag)t);
            }
            else if (t is FileTag)
            {
                return ConvertToFileTagView((FileTag)t);
            }
            else
            {
                return null;
            }

        }

        public bool StartManualSync(string tagname)
        {
            Tag tag = TaggingLayer.Instance.RetrieveTag(tagname);
            List<string> paths = tag.PathStringList;
            List<string> convertedPath = ProfilingLayer.Instance.ConvertAndFilterToPhysical(paths);
            SyncRequest syncRequest = new SyncRequest(convertedPath, false);
            CompareSyncController.Instance.Sync(syncRequest);
            return true;
        }

        public bool DeleteTag(string tagname)
        {
            throw new NotImplementedException();
        }

        public FileTagView CreateFileTag(string tagname)
        {
            return ConvertToFileTagView(TaggingLayer.Instance.CreateFileTag(tagname));
        }

        public FolderTagView CreateFolderTag(string tagname)
        {
            return ConvertToFolderTagView(TaggingLayer.Instance.CreateFolderTag(tagname));
        }

        public FileTagView TagFile(string tagname, FileInfo file)
        {
            return TagFile(tagname, file);
        }
        private FolderTagView ConvertToFolderTagView(FolderTag t)
        {
            FolderTagView view = new FolderTagView(t.TagName, t.LastUpdated);
            List<string> pathList = ProfilingLayer.Instance.ConvertAndFilterToPhysical(t.PathStringList);
            view.PathStringList = pathList;
            view.Created = t.Created;
            view.IsSeamless = t.IsSeamless;
            return view;
        }
        private FileTagView ConvertToFileTagView(FileTag t)
        {
            FileTagView view = new FileTagView(t.TagName, t.LastUpdated);
            List<string> pathList = ProfilingLayer.Instance.ConvertAndFilterToPhysical(t.PathStringList);
            view.PathStringList = pathList;
            view.Created = t.Created;
            view.IsSeamless = t.IsSeamless;
            return view;
        }
        public FolderTagView TagFolder(string tagname, DirectoryInfo folder)
        {
            string path = ProfilingLayer.Instance.ConvertPhysicalToLogical(folder.FullName, true);
            FolderTag tag = TaggingLayer.Instance.TagFolder(path, tagname);
            StartManualSync(tag.TagName);
            return ConvertToFolderTagView(tag);
        }
        public int UntagFile(string tagname, FileInfo file)
        {
            Tag tag = TaggingLayer.Instance.RetrieveTag(tagname);
            string path = ProfilingLayer.Instance.ConvertPhysicalToLogical(file.FullName, true);
            return TaggingLayer.Instance.UntagFile(path, tag.TagName);
        }

        public int UntagFolder(string tagname, DirectoryInfo folder)
        {
            Tag tag = TaggingLayer.Instance.RetrieveTag(tagname);
            string path = ProfilingLayer.Instance.ConvertPhysicalToLogical(folder.FullName, true);
            return TaggingLayer.Instance.UntagFolder(path, tag.TagName);
        }

        public bool MonitorTag(string tagname, bool mode)
        {
            //may need to return a list of error.
            List<string> pathList = new List<string>();
            Tag tag = TaggingLayer.Instance.RetrieveTag(tagname);
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

        public bool SetTagMultiDirectional(string tagname)
        {
            return false;
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

        #endregion
    }
}
