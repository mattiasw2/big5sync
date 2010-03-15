using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;

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
using Syncless.CompareAndSync.CompareObject;
namespace Syncless.Core
{
    internal class SystemLogicLayer : IUIControllerInterface, IMonitorControllerInterface, ICommandLineControllerInterface
    {
        #region Singleton
        private static SystemLogicLayer _instance;
        private IUIInterface _userInterface;
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
            _userInterface = null;
        }

        #endregion

        #region IMonitorControllerInterface
        /// <summary>
        /// Handling File Change
        /// </summary>
        /// <param name="fe">File Change Event with all the objects.</param>
        public void HandleFileChange(FileChangeEvent fe)
        {
            try
            {
                if (fe.Event == EventChangeType.CREATED)
                {
                    string logicalAddress = ProfilingLayer.Instance.ConvertPhysicalToLogical(fe.OldPath.FullName, false);
                    List<string> convertedList = FindSimilarSeamlessPathForFile(logicalAddress);
                    if (convertedList.Count == 0)
                        return;
                    List<string> parentList = new List<string>();


                    foreach (string path in convertedList)
                    {
                        FileInfo info = new FileInfo(PathHelper.RemoveTrailingSlash(path));
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
                    List<string> convertedList = FindSimilarSeamlessPathForFile(logicalAddress);
                    if (convertedList.Count == 0)
                        return;
                    List<string> parentList = new List<string>();
                    foreach (string path in convertedList)
                    {
                        FileInfo info = new FileInfo(PathHelper.RemoveTrailingSlash(path));
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
                    string logicalAddress = ProfilingLayer.Instance.ConvertPhysicalToLogical(fe.NewPath.FullName, false);
                    List<string> convertedList = FindSimilarSeamlessPathForFile(logicalAddress);
                    if (convertedList.Count == 0)
                        return;
                    List<string> parentList = new List<string>();
                    foreach (string path in convertedList)
                    {
                        FileInfo info = new FileInfo(PathHelper.RemoveTrailingSlash(path));
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
            catch (Exception e)
            {
                ExceptionHandler.Handle(e);
            }
        }
        /// <summary>
        /// Handling Folder Change
        /// </summary>
        /// <param name="fe">Folder Change Event with all the objects.</param>
        public void HandleFolderChange(FolderChangeEvent fe)
        {
            try
            {
                if (fe.Event == EventChangeType.CREATED)
                {
                    string logicalAddress = ProfilingLayer.Instance.ConvertPhysicalToLogical(fe.OldPath.FullName, false);
                    List<string> convertedList = FindSimilarSeamlessPathForFile(logicalAddress);
                    if (convertedList.Count == 0)
                        return;
                    List<string> parentList = new List<string>();
                    foreach (string path in convertedList)
                    {
                        DirectoryInfo info = new DirectoryInfo(path);
                        string parent = info.Parent.FullName;
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
                    string newLogicalAddress = ProfilingLayer.Instance.ConvertPhysicalToLogical(fe.NewPath.FullName,false);
                    List<string> convertedList = FindSimilarSeamlessPathForFile(logicalAddress);
                    if (convertedList.Count == 0)
                        return;
                    List<string> parentList = new List<string>();
                    foreach (string path in convertedList)
                    {
                        DirectoryInfo info = new DirectoryInfo(path);
                        string parent = info.Parent.FullName;
                        parentList.Add(parent);
                    }
                    List<Tag> tag = TaggingLayer.Instance.RetrieveParentTagByPath(logicalAddress);
                    if (tag.Count == 0)
                    {
                        return;
                    }
                    SyncConfig syncConfig = new SyncConfig(tag[0].ArchiveName, tag[0].ArchiveCount, tag[0].Recycle);
                    AutoSyncRequest request = new AutoSyncRequest(fe.OldPath.Name, fe.NewPath.Name, fe.OldPath.Parent.FullName, parentList, true, AutoSyncRequestType.Rename, syncConfig);
                    CompareAndSyncController.Instance.Sync(request);
                    TaggingLayer.Instance.RenameFolder(logicalAddress, newLogicalAddress);
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.Handle(e);
            }
        }
        /// <summary>
        /// Handling Delete Change
        /// </summary>
        /// <param name="dce">Delete Change Event with all the objects</param>
        public void HandleDeleteChange(DeleteChangeEvent dce)
        {
            try
            {
                if (dce.Event == EventChangeType.DELETED)
                {
                    string logicalAddress = ProfilingLayer.Instance.ConvertPhysicalToLogical(dce.Path.FullName, false);
                    List<string> convertedList = FindSimilarSeamlessPathForFile(logicalAddress);
                    if (convertedList.Count == 0)
                        return;
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
            catch (Exception e)
            {
                ExceptionHandler.Handle(e);
            }
        }
        /// <summary>
        /// Handling Drive Change
        /// </summary>
        /// <param name="dce">Drive Change Event with all the objects</param>
        public void HandleDriveChange(DriveChangeEvent dce)
        {
            try
            {
                if (dce.Type == DriveChangeType.DRIVE_IN)
                {
                    ProfilingLayer.Instance.UpdateDrive(dce.Info);
                    string logical = ProfilingLayer.Instance.GetLogicalIdFromDrive(dce.Info);
                    List<Tag> tagList = TaggingLayer.Instance.RetrieveTagByLogicalId(logical);

                    foreach (Tag t in tagList)
                    {
                        StartMonitorTag(t, t.IsSeamless);
                        //MonitorTag(t, t.IsSeamless);
                    }
                    Merge(dce.Info);
                }
                else
                {
                    ProfilingLayer.Instance.RemoveDrive(dce.Info);
                }
                _userInterface.DriveChanged();
            }
            catch (Exception e)
            {
                ExceptionHandler.Handle(e);
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
                try
                {
                    bool result = StartManualSync(tag);
                    return result;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                return false;
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
                    //SaveLoadHelper.SaveAll(_userInterface.getAppPath());
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
                    //SaveLoadHelper.SaveAll(_userInterface.getAppPath());
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
                MonitorTag(tag.TagName, tag.IsSeamless);
                //SaveLoadHelper.SaveAll(_userInterface.getAppPath());
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
                    List<Tag> tagList = TaggingLayer.Instance.RetrieveTagByPath(path);
                    if (tagList.Count == 0)
                    {
                        try
                        {
                            MonitorLayer.Instance.UnMonitorPath(folder.FullName);
                        }
                        catch (MonitorPathNotFoundException)
                        {
                            //do nothing
                            //in case the path does not exist.
                        }

                    }
                    //SaveLoadHelper.SaveAll(_userInterface.getAppPath());
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
                
                StartMonitorTag(tag, mode);
                //MonitorTag(tag, tag.IsSeamless);
                
                return true;
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
        /// <summary>
        /// Preview a Sync of a Tag
        /// </summary>
        /// <param name="tagName">The name of the tag to preview</param>
        /// <returns>the RootCompareObject</returns>
        public RootCompareObject PreviewSync(string tagName)
        {
            try
            {
                Tag tag = TaggingLayer.Instance.RetrieveTag(tagName, false);
                List<string> paths = tag.FilteredPathListString;
                List<string>[] filteredPaths = ProfilingLayer.Instance.ConvertAndFilter(paths);
                if (filteredPaths[0].Count != 0)
                {
                    ManualCompareRequest request = new ManualCompareRequest(filteredPaths[0].ToArray(), tag.Filters);
                    return CompareAndSyncController.Instance.Compare(request);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.Handle(e);
                throw new UnhandledException(e);
            }

        }
        /// <summary>
        /// Check if the Program can Terminate
        /// </summary>
        /// <returns>true if the program can terminate, false if the program is not ready for termination</returns>
        public bool PrepareForTermination()
        {
            try
            {
                SaveLoadHelper.SaveAll(_userInterface.getAppPath());
                CompareAndSyncController.Instance.PrepareForTermination();
                return true;
            }
            catch (Exception e)
            {
                ExceptionHandler.Handle(e);
                throw new UnhandledException(e);
            }
        }
        /// <summary>
        /// Terminate the program. 
        /// This is to kill the threads created and release the resources.
        /// </summary>
        /// <returns>true if the program successfully terminated , false if it can't be terminated.</returns>
        public bool Terminate()
        {
            try
            {
                DeviceWatcher.Instance.Terminate();
                MonitorLayer.Instance.Terminate();
                return true;
            }
            catch (Exception e)
            {
                ExceptionHandler.Handle(e);
                throw new UnhandledException(e);
            }
        }
        /// <summary>
        /// Initiate the program. This is the first command that needs to be run.
        /// </summary>
        /// <param name="inf"></param>
        /// <returns></returns>
        public bool Initiate(IUIInterface inf)
        {
            try
            {
                this._userInterface = inf;

                bool init = Initiate();
                SaveLoadHelper.SaveAll(_userInterface.getAppPath());
                return init;
            }
            catch (Exception e)
            {
                ExceptionHandler.Handle(e);
                throw new UnhandledException(e);
            }
        }
        /// <summary>
        /// XXXXXXXXXXXXXXXXXXXX NOT IMPLEMENTED XXXXXXXXXXXXXXXXXXXXXXXX
        /// </summary>
        /// <param name="oldtagname"></param>
        /// <param name="newtagname"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Get all the filters for a particular tag.
        /// </summary>
        /// <param name="tagname">the name of the tag.</param>
        /// <returns>the list of filters</returns>
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
        /// <summary>
        /// Call to release a drive so that it can be safety
        /// </summary>
        /// <param name="drive">the Drive to remove</param>
        /// <returns>true if succeess , false if fail.</returns>
        public bool AllowForRemoval(DriveInfo drive)
        {
            try
            {
                try
                {
                    MonitorLayer.Instance.UnMonitorDrive(drive.Name);
                    ProfilingLayer.Instance.RemoveDrive(drive);
                    return true;
                }
                catch (DriveNotFoundException dnfe)
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.Handle(e);
                throw new UnhandledException(e);
            }
        }

        #endregion
        
        #region private methods & delegate
        public delegate void MonitorTagDelegate(Tag t, bool isSeamless);
        public delegate void ManualSyncDelegate(Tag tag);
        public void StartMonitorTag(Tag tag, bool mode)
        {
            MonitorTagDelegate monitordelegate = new MonitorTagDelegate(MonitorTag);
            monitordelegate.BeginInvoke(tag, mode, null, null);
        }
        private void MonitorTag(Tag tag, bool mode)
        {
            
            tag.IsSeamless = mode;
            
            List<string> pathList = new List<string>();
            foreach (TaggedPath path in tag.FilteredPathList)
            {
                pathList.Add(path.PathName);
            }
            List<string> convertedPath = ProfilingLayer.Instance.ConvertAndFilterToPhysical(pathList);
            if (mode)
            {
                StartManualSync(tag.TagName);
                foreach (string path in convertedPath)
                {
                    try
                    {
                        MonitorLayer.Instance.MonitorPath(path);
                    }
                    catch (MonitorPathNotFoundException e)
                    {
                        Console.WriteLine(e.Message);
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
            
            
        }
        private bool Initiate()
        {
            SaveLoadHelper.LoadAll(_userInterface.getAppPath());
            List<Tag> tagList = TaggingLayer.Instance.AllTagList;
            foreach (Tag t in tagList)
            {
                if (t.IsSeamless)
                {
                    StartMonitorTag(t, t.IsSeamless);
                    //MonitorTag(t, t.IsSeamless);
                }
            }
            Console.WriteLine("Bye");
            DeviceWatcher.Instance.ToString(); //Starts watching for Drive Change
            return true;
        }       
        private TagView ConvertToTagView(Tag t)
        {
            TagView view = new TagView(t.TagName, t.LastUpdatedDate);
            List<string> pathList = ProfilingLayer.Instance.ConvertAndFilterToPhysical(t.FilteredPathListString);
            view.PathStringList = pathList;
            view.Created = t.CreatedDate;
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
        private void Merge(DriveInfo drive)
        {
            string profilingPath = PathHelper.FormatFolderPath(drive.RootDirectory.FullName)  + ProfilingLayer.RELATIVE_PROFILING_SAVE_PATH;
            ProfilingLayer.Instance.Merge(profilingPath);

            string taggingPath = PathHelper.FormatFolderPath(drive.RootDirectory.FullName) + TaggingLayer.RELATIVE_TAGGING_SAVE_PATH;
            TaggingLayer.Instance.Merge(taggingPath);

            
        }
        /// <summary>
        /// Find a list of paths of files which share the same parent directories as filePath
        /// </summary>
        /// <param name="filePath">The path to search</param>
        /// <returns>The list of similar paths</returns>
        private List<string> FindSimilarSeamlessPathForFile(string filePath)
        {
            string logicalid = TaggingHelper.GetLogicalID(filePath);
            List<string> pathList = new List<string>();
            List<Tag> matchingTag = TaggingLayer.Instance.RetrieveTagByLogicalId(logicalid);
            FilterChain chain = new FilterChain();
            foreach (Tag tag in matchingTag)
            {
                if (!tag.IsSeamless)
                {
                    continue;
                }
                List<Filter> tempFilters = new List<Filter>();
                tempFilters.Add(new SynclessArchiveFilter(tag.ArchiveName));
                tempFilters.AddRange(tag.Filters);

                string appendedPath;
                string trailingPath = tag.CreateTrailingPath(filePath, false);
                if (trailingPath != null)
                {
                    foreach (TaggedPath p in tag.FilteredPathList)
                    {
                        appendedPath = p.Append(trailingPath);
                        if (!pathList.Contains(appendedPath) && !appendedPath.Equals(filePath))
                        {
                            string physicalAddress = ProfilingLayer.Instance.ConvertLogicalToPhysical(appendedPath);
                            if (physicalAddress != null && chain.ApplyFilter(tempFilters, physicalAddress))
                                pathList.Add(physicalAddress);
                        }
                    }
                }
            }
            return pathList;
        }
        #endregion
        
        #region For TagMerger
        public void AddTagPath(Tag tag, TaggedPath path)
        {
            
            if (tag.IsSeamless)
            {
                StartMonitorTag(tag, tag.IsSeamless);
            }
        }
        public void RemoveTagPath(Tag tag, TaggedPath path)
        {
            if (tag.IsSeamless)
            {
                string converted = ProfilingLayer.Instance.ConvertLogicalToPhysical(path.PathName);
                if (converted != null)
                {
                    MonitorLayer.Instance.UnMonitorPath(converted);
                }
            }
        }
        public void AddTag(Tag tag)
        {
            TaggingLayer.Instance.AddTag(tag);
            StartMonitorTag(tag, tag.IsSeamless);
        }
        #endregion

    }
}
