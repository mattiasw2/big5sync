#define DEBUGPATH
using System;
using System.Collections.Generic;
using System.IO;

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
using Syncless.Notification;
using Syncless.Core.View;
using System.Runtime.CompilerServices;
namespace Syncless.Core
{

    internal class SystemLogicLayer : IUIControllerInterface, IMonitorControllerInterface, ICommandLineControllerInterface
    {
        #region Singleton

        private static SystemLogicLayer _instance;
        private IUIInterface _userInterface;
        private NotificationQueue _uiNotification;
        private NotificationQueue _sllNotification;
        private NotificationQueue _uiPriorityNotification;
        private LogicQueueObserver _queueObserver;


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

        public NotificationQueue UiNotification
        {
            get { return _uiNotification; }
        }

        public NotificationQueue SllNotification
        {
            get { return _sllNotification; }
        }

        public NotificationQueue UiPriorityNotification
        {
            get { return _uiPriorityNotification; }

        }

        private SystemLogicLayer()
        {
            _userInterface = null;
            _uiNotification = new NotificationQueue();
            _sllNotification = new NotificationQueue();
            _uiPriorityNotification = new NotificationQueue();
            _pathTable = new PathTable();
        }


        #endregion

        #region PathTable

        private PathTable _pathTable;
        private PathTableReader _reader;
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
                switch (fe.Event)
                {
                    case EventChangeType.CREATED:
                        {
                            HandleFileCreateEvent(fe);
                        }
                        break;
                    case EventChangeType.MODIFIED:
                        {
                            HandleFileModifyEvent(fe);
                        }
                        break;
                    case EventChangeType.RENAMED:
                        {
                            HandleFileRenameEvent(fe);
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
            }
        }

        private void HandleFileRenameEvent(FileChangeEvent fe)
        {
            //Find the logical Address for the old path
            //Do not create the logical address if not found.
            string logicalAddress = ProfilingLayer.Instance.ConvertPhysicalToLogical(fe.NewPath.FullName, false);
            //Get all the tag that contains this logicalAddress inside the tag.
            List<Tag> tag = TaggingLayer.Instance.RetrieveParentTagByPath(logicalAddress);
            //If tag count is 0 return as there is nothing to process.
            if (tag.Count == 0) //
            {
                return;
            }
            //Find the similiar paths for the logical Path
            //The return is physical address
            List<string> convertedList = FindSimilarSeamlessPathForFile(logicalAddress);
            convertedList.Remove(fe.OldPath.FullName);

            //////// Massive Path Table Code /////////////
            for (int i = 0; i < convertedList.Count; i++)
            {
                string dest = convertedList[i];
                if (_pathTable.JustPop(fe.OldPath.FullName, dest, TableType.Rename))
                {
                    convertedList.Remove(dest);
                    i--;
                    continue;
                }
            }
            ///////////////// For each Path in the converted List ///////////////////
            ///////////////////////////////// Create a entry such that source = convertedPath and destination is the original Path ///////////
            ///////////////////////For each other path , create a Source + dest pair //////////
            ///////////////// Create an additional Path Entry for each of the Siblings ////////////////
            for (int i = 0; i < convertedList.Count; i++)
            {
                _pathTable.AddPathPair(convertedList[i], fe.OldPath.FullName, TableType.Rename);
                for (int j = i + 1; j < convertedList.Count; j++)
                {
                    _pathTable.AddPathPair(convertedList[i], convertedList[j], TableType.Rename);
                    _pathTable.AddPathPair(convertedList[j], convertedList[i], TableType.Rename);
                }
            }
            ///////// End of Path Table Code ////////////////
            //For each path in the convertedList , extract the parent Path.
            List<string> parentList = new List<string>();
            foreach (string path in convertedList)
            {
                FileInfo info = new FileInfo(PathHelper.RemoveTrailingSlash(path));
                string parent = info.Directory == null ? "" : info.Directory.FullName;
                parentList.Add(parent);
            }
            //If the parent list if empty , it means that there is nothing to sync and thus return.
            if (parentList.Count == 0)
            {
                return;
            }

            AutoSyncRequest request = new AutoSyncRequest(fe.OldPath.Name, fe.NewPath.Name, fe.OldPath.DirectoryName, parentList, false, AutoSyncRequestType.Rename, SyncConfig.Instance);
            SendAutoRequest(request);
        }

