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
using Syncless.Monitor.Exceptions;
using Syncless.Profiling;
using Syncless.Logging;
using Syncless.Core.Exceptions;
using Syncless.Helper;
using Syncless.Filters;
using Syncless.CompareAndSync.Request;
using Syncless.CompareAndSync.Enum;
namespace Syncless.Core
{
    internal class SystemLogicLayer : IUIControllerInterface, IMonitorControllerInterface, ICommandLineControllerInterface, IOriginsFinder
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

        #region IMonitorControllerInterface

        public void HandleFileChange(FileChangeEvent fe)
        {
            if (fe.Event == EventChangeType.CREATED)
            {
                string logicalAddress = ProfilingLayer.Instance.ConvertPhysicalToLogical(fe.OldPath.FullName, false);
                List<string> unconvertedList = TaggingLayer.Instance.FindSimilarSeamlessPathForFile(logicalAddress);
                List<string> convertedList = ProfilingLayer.Instance.ConvertAndFilterToPhysical(unconvertedList);
                List<string> parentList = new List<string>();
                foreach (string path in convertedList)
                {
                    FileInfo info = new FileInfo(path);
                    string parent = info.Directory.FullName;
                    parentList.Add(parent);
                }
                List<Tag> tag = TaggingLayer.Instance.RetrieveParentTagByPath(logicalAddress);
                if (tag.Count == 0)
                {
                    return;
                }
                SyncConfig syncConfig = new SyncConfig(tag[0].ArchiveName, tag[0].ArchiveCount, tag[0].Recycle);
                AutoSyncRequest request = new AutoSyncRequest(fe.OldPath.Name, fe.OldPath.Directory.FullName, parentList, false, AutoSyncRequestType.New, syncConfig);
                CompareAndSyncController.Instance.Sync(request);
            }
            else if (fe.Event == EventChangeType.MODIFIED)
            {
                string logicalAddress = ProfilingLayer.Instance.ConvertPhysicalToLogical(fe.OldPath.FullName, false);
                List<string> unconvertedList = TaggingLayer.Instance.FindSimilarSeamlessPathForFile(logicalAddress);
                List<string> convertedList = ProfilingLayer.Instance.ConvertAndFilterToPhysical(unconvertedList);
                List<string> parentList = new List<string>();
                foreach (string path in convertedList)
                {
                    FileInfo info = new FileInfo(path);
                    string parent = info.Directory.FullName;
                    parentList.Add(parent);
                }
                List<Tag> tag = TaggingLayer.Instance.RetrieveParentTagByPath(logicalAddress);
                if (tag.Count == 0)
                {
                    return;
                }
                SyncConfig syncConfig = new SyncConfig(tag[0].ArchiveName, tag[0].ArchiveCount, tag[0].Recycle);
                AutoSyncRequest request = new AutoSyncRequest(fe.OldPath.Name, fe.OldPath.Directory.FullName, parentList, false, AutoSyncRequestType.Update, syncConfig);
                CompareAndSyncController.Instance.Sync(request);
            }
            else if (fe.Event == EventChangeType.RENAMED)
            {
                string logicalAddress = ProfilingLayer.Instance.ConvertPhysicalToLogical(fe.OldPath.FullName, false);
                List<string> unconvertedList = TaggingLayer.Instance.FindSimilarSeamlessPathForFile(logicalAddress);
                List<string> convertedList = ProfilingLayer.Instance.ConvertAndFilterToPhysical(unconvertedList);
                List<string> parentList = new List<string>();
                foreach (string path in convertedList)
                {
                    FileInfo info = new FileInfo(path);
                    string parent = info.Directory.FullName;
                    parentList.Add(parent);
                }
                List<Tag> tag = TaggingLayer.Instance.RetrieveParentTagByPath(logicalAddress);
                if (tag.Count == 0)
                {
                    return;
                }
                SyncConfig syncConfig = new SyncConfig(tag[0].ArchiveName, tag[0].ArchiveCount, tag[0].Recycle);

                AutoSyncRequest request = new AutoSyncRequest(fe.OldPath.Name, fe.NewPath.Name, fe.OldPath.DirectoryName, parentList, false, AutoSyncRequestType.Rename, syncConfig);
                CompareAndSyncController.Instance.Sync(request);
            }
        }

