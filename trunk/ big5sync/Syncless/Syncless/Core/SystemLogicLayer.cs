using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

using Syncless.Tagging;
using Syncless.Tagging.Exceptions;
using Syncless.CompareAndSync;
using Syncless.Monitor;
using Syncless.Monitor.DTO;
using Syncless.Profiling;
using Syncless.Logging;
using Syncless.Core.Exceptions;
using Syncless.Helper;
namespace Syncless.Core
{
    internal class SystemLogicLayer :IUIControllerInterface,IMonitorControllerInterface,ICommandLineControllerInterface,IOriginsFinder
    {
        #region Singleton
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

        #endregion

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
            if (tag == null)
            {
                return false;
            }
            bool result = StartManualSync(tag);
            return result;
        }
        /// <summary>
        /// Delete a tag
        /// </summary>
        /// <param name="tagname">Name of the tag to delete</param>
        /// <returns>true if a tag is removed. false if the tag cannot be removed</returns>
        public bool DeleteTag(string tagname)
        {
            try
            {
                Tag t = TaggingLayer.Instance.RemoveTag(tagname);
                return t != null;
            }
            catch (TagNotFoundException te)
            {
                throw te;
            }            
        }
        /// <summary>
        /// Create a Tag.
        /// </summary>
        /// <param name="tagname">name of the tag.</param>
        /// <returns>The Detail of the Tag.</returns>
        public TagView CreateTag(string tagname)
        {
            try
            {
                Tag t = TaggingLayer.Instance.CreateTag(tagname);
                return ConvertToTagView(t);
            }
            catch (TagAlreadyExistsException te)
            {
                throw te;
            }
            
        }
        /// <summary>
        /// Tag a Folder to a name
        /// </summary>
        /// <param name="tagname">name of the tag</param>
        /// <param name="folder">the folder to tag</param>
        /// <exception cref="InvalidPathException">The Path is invalid</exception>
        /// <exception cref="RecursiveDirectoryException">Tagging the folder will cause a recursive during Synchronization.</exception>
        /// <exception cref="PathAlreadyExistsException">The Path already exist in the Tag.</exception>
        /// <returns>the Tag view</returns>
        public TagView Tag(string tagname, DirectoryInfo folder)
        {
            string path = ProfilingLayer.Instance.ConvertPhysicalToLogical(folder.FullName, true);
            if (path == null)
            {
                throw new InvalidPathException(folder.FullName);
            }

            Tag tag = null;
            try
            {
                tag = TaggingLayer.Instance.TagFolder(path, tagname);
            }
            catch (RecursiveDirectoryException re)
            {
                throw re;
            }
            catch (PathAlreadyExistsException pae)
            {
                throw pae;
            }
            if (tag == null)
            {
                return null;
            }
            StartManualSync(tag.TagName);
            MonitorTag(tag.TagName, true);
            return ConvertToTagView(tag);
        }
        /// <summary>
        /// Untag the folder from a tag. 
        /// </summary>
        /// <param name="tagname">The name of the tag</param>
        /// <param name="folder">The folder to untag</param>
        /// <exception cref="TagNotFoundException">The tag is not found.</exception>
        /// <returns>The number of path untag</returns>
        public int Untag(string tagname, DirectoryInfo folder)
        {
            
            string path = ProfilingLayer.Instance.ConvertPhysicalToLogical(folder.FullName, true);
            try
            {
                int count = TaggingLayer.Instance.UntagFolder(path, tagname);
                return count;
            }
            catch (TagNotFoundException tnfe)
            {
                throw tnfe;
            }
            
        }
        /// <summary>
        /// Set the monitor mode for a tag
        /// </summary>
        /// <param name="tagname">tagname to set</param>
        /// <param name="mode">true - set tag to seamless. , false - set tag to manual</param>
        /// <exception cref="TagNotFoundException">If the Tag is not found</exception>
        /// <returns>whether the tag can be changed.</returns>
        public bool MonitorTag(string tagname, bool mode)
        {   
            Tag tag = TaggingLayer.Instance.RetrieveTag(tagname);
            if (tag == null)
            {
                throw new TagNotFoundException(tagname);
            }
            return MonitorTag(tag, mode);
        }
        /// <summary>
        /// Return a list of tag name.
        /// </summary>
        /// <returns>the List containing the name of the tags</returns>
        public List<string> GetAllTags()
        {
            List<Tag> TagList = TaggingLayer.Instance.TagList;
            List<string> tagNames = new List<string>();
            foreach (Tag t in TagList)
            {
                tagNames.Add(t.TagName);
            }
            tagNames.Sort();
            return tagNames;
        }
        /// <summary>
        /// Return a list of tag name that a folder belongs to.
        /// </summary>
        /// <param name="folder">the folder to find.</param>
        /// <returns>the List containing the name of the tags</returns>
        public List<string> GetTags(DirectoryInfo folder)
        {
            string path = ProfilingLayer.Instance.ConvertPhysicalToLogical(folder.FullName, false);
            if (path == null)
            {
                return null;
            }
            List<Tag> tagList = TaggingLayer.Instance.RetrieveTagByPath(path);
            List<string> tagNames = new List<string>();
            foreach (Tag t in tagList)
            {
                tagNames.Add(t.TagName);
            }
            tagNames.Sort();
            return tagNames;
        }
        /// <summary>
        /// Get The detailed Tag Info of a tag.
        /// </summary>
        /// <param name="tagname"></param>
        /// <returns></returns>
        public TagView GetTag(string tagname)
        {
            Tag t = TaggingLayer.Instance.RetrieveTag(tagname);
            if (t == null)
            {
                return null;
            }
            return ConvertToTagView(t);

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
        public bool Initiate(IUIInterface inf)
        {
            this.appPath = inf.getAppPath();
            bool init = Initiate();
            SaveLoadHelper.SaveAll(appPath);
            return init;
        }
        public List<CompareResult> PreviewSync(string tagname)
        {
            Tag Tag = TaggingLayer.Instance.RetrieveTag(tagname);
            List<string> paths = Tag.PathStringList;
            List<string> convertedPath = ProfilingLayer.Instance.ConvertAndFilterToPhysical(paths);
            CompareRequest compareRequest = new CompareRequest(convertedPath);
            return CompareSyncController.Instance.Compare(compareRequest);
        }
        
        
        public bool RenameTag(string oldtagname, string newtagname)
        {
            /*
            try
            {
                TaggingLayer.Instance.RenameTag(oldtagname, newtagname);
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
            */
            throw new NotImplementedException();
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
        #region private methods

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
            DeviceWatcher.Instance.ToString(); //Starts watching for Drive Change
            return true;

        }
        private TagView ConvertToTagView(Tag t)
        {
            TagView view = new TagView(t.TagName, t.LastUpdated);
            List<string> pathList = ProfilingLayer.Instance.ConvertAndFilterToPhysical(t.PathStringList);
            view.PathStringList = pathList;
            view.Created = t.Created;
            view.IsSeamless = t.IsSeamless;
            return view;
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
        #endregion
    }
}