        private void HandleFileModifyEvent(FileChangeEvent fe)
        {
            //Find the logical Address for the old path
            //Do not create the logical address if not found.
            string logicalAddress = ProfilingLayer.Instance.ConvertPhysicalToLogical(fe.OldPath.FullName, false);
            //Get all the tag that contains this logicalAddress inside the tag.
            List<Tag> tag = TaggingLayer.Instance.RetrieveParentTagByPath(logicalAddress);
            //If tag count is 0 return as there is nothing to process.
            if (tag.Count == 0) //
            {
                return;
            }
            //Find the similiar paths for the logical Path
            //The return is physical address
            List<string> convertedList = FindSimilarSeamlessPathForFile(logicalAddress);
            convertedList.Remove(fe.OldPath.FullName);

            //////// Massive Path Table Code /////////////
            for (int i = 0; i < convertedList.Count; i++)
            {
                string dest = convertedList[i];
                if (_pathTable.JustPop(fe.OldPath.FullName, dest, TableType.Update))
                {
                    convertedList.Remove(dest);
                    i--;
                    continue;
                }
            }
            ///////////////// For each Path in the converted List ///////////////////
            ///////////////////////////////// Create a entry such that source = convertedPath and destination is the original Path ///////////
            ///////////////////////For each other path , create a Source + dest pair //////////
            ///////////////// Create an additional Path Entry for each of the Siblings ////////////////
            for (int i = 0; i < convertedList.Count; i++)
            {
                _pathTable.AddPathPair(convertedList[i], fe.OldPath.FullName, TableType.Update);
                for (int j = i + 1; j < convertedList.Count; j++)
                {
                    _pathTable.AddPathPair(convertedList[i], convertedList[j], TableType.Update);
                    _pathTable.AddPathPair(convertedList[j], convertedList[i], TableType.Update);
                }
            }
            ///////// End of Path Table Code ////////////////
            //For each path in the convertedList , extract the parent Path.
            List<string> parentList = new List<string>();
            foreach (string path in convertedList)
            {
                FileInfo info = new FileInfo(PathHelper.RemoveTrailingSlash(path));
                string parent = info.Directory == null ? "" : info.Directory.FullName;
                parentList.Add(parent);
            }
            //If the parent list if empty , it means that there is nothing to sync and thus return.
            if (parentList.Count == 0)
            {
                return;
            }
            //Create the request and Send it.
            AutoSyncRequest request = new AutoSyncRequest(fe.OldPath.Name, fe.OldPath.Directory.FullName, parentList, false, AutoSyncRequestType.Update, SyncConfig.Instance);
            SendAutoRequest(request);
        }

        private void HandleFileCreateEvent(FileChangeEvent fe)
        {
            //Find the logical Address for the old path
            //Do not create the logical address if not found.
            string logicalAddress = ProfilingLayer.Instance.ConvertPhysicalToLogical(fe.OldPath.FullName, false);
            //Get all the tag that contains this logicalAddress inside the tag.
            List<Tag> tag = TaggingLayer.Instance.RetrieveParentTagByPath(logicalAddress);
            //If tag count is 0 return as there is nothing to process.
            if (tag.Count == 0) //
            {
                return;
            }
            //Find the similiar paths for the logical Path
            //The return is physical address
            List<string> convertedList = FindSimilarSeamlessPathForFile(logicalAddress);
            convertedList.Remove(fe.OldPath.FullName);

            //////// Massive Path Table Code /////////////
            for (int i = 0; i < convertedList.Count; i++)
            {
                string dest = convertedList[i];
                if (_pathTable.JustPop(fe.OldPath.FullName, dest, TableType.Create))
                {
                    convertedList.Remove(dest);
                    i--;
                    continue;
                }
            }
            ///////////////// For each Path in the converted List ///////////////////
            ///////////////////////////////// Create a entry such that source = convertedPath and destination is the original Path ///////////
            ///////////////////////For each other path , create a Source + dest pair //////////
            ///////////////// Create an additional Path Entry for each of the Siblings ////////////////
            for (int i = 0; i < convertedList.Count; i++)
            {
                _pathTable.AddPathPair(convertedList[i], fe.OldPath.FullName, TableType.Create);
                for (int j = i + 1; j < convertedList.Count; j++)
                {
                    _pathTable.AddPathPair(convertedList[i], convertedList[j], TableType.Create);
                    _pathTable.AddPathPair(convertedList[j], convertedList[i], TableType.Create);
                }
            }
            ///////// End of Path Table Code ////////////////
            //For each path in the convertedList , extract the parent Path.
            List<string> parentList = new List<string>();
            foreach (string path in convertedList)
            {
                FileInfo info = new FileInfo(PathHelper.RemoveTrailingSlash(path));
                string parent = info.Directory == null ? "" : info.Directory.FullName;
                parentList.Add(parent);
            }
            //If the parent list if empty , it means that there is nothing to sync and thus return.
            if (parentList.Count == 0)
            {
                return;
            }
            //Create the request and Send it.
            AutoSyncRequest request = new AutoSyncRequest(fe.OldPath.Name, fe.OldPath.Directory.FullName, parentList, false, AutoSyncRequestType.New, SyncConfig.Instance);
            SendAutoRequest(request);
        }

