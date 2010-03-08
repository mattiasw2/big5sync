using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Syncless.Tagging;
using Syncless.Tagging.Exceptions;
using Syncless.CompareAndSync;
using Syncless.Monitor;
using Syncless.Profiling;
using Syncless.Logging;
using System.Diagnostics;
using Syncless.Helper;
namespace Syncless.Core
{
    internal class SystemLogicLayer :IUIControllerInterface,IMonitorControllerInterface,ICommandLineControllerInterface,IOriginsFinder
    {
        private static SystemLogicLayer _instance;
        private string appPath = "";
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
            
            Console.WriteLine(fe.OldPath);
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
                MonitorSyncRequest syncRequest = new MonitorSyncRequest(oldPair,monitorPair,FileChangeType.Create,IsFolder.No);
                CompareSyncController.Instance.Sync(syncRequest);
            }
            else if (fe.Event == EventChangeType.MODIFIED)
            {
                MonitorSyncRequest syncRequest = new MonitorSyncRequest(oldPair, monitorPair, FileChangeType.Update, IsFolder.No);
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
                MonitorSyncRequest syncRequest = new MonitorSyncRequest(oldPair, newPair, monitorPair, FileChangeType.Rename, IsFolder.No);
                
                TaggingLayer.Instance.RenameFile(logicalOldPath, logicalNewPath);
                CompareSyncController.Instance.Sync(syncRequest);
            }
            
        }

        public void HandleFolderChange(FolderChangeEvent fe)
        {
            
            string logicalOldPath = ProfilingLayer.Instance.ConvertPhysicalToLogical(fe.OldPath.FullName, false);
            if (logicalOldPath == null)
            {
                return;
            }
            List<string> oldLogicalOrigin = TaggingLayer.Instance.RetrieveParentByPath(logicalOldPath);
            List<string> oldPhysicalOrigin = ProfilingLayer.Instance.ConvertAndFilterToPhysical(oldLogicalOrigin);
            MonitorPathPair oldPair = new MonitorPathPair(oldPhysicalOrigin,fe.OldPath.FullName);

            List<string> logicalSimilarPaths = TaggingLayer.Instance.FindSimilarPathForFolder(logicalOldPath);
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
                MonitorSyncRequest syncRequest = new MonitorSyncRequest(oldPair, monitorPair, FileChangeType.Create, IsFolder.Yes);
                CompareSyncController.Instance.Sync(syncRequest);

            }
            else if (fe.Event == EventChangeType.RENAMED)
            {
                string physicalNewPath = fe.NewPath.FullName;
                string logicalNewPath = ProfilingLayer.Instance.ConvertPhysicalToLogical(physicalNewPath, false);
                List<string> newLogicalOrigin = TaggingLayer.Instance.RetrieveParentByPath(logicalNewPath);
                List<string> newPhysicalOrigin = ProfilingLayer.Instance.ConvertAndFilterToPhysical(newLogicalOrigin);
                MonitorPathPair newPair = new MonitorPathPair(newPhysicalOrigin, physicalNewPath);
                Debug.Assert(logicalNewPath != null);
                
                MonitorSyncRequest syncRequest = new MonitorSyncRequest(oldPair,newPair, monitorPair, FileChangeType.Rename, IsFolder.Yes);
                CompareSyncController.Instance.Sync(syncRequest);
                TaggingLayer.Instance.RenameFolder(logicalOldPath, logicalNewPath);
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
                    SyncRequest syncRequest = new SyncRequest(paths);
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
        
        public Logger GetLogger(string type)
        {
            return LoggingLayer.Instance.GetLogger(type);
        }
        
        #endregion

        #region IUIControllerInterface Members
          

        public bool StartManualSync(string tagname)
        {
            Tag tag = TaggingLayer.Instance.RetrieveTag(tagname);
            return StartManualSync(tag);
        }
        private bool StartManualSync(Tag tag)
        {
            List<string> paths = tag.PathStringList;
            List<string> convertedPath = ProfilingLayer.Instance.ConvertAndFilterToPhysical(paths);
            if (convertedPath.Count != 0)
            {
                SyncRequest syncRequest = new SyncRequest(convertedPath);
                CompareSyncController.Instance.Sync(syncRequest);
            }
            return true;
        }

        public bool DeleteTag(string tagname)
        {
            Tag t = TaggingLayer.Instance.RemoveTag(tagname);
            return t != null;
        }

        public FolderTagView CreateFolderTag(string tagname)
        {
            return ConvertToFolderTagView(TaggingLayer.Instance.CreateFolderTag(tagname));
        }
          
        public FolderTagView TagFolder(string tagname, DirectoryInfo folder)
        {
            string path = ProfilingLayer.Instance.ConvertPhysicalToLogical(folder.FullName, true);
            FolderTag tag = TaggingLayer.Instance.TagFolder(path, tagname);
            StartManualSync(tag.TagName);
            MonitorTag(tag.TagName, true);
            return ConvertToFolderTagView(tag);
        }

        public int UntagFolder(string tagname, DirectoryInfo folder)
        {
            Tag tag = TaggingLayer.Instance.RetrieveTag(tagname);
            string path = ProfilingLayer.Instance.ConvertPhysicalToLogical(folder.FullName, true);
            return TaggingLayer.Instance.UntagFolder(path, tag.TagName);
        }

        public bool MonitorTag(string tagname, bool mode)
        {   
            Tag tag = TaggingLayer.Instance.RetrieveTag(tagname);
            return MonitorTag(tag, mode);
        }
        private void MonitorTag(Tag tag)
        {
            List<string> pathList = new List<string>();
            foreach (TaggedPath path in tag.PathList)
            {
                pathList.Add(path.Path);
            }
            List<string> convertedPath = ProfilingLayer.Instance.ConvertAndFilterToPhysical(pathList);
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
        private bool MonitorTag(Tag tag, bool mode)
        {
            tag.IsSeamless = mode;
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
                        StartManualSync(tag.TagName);
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
            SaveLoadHelper.SaveAll(appPath);
            return true;
        }
        public bool Terminate()
        {
            DeviceWatcher.Instance.Terminate();
            return false;
        }

        private bool Initiate()
        {
            SaveLoadHelper.LoadAll(appPath);
            List<Tag> tagList = TaggingLayer.Instance.AllTagList;
            foreach (Tag t in tagList)
            {
                StartManualSync(t);
                MonitorTag(t);
            }
            CompareSyncController.Instance.Init(this);
            DeviceWatcher.Instance.ToString(); //randomly call a method to start watching folders.
            return true;
        }

        public bool Initiate(UIInterface inf)
        {
            this.appPath = inf.getAppPath();
            bool init = Initiate();
            SaveLoadHelper.SaveAll(appPath);
            return init;
        }

        public List<CompareResult> PreviewSync(FolderTag tag)
        {
            FolderTag folderTag = TaggingLayer.Instance.RetrieveFolderTag(tag.TagName);
            List<string> paths = folderTag.PathStringList;
            List<string> convertedPath = ProfilingLayer.Instance.ConvertAndFilterToPhysical(paths);
            CompareRequest compareRequest = new CompareRequest(convertedPath);
            return CompareSyncController.Instance.Compare(compareRequest);
        }

        public List<string> GetAllFolderTags()
        {
            List<FolderTag> folderTagList = TaggingLayer.Instance.FolderTagList;
            List<string> tagNames = new List<string>();
            foreach (Tag t in folderTagList)
            {
                tagNames.Add(t.TagName);
            }
            tagNames.Sort();
            return tagNames;
        }
        
        public List<string> GetTagsByFolder(DirectoryInfo folder)
        {
            string path = ProfilingLayer.Instance.ConvertPhysicalToLogical(folder.FullName,false);
            if (path == null)
            {
                return null;
            }
            List<FolderTag> tagList = TaggingLayer.Instance.RetrieveFolderTagByPath(path);
            List<string> tagNames = new List<string>();
            foreach (Tag t in tagList)
            {
                tagNames.Add(t.TagName);
            }
            tagNames.Sort();
            return tagNames;
        }
        public List<string> GetAllTags()
        {
            List<Tag> tagList = TaggingLayer.Instance.AllTagList;
            List<string> tagNames = new List<string>();
            foreach (Tag t in tagList)
            {
                tagNames.Add(t.TagName);
            }
            tagNames.Sort();
            return tagNames;
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

        #endregion

        #region IOriginsFinder Members

        public List<string> GetOrigins(string path)
        {
            string logicalOldPath = ProfilingLayer.Instance.ConvertPhysicalToLogical(path,false);
            if (path == null)
            {
                return null;
            }
            List<string> oldLogicalOrigin = TaggingLayer.Instance.RetrieveParentByPath(logicalOldPath);
            List<string> oldPhysicalOrigin = ProfilingLayer.Instance.ConvertAndFilterToPhysical(oldLogicalOrigin);
            return oldPhysicalOrigin;
        }

        #endregion





        #region IUIControllerInterface Members


        public bool RenameTag(string oldtagname, string newtagname)
        {
            try
            {
                TaggingLayer.Instance.RenameFolderTag(oldtagname, newtagname);
            }
            catch (TagNotFoundException)
            {
                return false;
            }
            catch (TagAlreadyExistsException)
            {
                return false;
            }
            return true;
        }

        #endregion
    }
}