        public void HandleFolderChange(FolderChangeEvent fe)
        {
            if (fe.Event == EventChangeType.CREATED)
            {
                string logicalAddress = ProfilingLayer.Instance.ConvertPhysicalToLogical(fe.OldPath.FullName, false);
                List<string> unconvertedList = TaggingLayer.Instance.FindSimilarSeamlessPathForFile(logicalAddress);
                List<string> convertedList = ProfilingLayer.Instance.ConvertAndFilterToPhysical(unconvertedList);
                List<string> parentList = new List<string>();
                foreach (string path in convertedList)
                {
                    FileInfo info = new FileInfo(path);
                    string parent = info.Directory.FullName;
                    parentList.Add(parent);
                }
                List<Tag> tag = TaggingLayer.Instance.RetrieveParentTagByPath(logicalAddress);
                if (tag.Count == 0)
                {
                    return;
                }
                SyncConfig syncConfig = new SyncConfig(tag[0].ArchiveName, tag[0].ArchiveCount, tag[0].Recycle);
                AutoSyncRequest request = new AutoSyncRequest(fe.OldPath.Name, fe.OldPath.Parent.FullName, parentList, true, AutoSyncRequestType.New, syncConfig);
                CompareAndSyncController.Instance.Sync(request);
            }
            else if (fe.Event == EventChangeType.RENAMED)
            {
                string logicalAddress = ProfilingLayer.Instance.ConvertPhysicalToLogical(fe.OldPath.FullName, false);
                List<string> unconvertedList = TaggingLayer.Instance.FindSimilarSeamlessPathForFile(logicalAddress);
                List<string> convertedList = ProfilingLayer.Instance.ConvertAndFilterToPhysical(unconvertedList);
                List<string> parentList = new List<string>();
                foreach (string path in convertedList)
                {
                    FileInfo info = new FileInfo(path);
                    string parent = info.Directory.FullName;
                    parentList.Add(parent);
                }
                List<Tag> tag = TaggingLayer.Instance.RetrieveParentTagByPath(logicalAddress);
                if (tag.Count == 0)
                {
                    return;
                }
                SyncConfig syncConfig = new SyncConfig(tag[0].ArchiveName, tag[0].ArchiveCount, tag[0].Recycle);
                AutoSyncRequest request = new AutoSyncRequest(fe.OldPath.Name, fe.NewPath.Name, parentList, true, AutoSyncRequestType.Rename, syncConfig);
                CompareAndSyncController.Instance.Sync(request);
            }
        }
        public void HandleDriveChange(DriveChangeEvent dce)
        {

            if (dce.Type == DriveChangeType.DRIVE_IN)
            {
                ProfilingLayer.Instance.UpdateDrive(dce.Info);
                //string logical = ProfilingLayer.Instance.GetLogicalIdFromDrive(dce.Info);
                //List<Tag> tagList = TaggingLayer.Instance.RetrieveTagByLogicalId(logical);

                //foreach (Tag t in tagList)
                //{
                //    List<string> rawPaths = t.PathStringList;
                //    List<string> paths = ProfilingLayer.Instance.ConvertAndFilterToPhysical(rawPaths);
                //    SyncRequest syncRequest = new SyncRequest(paths);
                //    CompareSyncController.Instance.Sync(syncRequest);
                //}
                //List<string> pathList = TaggingLayer.Instance.RetrievePathByLogicalId(logical);

                //foreach (string path in pathList)
                //{
                //    MonitorLayer.Instance.MonitorPath(path);
                //}
            }
            else
            {

                //string logical = ProfilingLayer.Instance.GetLogicalIdFromDrive(dce.Info);
                //List<string> pathList = TaggingLayer.Instance.RetrievePathByLogicalId(logical);

                //foreach (string path in pathList)
                //{
                //    MonitorLayer.Instance.MonitorPath(path);
                //}
                ProfilingLayer.Instance.RemoveDrive(dce.Info);
            }

        }