        private static void SendAutoRequest(AutoSyncRequest request)
        {
#if DEBUG
            if (request.ChangeType == AutoSyncRequestType.New || request.ChangeType == AutoSyncRequestType.Update)
            {
                string output =
                    string.Format("=====================================================\nAuto Request sent : \nName of File : ({0}){1}\nSource : {2}{3}\nDestination:", (request.ChangeType == AutoSyncRequestType.New ? "New" : "Update"), request.SourceName, request.SourceParent, request.SourceName);
                foreach (string destination in request.DestinationFolders)
                {
                    output += "\n" + destination + "\\" + request.SourceName;
                }
                output += "\n================================================================";
                ServiceLocator.GetLogger(ServiceLocator.DEVELOPER_LOG).Write(output);
            }
            else if (request.ChangeType == AutoSyncRequestType.Rename)
            {
                string output =
                    string.Format("=====================================================\nAuto Request sent : \nName of File : (Renamed){0}\\\\{1}\nSource : {2}{3}---{4}\nDestination:", request.OldName, request.NewName, request.SourceParent, request.OldName, request.NewName);
                foreach (string destination in request.DestinationFolders)
                {
                    output += "\n" + destination + "\\" + request.OldName + " =>" + request.NewName;
                }
                output += "\n================================================================";
                ServiceLocator.GetLogger(ServiceLocator.DEVELOPER_LOG).Write(output);
            }
            else if (request.ChangeType == AutoSyncRequestType.Delete)
            {
                string output =
                    string.Format("=====================================================\nAuto Request sent : \nName of File : (Delete){0}\nSource : {1}{2}\nDestination:", request.SourceName, request.SourceParent, request.SourceName);
                foreach (string destination in request.DestinationFolders)
                {
                    output += "\n" + destination + "\\" + request.SourceName;
                }
                output += "\n================================================================";
                ServiceLocator.GetLogger(ServiceLocator.DEVELOPER_LOG).Write(output);
            }
#endif

            CompareAndSyncController.Instance.Sync(request);
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
                    HandleFolderNewEvent(fe);
                }
                else if (fe.Event == EventChangeType.RENAMED)
                {
                    HandleFolderRenameEvent(fe);
                }
            }
            catch (Exception e)
            {
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
            }
        }
        private void HandleFolderNewEvent(FolderChangeEvent fe)
        {
            //Find the logical Address for the old path
            //Do not create the logical address if not found.
            string logicalAddress = ProfilingLayer.Instance.ConvertPhysicalToLogical(fe.OldPath.FullName, false);
            //Get all the tag that contains this logicalAddress inside the tag.
            List<Tag> tag = TaggingLayer.Instance.RetrieveParentTagByPath(logicalAddress);
            //If tag count is 0 return as there is nothing to process.
            if (tag.Count == 0) //
            {
                return;
            }
            //Find the similiar paths for the logical Path
            //The return is physical address
            List<string> convertedList = FindSimilarSeamlessPathForFile(logicalAddress);
            convertedList.Remove(fe.OldPath.FullName);

            //////// Massive Path Table Code /////////////
            for (int i = 0; i < convertedList.Count; i++)
            {
                string dest = convertedList[i];
                if (_pathTable.JustPop(fe.OldPath.FullName, dest, TableType.Create))
                {
                    convertedList.Remove(dest);
                    i--;
                    continue;
                }
            }
            ///////////////// For each Path in the converted List ///////////////////
            ///////////////////////////////// Create a entry such that source = convertedPath and destination is the original Path ///////////
            ///////////////////////For each other path , create a Source + dest pair //////////
            ///////////////// Create an additional Path Entry for each of the Siblings ////////////////
            for (int i = 0; i < convertedList.Count; i++)
            {
                _pathTable.AddPathPair(convertedList[i], fe.OldPath.FullName, TableType.Create);
                for (int j = i + 1; j < convertedList.Count; j++)
                {
                    _pathTable.AddPathPair(convertedList[i], convertedList[j], TableType.Create);
                    _pathTable.AddPathPair(convertedList[j], convertedList[i], TableType.Create);
                }
            }
            ///////// End of Path Table Code ////////////////
            //For each path in the convertedList , extract the parent Path.
            List<string> parentList = new List<string>();
            foreach (string path in convertedList)
            {
                FileInfo info = new FileInfo(PathHelper.RemoveTrailingSlash(path));
                string parent = info.Directory == null ? "" : info.Directory.FullName;
                parentList.Add(parent);
            }
            //If the parent list if empty , it means that there is nothing to sync and thus return.
            if (parentList.Count == 0)
            {
                return;
            }
            //Create the request and Send it.
            AutoSyncRequest request = new AutoSyncRequest(fe.OldPath.Name, fe.OldPath.Parent.FullName, parentList, true, AutoSyncRequestType.New, SyncConfig.Instance);
            SendAutoRequest(request);
        }
        private void HandleFolderRenameEvent(FolderChangeEvent fe)
        {
            //Find the logical Address for the old path
            //Do not create the logical address if not found.
            string logicalAddress = ProfilingLayer.Instance.ConvertPhysicalToLogical(fe.OldPath.FullName, false);
            string newLogicalAddress = ProfilingLayer.Instance.ConvertPhysicalToLogical(fe.NewPath.FullName, false);
            //Get all the tag that contains this logicalAddress inside the tag.
            List<Tag> tag = TaggingLayer.Instance.RetrieveParentTagByPath(logicalAddress);
            //If tag count is 0 return as there is nothing to process.
            if (tag.Count != 0) //
            {


                //Find the similiar paths for the logical Path
                //The return is physical address
                List<string> convertedList = FindSimilarSeamlessPathForFile(logicalAddress);
                convertedList.Remove(fe.OldPath.FullName);

                //////// Massive Path Table Code /////////////
                for (int i = 0; i < convertedList.Count; i++)
                {
                    string dest = convertedList[i];
                    if (_pathTable.JustPop(fe.OldPath.FullName, dest, TableType.Rename))
                    {
                        convertedList.Remove(dest);
                        i--;
                        continue;
                    }
                }
                ///////////////// For each Path in the converted List ///////////////////
                ///////////////////////////////// Create a entry such that source = convertedPath and destination is the original Path ///////////
                ///////////////////////For each other path , create a Source + dest pair //////////
                ///////////////// Create an additional Path Entry for each of the Siblings ////////////////
                for (int i = 0; i < convertedList.Count; i++)
                {
                    _pathTable.AddPathPair(convertedList[i], fe.OldPath.FullName, TableType.Rename);
                    for (int j = i + 1; j < convertedList.Count; j++)
                    {
                        _pathTable.AddPathPair(convertedList[i], convertedList[j], TableType.Rename);
                        _pathTable.AddPathPair(convertedList[j], convertedList[i], TableType.Rename);
                    }
                }
                ///////// End of Path Table Code ////////////////
                //For each path in the convertedList , extract the parent Path.
                List<string> parentList = new List<string>();
                foreach (string path in convertedList)
                {
                    FileInfo info = new FileInfo(PathHelper.RemoveTrailingSlash(path));
                    string parent = info.Directory == null ? "" : info.Directory.FullName;
                    parentList.Add(parent);
                }
                //If the parent list if empty , it means that there is nothing to sync and thus return.
                if (parentList.Count != 0)
                {
                    AutoSyncRequest request = new AutoSyncRequest(fe.OldPath.Name, fe.NewPath.Name, fe.OldPath.Parent.FullName,
                                                                 parentList, true, AutoSyncRequestType.Rename,
                                                                 SyncConfig.Instance);
                    SendAutoRequest(request);
                }
            }

            TaggingLayer.Instance.RenameFolder(logicalAddress, newLogicalAddress);
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
                    HandleGenericDelete(dce);
                }
            }
            catch (Exception e)
            {
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
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
                    Merge(dce.Info);
                    string logical = ProfilingLayer.Instance.GetLogicalIdFromDrive(dce.Info);
                    List<Tag> tagList = TaggingLayer.Instance.RetrieveTagByLogicalId(logical);

                    foreach (Tag t in tagList)
                    {
                        SwitchMonitorTag(t, t.IsSeamless);
                    }
                }
                else
                {
                    ProfilingLayer.Instance.RemoveDrive(dce.Info);
                    MonitorLayer.Instance.UnMonitorDrive(dce.Info.Name);
                }
                _userInterface.DriveChanged();

            }
            catch (Exception e)
            {
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
            }
        }
        private void HandleGenericDelete(DeleteChangeEvent dce)
        {

            //Find the logical Address for the old path
            //Do not create the logical address if not found.
            string logicalAddress = ProfilingLayer.Instance.ConvertPhysicalToLogical(dce.Path.FullName, false);
            //Get all the tag that contains this logicalAddress inside the tag.

            List<Tag> tag = TaggingLayer.Instance.RetrieveParentTagByPath(logicalAddress);
            //If tag count is 0 return as there is nothing to process.
            if (tag.Count == 0)
            {
                return;
            }
            //Find the similiar paths for the logical Path
            //The return is physical address
            List<string> convertedList = FindSimilarSeamlessPathForFile(logicalAddress);
            convertedList.Remove(dce.Path.FullName);

            //////// Massive Path Table Code /////////////
            for (int i = 0; i < convertedList.Count; i++)
            {
                string dest = convertedList[i];
                if (_pathTable.JustPop(dce.Path.FullName, dest, TableType.Delete))
                {
                    convertedList.Remove(dest);
                    i--;
                    continue;
                }
            }
            ///////////////// For each Path in the converted List ///////////////////
            ///////////////////////////////// Create a entry such that source = convertedPath and destination is the original Path ///////////
            ///////////////////////For each other path , create a Source + dest pair //////////
            ///////////////// Create an additional Path Entry for each of the Siblings ////////////////
            for (int i = 0; i < convertedList.Count; i++)
            {
                _pathTable.AddPathPair(convertedList[i], dce.Path.FullName, TableType.Delete);
                for (int j = i + 1; j < convertedList.Count; j++)
                {
                    _pathTable.AddPathPair(convertedList[i], convertedList[j], TableType.Delete);
                    _pathTable.AddPathPair(convertedList[j], convertedList[i], TableType.Delete);
                }
            }
            ///////// End of Path Table Code ////////////////
            //For each path in the convertedList , extract the parent Path.


            List<string> parentList = new List<string>();
            foreach (string path in convertedList)
            {
                FileInfo info = new FileInfo(PathHelper.RemoveTrailingSlash(path));
                string parent = info.Directory == null ? "" : info.Directory.FullName;
                parentList.Add(parent);
            }
            //If the parent list if empty , it means that there is nothing to sync and thus return.
            if (parentList.Count == 0)
            {
                return;
            }
            //Create the request and Send it.

            AutoSyncRequest request = new AutoSyncRequest(dce.Path.Name, dce.Path.Parent.FullName, parentList, AutoSyncRequestType.Delete, SyncConfig.Instance);
            SendAutoRequest(request);

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
            return this.ManualSync(tagname, false);
        }
        /// <summary>
        /// Delete a tag
        /// </summary>
        /// <param name="tagname">Name of the tag to delete</param>
        /// <returns>true if a tag is removed. false if the tag cannot be removed</returns>
        public bool DeleteTag(string tagname)
        {
            if (CompareAndSyncController.Instance.IsQueuedOrSyncing(tagname))
            {
                return false;
            }
            try
            {
                Tag t = TaggingLayer.Instance.DeleteTag(tagname);
                SaveLoadHelper.SaveAll(_userInterface.getAppPath());
                return t != null;
            }
            catch (TagNotFoundException te)
            {
                throw te;
            }
            catch (Exception e)// Handle Unexpected Exception
            {
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
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
                Tag t = TaggingLayer.Instance.CreateTag(tagname);
                SaveLoadHelper.SaveAll(_userInterface.getAppPath());
                return ConvertToTagView(t);
            }
            catch (TagAlreadyExistsException te)
            {
                throw te;
            }
            catch (Exception e) //Handle Unexpected Exception
            {
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
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
                tag = TaggingLayer.Instance.TagFolder(path, tagname);
                if (tag == null)
                {
                    return null;
                }
                if (tag.IsSeamless)
                {
                    StartMonitorTag(tag);
                }
                SaveLoadHelper.SaveAll(_userInterface.getAppPath());
                return ConvertToTagView(tag);
            }
            catch (RecursiveDirectoryException re)
            {
                throw re;
            }
            catch (PathAlreadyExistsException pae)
            {
                throw pae;
            }
            catch (Exception e) // Handle Unexpected Exception
            {
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
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
                SaveLoadHelper.SaveAll(_userInterface.getAppPath());
                return count;


            }
            catch (TagNotFoundException tnfe)
            {
                throw tnfe;
            }
            catch (Exception e)
            {
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
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
            if (CompareAndSyncController.Instance.IsQueuedOrSyncing(tagname))
            {
                return false;
            }
            try
            {
                Tag tag = TaggingLayer.Instance.RetrieveTag(tagname);
                if (tag == null)
                {
                    throw new TagNotFoundException(tagname);
                }
                SwitchMonitorTag(tag, mode);
                return true;
            }
            catch (TagNotFoundException tge)
            {
                throw tge;
            }
            catch (Exception e)
            {
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
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
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
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
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
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
            if (tagname == null)
            {
                return null;
            }
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
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
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
                    ManualCompareRequest request = new ManualCompareRequest(filteredPaths[0].ToArray(), tag.Filters, SyncConfig.Instance);
                    return CompareAndSyncController.Instance.Compare(request);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
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
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
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
                _queueObserver.Stop();
                _reader.Stop();
                return true;
            }
            catch (Exception e)
            {
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
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
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
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
                TaggingLayer.Instance.UpdateFilter(tagname, filterlist);
                SaveLoadHelper.SaveAll(_userInterface.getAppPath());
                return true;
            }
            catch (TagNotFoundException tnfe)
            {
                throw tnfe;
            }
            catch (Exception e)
            {
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
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
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
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
            SaveLoadHelper.SaveAll(_userInterface.getAppPath());
            try
            {
                if (!drive.IsReady)
                {
                    return false;
                }
                MonitorLayer.Instance.UnMonitorDrive(drive.Name);
                ProfilingLayer.Instance.RemoveDrive(drive);
                _userInterface.DriveChanged();
                return true;
            }
            catch (DriveNotFoundException)
            {
                return false;
            }
            catch (Exception e)
            {
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                throw new UnhandledException(e);
            }
        }
        /// <summary>
        /// Clean the meta data of syncless in a folder.
        /// </summary>
        /// <param name="path">the path of the directory</param>
        /// <returns>number of .syncless removed.</returns>
        public int Clean(string path)
        {
            return Cleaner.CleanSynclessMeta(new DirectoryInfo(path));
        }
        /// <summary>
        /// Merge a profile from a particular path. Will only merge profile with the same name.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool Merge(string path)
        {
            ProfilingLayer.Instance.Merge(path);
            TaggingLayer.Instance.Merge(path);
            return true;
        }
        /// <summary>
        /// Set the profile name
        /// </summary>
        /// <param name="profileName">name of the profile</param>
        /// <returns></returns>
        public bool SetProfileName(string profileName)
        {
            return true;
        }
        /// <summary>
        /// Get the current profile name
        /// </summary>
        /// <returns>Profile name</returns>
        public string GetProfileName()
        {
            return ProfilingLayer.Instance.CurrentProfile.ProfileName;
        }
        /// <summary>
        /// Set the name for a drive.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="driveName"></param>
        /// <returns></returns>
        public bool SetDriveName(DriveInfo info, string driveName)
        {
            ProfilingLayer.Instance.SetDriveName(info, driveName);
            return false;
        }
        /// <summary>
        /// Return the user log 
        /// </summary>
        /// <returns>return list of log data.</returns>
        public List<LogData> ReadLog()
        {
            try
            {
                return LoggingLayer.Instance.ReadLog();
            }
            catch (LogFileCorruptedException lfce)
            {
                throw lfce;
            }
            catch (Exception e)
            {
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                throw new UnhandledException(e);
            }
        }
        #endregion

        #region private / delegate
        /// <summary>
        /// Set the mode of the tag to the mode.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="mode"></param>
        private void SetTagMode(Tag tag, bool mode)
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
                foreach (string path in convertedPath)
                {
                    try
                    {
                        MonitorLayer.Instance.MonitorPath(PathHelper.RemoveTrailingSlash(path));
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
                        MonitorLayer.Instance.UnMonitorPath(PathHelper.RemoveTrailingSlash(path));
                    }
                    catch (MonitorPathNotFoundException)
                    {
                    }
                }
            }


        }
        /// <summary>
        /// Manual Sync
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="notify"></param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        private bool ManualSync(Tag tag, bool notify)
        {
            if (CompareAndSyncController.Instance.IsQueuedOrSyncing(tag.TagName))
            {
                return false;
            }
            List<string> paths = tag.FilteredPathListString;
            List<string>[] filterPaths = ProfilingLayer.Instance.ConvertAndFilter(paths);

            ManualSyncRequest syncRequest = new ManualSyncRequest(filterPaths[0].ToArray(), tag.Filters, SyncConfig.Instance, tag.TagName, notify);
            CompareAndSyncController.Instance.Sync(syncRequest);

            return true;
        }
        /// <summary>
        /// Manual Sync
        /// </summary>
        /// <param name="tagname"></param>
        /// <param name="notify"></param>
        /// <returns></returns>
        private bool ManualSync(string tagname, bool notify)
        {
            Tag tag = TaggingLayer.Instance.RetrieveTag(tagname);
            if (tag == null)
            {
                return false;
            }
            return ManualSync(tag, notify);

        }
        /// <summary>
        /// Sync the tag then monitor the tag.
        /// </summary>
        /// <param name="tag"></param>
        private void StartMonitorTag(Tag tag)
        {
            ManualSync(tag, true);
        }
        /// <summary>
        /// Switch the tag to the mode. Do the respective work.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="mode"></param>
        private void SwitchMonitorTag(Tag tag, bool mode)
        {
            if (mode)
            {
                StartMonitorTag(tag);
            }
            else
            {
                SetTagMode(tag, false);
            }
        }
        /// <summary>
        /// Convert a Tag to a TagView for UI.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private TagView ConvertToTagView(Tag t)
        {
            TagView view = new TagView(t.TagName, t.LastUpdatedDate);
            List<string>[] pathList = ProfilingLayer.Instance.ConvertAndFilter(t.FilteredPathListString);
            List<string> namedPath = ProfilingLayer.Instance.ConvertAndFilterToNamed(pathList[1]);

            PathGroupView availGrpView = new PathGroupView("Available");
            List<PathView> pathViewList = new List<PathView>();
            foreach (string path in pathList[0])
            {
                PathView p = new PathView(path);
                if (!Directory.Exists(path))
                {
                    p.IsMissing = false;
                }
                pathViewList.Add(p);
            }
            availGrpView.PathList = pathViewList;
            PathGroupView unavailableList = new PathGroupView("Unavailable");
            pathViewList = new List<PathView>();
            foreach (string path in namedPath)
            {
                PathView p = new PathView(path);
                p.IsAvailable = false;
                pathViewList.Add(p);

            }
            view.GroupList.Add(availGrpView);
            view.GroupList.Add(unavailableList);

            view.Created = t.CreatedDate;
            view.IsSeamless = t.IsSeamless;
            view.IsQueued = CompareAndSyncController.Instance.IsQueued(t.TagName);
            view.IsSyncing = CompareAndSyncController.Instance.IsSyncing(t.TagName);
            return view;
        }
        /// <summary>
        /// Merge a profile from a drive. (use when a drive plug in)
        /// </summary>
        /// <param name="drive"></param>
        private void Merge(DriveInfo drive)
        {
            string profilingPath = PathHelper.AddTrailingSlash(drive.RootDirectory.FullName) + ProfilingLayer.RELATIVE_PROFILING_SAVE_PATH;
            ProfilingLayer.Instance.Merge(profilingPath);

            string taggingPath = PathHelper.AddTrailingSlash(drive.RootDirectory.FullName) + TaggingLayer.RELATIVE_TAGGING_SAVE_PATH;
            TaggingLayer.Instance.Merge(taggingPath);
            SaveLoadHelper.SaveAll(_userInterface.getAppPath());
        }
        /// <summary>
        /// Initialize
        /// </summary>
        /// <returns></returns>
        private bool Initiate()
        {
            try
            {
                _reader = new PathTableReader(_pathTable);
                _reader.Start();
                _queueObserver = new LogicQueueObserver();
                _queueObserver.Start();
                bool loadSuccess = SaveLoadHelper.LoadAll(_userInterface.getAppPath());
                if (!loadSuccess)
                {
                    return false;
                }
                DeviceWatcher.Instance.ToString(); //Starts watching for Drive Change
                List<Tag> tagList = TaggingLayer.Instance.FilteredTagList;
                foreach (Tag t in tagList)
                {
                    if (t.IsSeamless)
                    {
                        StartMonitorTag(t);
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                return false;
            }
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
                tempFilters.Add(new SynclessArchiveFilter("_synclessArchive"));
                tempFilters.AddRange(tag.Filters);

                string appendedPath;
                string trailingPath = tag.CreateTrailingPath(filePath, false);
                if (trailingPath != null)
                {
                    foreach (TaggedPath p in tag.FilteredPathList)
                    {
                        appendedPath = PathHelper.RemoveTrailingSlash((p.Append(trailingPath)));
                        if (!pathList.Contains(appendedPath) && !appendedPath.Equals(filePath))
                        {
                            string physicalAddress = ProfilingLayer.Instance.ConvertLogicalToPhysical(appendedPath);
                            if (physicalAddress != null && chain.ApplyFilter(tempFilters, physicalAddress))
                                pathList.Add(PathHelper.RemoveTrailingSlash(physicalAddress));
                        }
                    }
                }
            }


            return pathList;
        }
        /// <summary>
        /// Find a list of paths which are tagged but the the physical path no longer exists in filesystem
        /// </summary>
        /// <returns>The list of paths which are tagged but no longer exists in filesystem</returns>
        internal List<string> FindAllDeletedPaths()
        {
            List<string> deletedPaths = new List<string>();
            List<string> allPaths = ProfilingLayer.Instance.ConvertAndFilterToPhysical(TaggingLayer.Instance.GetAllPaths());
            foreach (string path in allPaths)
            {
                if (!Directory.Exists(path))
                {
                    if (!PathHelper.ContainsIgnoreCase(deletedPaths, path))
                    {
                        deletedPaths.Add(path);
                    }
                }
            }
            return deletedPaths;
        }
        #endregion

        #region For Notification
        /// <summary>
        /// Add a Tag Path ( Notify from Merging )
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="path"></param>
        internal void AddTagPath(Tag tag, TaggedPath path)
        {
            if (tag.IsSeamless)
            {
                string newPath = ProfilingLayer.Instance.ConvertLogicalToPhysical(path.PathName);
                if (newPath != null)
                {
                    MonitorLayer.Instance.MonitorPath(newPath);
                }
            }
        }
        /// <summary>
        /// Remove a Tag Path ( Notify from Merging )
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="path"></param>
        internal void RemoveTagPath(Tag tag, TaggedPath path)
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
        /// <summary>
        /// Add a Tag ( Notify from Merging )
        /// </summary>
        /// <param name="tag"></param>
        internal void AddTag(Tag tag)
        {
            TaggingLayer.Instance.AddTag(tag);
            SwitchMonitorTag(tag, tag.IsSeamless);
        }
        /// <summary>
        /// Remove a Tag ( Notify from Merging )
        /// </summary>
        /// <param name="tag"></param>
        internal void RemoveTag(Tag tag)
        {
            try
            {
                Tag t = TaggingLayer.Instance.DeleteTag(tag.TagName);
            }
            catch (TagNotFoundException te)
            {
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(te);
            }
            catch (Exception e)// Handle Unexpected Exception
            {
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
            }

        }
        /// <summary>
        /// Monitor Tag ( From Compare and Sync / Merging )
        /// </summary>
        /// <param name="tagname"></param>
        internal void MonitorTag(string tagname)
        {
            Tag t = TaggingLayer.Instance.RetrieveTag(tagname);
            if (t == null)
            {
                //Shouldn't happen
                return;
            }
            SetTagMode(t, true);
            _userInterface.TagChanged();
        }
        #endregion



    }
}