        public void HandleDeleteChange(DeleteChangeEvent dce)
        {
            if (dce.Event == EventChangeType.DELETED)
            {
                string logicalAddress = ProfilingLayer.Instance.ConvertPhysicalToLogical(dce.Path.FullName, false);
                List<string> unconvertedList = TaggingLayer.Instance.FindSimilarSeamlessPathForFile(logicalAddress);
                List<string> convertedList = ProfilingLayer.Instance.ConvertAndFilterToPhysical(unconvertedList);
                List<string> parentList = new List<string>();
                foreach (string path in convertedList)
                {
                    FileInfo info = new FileInfo(path);
                    string parent = info.Directory.FullName;
                    parentList.Add(parent);
                }
                List<Tag> tag = TaggingLayer.Instance.RetrieveParentTagByPath(logicalAddress);
                if (tag.Count == 0)
                {
                    return;
                }
                SyncConfig syncConfig = new SyncConfig(tag[0].ArchiveName, tag[0].ArchiveCount, tag[0].Recycle);
                AutoSyncRequest request = new AutoSyncRequest(dce.Path.Name, dce.Path.Parent.FullName, parentList, AutoSyncRequestType.Delete, syncConfig);
                CompareAndSyncController.Instance.Sync(request);
            }
        }

        #endregion

        #region Logging

        public Logger GetLogger(string type)
        {
            return LoggingLayer.Instance.GetLogger(type);
        }

        #endregion

        #region IUIControllerInterface Members

        /// <summary>
        /// Manually Sync a Tag
        /// </summary>
        /// <param name="tagname">tagname of the Tag to Sync</param>
        /// <returns>true if the sync is success.</returns>
        public bool StartManualSync(string tagname)
        {
            try
            {
                Tag tag = TaggingLayer.Instance.RetrieveTag(tagname);
                if (tag == null)
                {
                    return false;
                }
                bool result = StartManualSync(tag);
                return result;
            }
            catch (Exception e) // Handle Unexpected Exception
            {
                ExceptionHandler.Handle(e);
                throw new UnhandledException(e);
            }
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
                try
                {
                    Tag t = TaggingLayer.Instance.DeleteTag(tagname);
                    return t != null;
                }
                catch (TagNotFoundException te)
                {
                    throw te;
                }
            }
            catch (Exception e)// Handle Unexpected Exception
            {
                ExceptionHandler.Handle(e);
                throw new UnhandledException(e);
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
            catch (Exception e) //Handle Unexpected Exception
            {
                ExceptionHandler.Handle(e);
                throw new UnhandledException(e);
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
            try
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
            catch (Exception e) // Handle Unexpected Exception
            {
                ExceptionHandler.Handle(e);
                throw new UnhandledException(e);
            }
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
            try
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
            catch (Exception e)
            {
                ExceptionHandler.Handle(e);
                throw new UnhandledException(e);
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
            try
            {
                Tag tag = TaggingLayer.Instance.RetrieveTag(tagname);
                if (tag == null)
                {
                    throw new TagNotFoundException(tagname);
                }
                return MonitorTag(tag, mode);
            }
            catch (Exception e)
            {
                ExceptionHandler.Handle(e);
                throw new UnhandledException(e);
            }
        }
        /// <summary>
        /// Return a list of tag name.
        /// </summary>
        /// <returns>the List containing the name of the tags</returns>
        public List<string> GetAllTags()
        {
            try
            {
                List<Tag> TagList = TaggingLayer.Instance.TagList;
                List<string> tagNames = new List<string>();
                foreach (Tag t in TagList)
                {
                    if (!t.IsDeleted)
                    {
                        tagNames.Add(t.TagName);
                    }
                }
                tagNames.Sort();
                return tagNames;
            }
            catch (Exception e)// Handle Unexpected Exception
            {
                ExceptionHandler.Handle(e);
                throw new UnhandledException(e);
            }
        }
        /// <summary>
        /// Return a list of tag name that a folder belongs to.
        /// </summary>
        /// <param name="folder">the folder to find.</param>
        /// <returns>the List containing the name of the tags</returns>
        public List<string> GetTags(DirectoryInfo folder)
        {
            try
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
            catch (Exception e)//Handle Unexpected Exception
            {
                ExceptionHandler.Handle(e);
                throw new UnhandledException(e);
            }
        }
        /// <summary>
        /// Get The detailed Tag Info of a tag.
        /// </summary>
        /// <param name="tagname"></param>
        /// <returns></returns>
        public TagView GetTag(string tagname)
        {
            try
            {
                Tag t = TaggingLayer.Instance.RetrieveTag(tagname);
                if (t == null)
                {
                    return null;
                }
                return ConvertToTagView(t);
            }
            catch (Exception e)
            {
                ExceptionHandler.Handle(e);
                throw new UnhandledException(e);
            }
        }

        public bool PrepareForTermination()
        {
            try
            {
                SaveLoadHelper.SaveAll(appPath);
                return true;
            }
            catch (Exception e)
            {
                ExceptionHandler.Handle(e);
                throw new UnhandledException(e);
            }
        }
        public bool Terminate()
        {
            try
            {
                DeviceWatcher.Instance.Terminate();
                return false;
            }
            catch (Exception e)
            {
                ExceptionHandler.Handle(e);
                throw new UnhandledException(e);
            }
        }
        public bool Initiate(IUIInterface inf)
        {
            try
            {
                this.appPath = inf.getAppPath();
                bool init = Initiate();
                SaveLoadHelper.SaveAll(appPath);
                return init;
            }
            catch (Exception e)
            {
                ExceptionHandler.Handle(e);
                throw new UnhandledException(e);
            }
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
        /// <summary>
        /// Update the filterlist of a tag
        /// </summary>
        /// <param name="tagname">tagname of the tag</param>
        /// <param name="filterlist">filterlist</param>
        /// <returns>true if succeed</returns>
        public bool UpdateFilterList(string tagname, List<Filter> filterlist)
        {

            try
            {
                try
                {
                    TaggingLayer.Instance.UpdateFilter(tagname, filterlist);
                    return true;
                }
                catch (TagNotFoundException tnfe)
                {
                    throw tnfe;
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.Handle(e);
                throw new UnhandledException(e);
            }
        }


        public List<Filter> GetAllFilters(String tagname)
        {
            try
            {
                Tag t = TaggingLayer.Instance.RetrieveTag(tagname);
                return t.Filters;

            }
            catch (Exception e)
            {
                ExceptionHandler.Handle(e);
                throw new UnhandledException(e);
            }
        }

        #endregion

        #region IOriginsFinder Members

        public List<string> GetOrigins(string path)
        {
            string logicalOldPath = ProfilingLayer.Instance.ConvertPhysicalToLogical(path, false);
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

        public bool MonitorTag(Tag tag, bool mode)
        {
            tag.IsSeamless = mode;
            List<string> pathList = new List<string>();
            foreach (TaggedPath path in tag.FilteredPathList)
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
                    catch (MonitorPathNotFoundException)
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
                    catch (MonitorPathNotFoundException)
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
                if (t.IsSeamless)
                {
                    MonitorTag(t, t.IsSeamless);
                }
            }

            DeviceWatcher.Instance.ToString(); //Starts watching for Drive Change
            return true;
        }
        private TagView ConvertToTagView(Tag t)
        {
            TagView view = new TagView(t.TagName, t.LastUpdated);
            List<string> pathList = ProfilingLayer.Instance.ConvertAndFilterToPhysical(t.FilteredPathListString);
            view.PathStringList = pathList;
            view.Created = t.Created;
            view.IsSeamless = t.IsSeamless;
            return view;
        }
        private bool StartManualSync(Tag tag)
        {
            List<string> paths = tag.FilteredPathListString;
            List<string>[] filterPaths = ProfilingLayer.Instance.ConvertAndFilter(paths);
            if (filterPaths[0].Count != 0)
            {
                SyncConfig syncConfig = new SyncConfig(tag.ArchiveName, tag.ArchiveCount, tag.Recycle);
                ManualSyncRequest syncRequest = new ManualSyncRequest(filterPaths[0].ToArray(), filterPaths[1].ToArray(), tag.Filters, syncConfig);

                CompareAndSyncController.Instance.Sync(syncRequest);
            }
            return true;
        }

        #endregion

        #region For TargerMerger
        public void AddTagPath(Tag tag, TaggedPath path)
        {
            if (tag.IsSeamless)
            {
                MonitorTag(tag, tag.IsSeamless);
            }
        }
        public void RemoveTagPath(Tag tag, TaggedPath path)
        {
            if (tag.IsSeamless)
            {
                string converted = ProfilingLayer.Instance.ConvertLogicalToPhysical(path.Path);
                if (converted != null)
                {
                    MonitorLayer.Instance.UnMonitorPath(converted);
                }
            }
        }
        #endregion
    }
}
