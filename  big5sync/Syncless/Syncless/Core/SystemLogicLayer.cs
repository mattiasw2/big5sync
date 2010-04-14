/*
 * 
 * Author: Eric Ng Jun Feng
 * 
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Syncless.CompareAndSync.Manual.CompareObject;
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
using Syncless.Notification;
using Syncless.Core.View;
using System.Runtime.CompilerServices;
namespace Syncless.Core
{

    internal class SystemLogicLayer : IUIControllerInterface, IMonitorControllerInterface, ICommandLineControllerInterface
    {
        #region Singleton
        /// <summary>
        /// Instance of the System Logic Layer.
        /// </summary>
        private static SystemLogicLayer _instance;
        /// <summary>
        /// The User Interface that is using this System Logic Layer.
        /// </summary>
        private IUIInterface _userInterface;
        /// <summary>
        /// The Notification Queue Observer for _sllNotification.
        /// </summary>
        private LogicQueueObserver _queueObserver;
        /// <summary>
        /// The Table for storing the state of some of the tags that are in transition state(i.e ManualToSeamless, SeamlessToManual)
        /// </summary>
        private readonly Dictionary<string, TagState> _switchingTable;
        /// <summary>
        /// Return the Instance of System Logic Layer.
        /// </summary>
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
        /// <summary>
        /// UI Notification Queue
        /// </summary>
        public NotificationQueue UiNotification { get; private set; }
        /// <summary>
        /// UI Priority Notification Queue
        /// </summary>
        public NotificationQueue UiPriorityNotification { get; private set; }
        /// <summary>
        /// System Logic Layer Notification Queue
        /// </summary>
        public NotificationQueue SllNotification { get; private set; }
        /// <summary>
        /// Private constructor(for singleton)
        /// </summary>
        private SystemLogicLayer()
        {
            _userInterface = null;
            UiNotification = new NotificationQueue();
            SllNotification = new NotificationQueue();
            UiPriorityNotification = new NotificationQueue();
            _pathTable = new PathTable();
            _switchingTable = new Dictionary<string, TagState>();
        }
        #endregion

        #region PathTable
        /// <summary>
        /// Path Table
        /// </summary>
        private readonly PathTable _pathTable;
        /// <summary>
        /// A watcher that constantly find path that are tagged but does not exist on the filesystem.
        /// </summary>
        private DeletedTaggedPathWatcher _deletedTaggedPathWatcher;

        #endregion

        #region IMonitorControllerInterface
        /// <summary>
        /// A method for Monitor Interface to clear the path table.
        /// </summary>
        public void ClearPathHash()
        {
            _pathTable.ClearEntry();
        }

        /// <summary>
        /// Handle a file change (New,Update,Rename)
        /// </summary>
        /// <param name="fe">File Change Event with all the information for the file change</param>
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
            convertedList.Remove(fe.NewPath.FullName);

            //////// Massive Path Table Code /////////////
            for (int i = 0; i < convertedList.Count; i++)
            {
                string dest = convertedList[i];
                if (_pathTable.JustPop(fe.NewPath.FullName, dest, TableType.Rename))
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
                _pathTable.AddPathPair(convertedList[i], fe.NewPath.FullName, TableType.Rename);
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

            AutoSyncRequest request = new AutoSyncRequest(fe.OldPath.Name, fe.NewPath.Name, fe.OldPath.DirectoryName, parentList, false, AutoSyncRequestType.Rename, SyncConfig.Instance, ConvertTagListToTagString(tag));
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
            if (fe.OldPath.Directory != null)
            {
                AutoSyncRequest request = new AutoSyncRequest(fe.OldPath.Name, fe.OldPath.Directory.FullName, parentList, false, AutoSyncRequestType.Update, SyncConfig.Instance, ConvertTagListToTagString(tag));
                SendAutoRequest(request);
            }
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
            if (fe.OldPath.Directory != null)
            {
                AutoSyncRequest request = new AutoSyncRequest(fe.OldPath.Name, fe.OldPath.Directory.FullName, parentList, false, AutoSyncRequestType.New, SyncConfig.Instance, ConvertTagListToTagString(tag));
                SendAutoRequest(request);
            }
        }

        /// <summary>
        /// Handling Folder Change (New,Rename)
        /// </summary>
        /// <param name="fe">Folder Change Event with all the information for hte file change</param>
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
                else if (fe.Event == EventChangeType.DELETED)
                {
                    HandleRootFolderDeleteEvent(fe);
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
            // ReSharper disable PossibleNullReferenceException
            AutoSyncRequest request = new AutoSyncRequest(fe.OldPath.Name, fe.OldPath.Parent.FullName, parentList, true, AutoSyncRequestType.New, SyncConfig.Instance, ConvertTagListToTagString(tag));
            // ReSharper restore PossibleNullReferenceException
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
                    Debug.Assert(fe.OldPath.Parent != null);
                    AutoSyncRequest request = new AutoSyncRequest(fe.OldPath.Name, fe.NewPath.Name, fe.OldPath.Parent.FullName,
                                                                 parentList, true, AutoSyncRequestType.Rename,
                                                                 SyncConfig.Instance, ConvertTagListToTagString(tag));
                    SendAutoRequest(request);
                }
            }
            List<Tag> preRenameTagList = TaggingLayer.Instance.RetrieveTagByPath(logicalAddress);
            if (preRenameTagList.Count != 0)
            {
                TaggingLayer.Instance.RenameFolder(logicalAddress, newLogicalAddress);
                MonitorLayer.Instance.UnMonitorPath(fe.OldPath.FullName);
                MonitorLayer.Instance.MonitorPath(fe.NewPath.FullName);
                _userInterface.PathChanged();
            }


        }

        /// <summary>
        /// Handling delete change (For both file and folder) 
        ///   Unable to detect what type of the file is deleted as the file/folder no longer exist.
        /// </summary>
        /// <param name="dce">Delete Change Event with all the information for the file change</param>
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
        /// Handling drive change ( plug in and plug out)
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

                }
                else
                {
                    ProfilingLayer.Instance.RemoveDrive(dce.Info);
                    MonitorLayer.Instance.UnMonitorDrive(dce.Info.Name);
                }
                _userInterface.DriveChanged();
                _userInterface.PathChanged();
            }
            catch (Exception e)
            {
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
            }
        }
        private void HandleGenericDelete(DeleteChangeEvent dce)
        {
            MonitorLayer.Instance.UnMonitorPath(dce.Path.FullName);
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
            Debug.Assert(dce.Path.Parent != null);
            AutoSyncRequest request = new AutoSyncRequest(dce.Path.Name, dce.Path.Parent.FullName, parentList, AutoSyncRequestType.Delete, SyncConfig.Instance, ConvertTagListToTagString(tag));
            SendAutoRequest(request);
            FindAndCleanDeletedPaths();
            _userInterface.TagsChanged();
        }
        private void HandleRootFolderDeleteEvent(FolderChangeEvent dce)
        {
            MonitorLayer.Instance.UnMonitorPath(dce.OldPath.FullName);
            //Find the logical Address for the old path
            //Do not create the logical address if not found.
            string logicalAddress = ProfilingLayer.Instance.ConvertPhysicalToLogical(dce.OldPath.FullName, false);
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
            convertedList.Remove(dce.OldPath.FullName);

            //////// Massive Path Table Code /////////////
            for (int i = 0; i < convertedList.Count; i++)
            {
                string dest = convertedList[i];
                if (_pathTable.JustPop(dce.OldPath.FullName, dest, TableType.Delete))
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
                _pathTable.AddPathPair(convertedList[i], dce.OldPath.FullName, TableType.Delete);
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
            Debug.Assert(dce.OldPath.Parent != null);
            AutoSyncRequest request = new AutoSyncRequest(dce.OldPath.Name, dce.OldPath.Parent.FullName, parentList, AutoSyncRequestType.Delete, SyncConfig.Instance, ConvertTagListToTagString(tag));
            SendAutoRequest(request);
            FindAndCleanDeletedPaths();
            _userInterface.TagsChanged();
        }

        private void SendAutoRequest(AutoSyncRequest request)
        {
            #region debug region
            /*
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
            */
            #endregion
            CompareAndSyncController.Instance.Sync(request);
        }
        #endregion

        #region Logging
        /// <summary>
        /// Provide the method to get the logger object. Used by Service Locator.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public Logger GetLogger(string type)
        {
            return LoggingLayer.Instance.GetLogger(type);
        }

        #endregion

        #region IUIControllerInterface Members
        /// <summary>
        /// Starts a Manual Sync. The Sync will be queued and will be processed when it is its turn.
        /// </summary>
        /// <param name="tagName">Tagname of the Tag to sync</param>
        /// <returns>true if the sync is successfully queued. false if the tag is currently being queued/sync or the tag does not exist.  </returns>        
        /// <exception cref="UnhandledException">Unhandled Exception</exception>
        public bool StartManualSync(string tagName)
        {
            try
            {
                //Call the internal method to Sync, and does not switch to seamless after syncing.
                return ManualSync(tagName, false);
            }
            catch (TagNotFoundException)
            {
                //If tag not found, return false
                return false;
            }
            catch (Exception e)
            {
                //Handle some unexpected exception so that it does not hang the UI.
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                throw new UnhandledException(e);
            }
        }
        /// <summary>
        /// Cancel a Manual Sync.
        /// </summary>
        /// <param name="tagName">Tagname of the Tag to sync</param>
        /// <returns>true if the sync is cancel. false if the tag is not currently being queued/sync or the request cannot be cancel.</returns>
        /// <exception cref="UnhandledException">Unhandled Exception</exception> 
        public bool CancelManualSync(string tagName)
        {
            try
            {
                //If Tag is not currently being queue, return false
                if (!CompareAndSyncController.Instance.IsQueuedOrSyncing(tagName))
                {
                    return false;
                }
                //Notify Compare and Sync to Cancel the Job.
                return CompareAndSyncController.Instance.Cancel(new CancelSyncRequest(tagName));
            }
            catch (Exception e)
            {
                //Handle some unexpected exception so that it does not hang the UI.
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                throw new UnhandledException(e);
            }
        }
        /// <summary>
        /// Delete a tag
        /// </summary>
        /// <param name="tagName">Name of the tag to delete</param>
        /// <returns>true if a tag is removed. false if the tag cannot be removed(i.e Currently Synchronizing)</returns>
        /// <exception cref="UnhandledException">Unhandled Exception</exception>
        public bool DeleteTag(string tagName)
        {
            //If the tag is currently being Sync/Queue , does not allow the user to delete.
            if (CompareAndSyncController.Instance.IsQueuedOrSyncing(tagName))
            {
                return false;
            }
            try
            {
                //Delete the tag.
                Tag t = TaggingLayer.Instance.DeleteTag(tagName);
                //Initiate a Save.
                InitiateSave();
                //Clean up the .syncless file inside all the deleted tag.
                new DeleteTagCleanDelegate(DeleteTagClean).BeginInvoke(t, null, null);
                //if tag does not exist, return false.
                return t != null;
            }
            catch (TagNotFoundException)
            {
                //if tag does not exist , return false.
                return false;
            }
            catch (Exception e)
            {
                //Handle some unexpected exception so that it does not hang the UI.
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                throw new UnhandledException(e);
            }

        }
        /// <summary>
        /// Create a Tag.
        /// </summary>
        /// <param name="tagName">name of the tag.</param>
        /// <returns>The Detail of the Tag.</returns>
        /// <exception cref="UnhandledException">Unhandled Exception</exception>
        /// <exception cref="TagAlreadyExistsException">Tag with tagName already exist.</exception>
        public TagView CreateTag(string tagName)
        {
            try
            {
                //Create the tag.
                Tag t = TaggingLayer.Instance.CreateTag(tagName);
                //Initiate Save.
                InitiateSave();
                //Convert the tag to a tagview and return it.
                return ConvertToTagView(t);
            }
            catch (TagAlreadyExistsException)
            {
                throw;
            }
            catch (Exception e)
            {
                //Handle some unexpected exception so that it does not hang the UI.
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                throw new UnhandledException(e);
            }
        }
        /// <summary>
        /// Tag a Folder to a Tag based on the tag name
        /// </summary>
        /// <param name="tagname">name of the tag</param>
        /// <param name="folder">the folder to tag</param>
        /// <exception cref="InvalidPathException">The Path is invalid</exception>
        /// <exception cref="RecursiveDirectoryException">Tagging the folder will cause a recursive during Synchronization.</exception>
        /// <exception cref="PathAlreadyExistsException">The Path already exist in the Tag.</exception>
        /// <exception cref="UnhandledException">Unhandled Exception</exception>
        /// <returns>The Tag View representing the tag. return null , if the tag fail.</returns>
        public TagView Tag(string tagname, DirectoryInfo folder)
        {
            try
            {
                //Convert the path to a logical path.
                string path = ProfilingLayer.Instance.ConvertPhysicalToLogical(folder.FullName, true);
                //If path == null , means conversion fail. throw a Invalid Path Exception
                if (path == null)
                {
                    throw new InvalidPathException(folder.FullName);
                }
                //Tag the folder.
                Tag tag = TaggingLayer.Instance.TagFolder(path, tagname);
                if (tag == null) // If tag == null , Something went wrong , and thus return null.
                {
                    return null;
                }
                //if tag is seamless, start monitoring the tag.
                if (tag.IsSeamless)
                {

                    SwitchMode(tag.TagName, TagMode.Manual);
                    SwitchMode(tag.TagName, TagMode.Seamless);
                }
                //Initiate a save.
                InitiateSave();
                //Convert the Tag to a Tagview and return it.
                return ConvertToTagView(tag);
            }
            catch (RecursiveDirectoryException)
            {
                throw;
            }
            catch (PathAlreadyExistsException)
            {
                throw;
            }
            catch (InvalidPathException)
            {
                throw;
            }
            catch (Exception e)
            {
                //Handle some unexpected exception so that it does not hang the UI.
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
        /// <exception cref="UnhandledException">Unhandled Exception</exception>
        /// <returns>The number of path untag</returns>
        public int Untag(string tagname, DirectoryInfo folder)
        {
            try
            {
                //Convert the path to logical path.
                string path = ProfilingLayer.Instance.ConvertPhysicalToLogical(folder.FullName, true);
                //Untag a folder and record the number of path untagged.
                int count = TaggingLayer.Instance.UntagFolder(path, tagname);
                //Check if there if another tag that this path is tagged to.
                //if there is no other tag containing this path, unmonitor the path
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
                //Initiate a Save.
                InitiateSave();
                //Clean up the meta data (.syncless) inside the folder.
                new CleanMetaDataDelegate(CleanMetaData).BeginInvoke(folder, null, null);

                return count;
            }
            catch (TagNotFoundException)
            {
                throw;
            }
            catch (Exception e)
            {
                //Handle some unexpected exception so that it does not hang the UI.
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                throw new UnhandledException(e);
            }
        }
        /// <summary>
        /// Switch the mode of a particular Tag to a mode.
        /// Valid Mode are <see cref="TagMode.Seamless"/> and <see cref="TagMode.Manual"/>
        /// </summary>
        /// <param name="name">The name of the tag to switch</param>
        /// <param name="mode">The mode to switch to</param>
        /// <exception cref="ArgumentOutOfRangeException">If the TagMode Specified is not a valid TagMode</exception>
        /// <exception cref="UnhandledException">Unhandled Exception</exception>
        /// <returns>true if the mode can be switch.</returns>
        public bool SwitchMode(string name, TagMode mode)
        {
            try
            {
                switch (mode)
                {
                    case TagMode.Seamless: MonitorTag(name, true);
                        break;
                    case TagMode.Manual: MonitorTag(name, false);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("mode");
                }
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                throw;
            }
            catch (Exception e)
            {
                //Handle some unexpected exception so that it does not hang the UI.
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                throw new UnhandledException(e);
            }
        }
        /// <summary>
        /// Cancel a Mode swtich
        /// </summary>
        /// <param name="tagName">the name of the tag to cancel</param>
        /// <returns>true if is possible to cancel the switch, otherwise false</returns>
        public bool CancelSwitch(string tagName)
        {
            try
            {
                TagState state = GetTagState(tagName);
                if (!(state == TagState.SeamlessToManual || state == TagState.ManualToSeamless))
                {
                    return false;
                }
                if (!CompareAndSyncController.Instance.IsQueuedOrSyncing(tagName))
                {
                    _switchingTable.Remove(tagName);
                    _userInterface.TagChanged(tagName);
                    return true;
                }
                if (CompareAndSyncController.Instance.Cancel(new CancelSyncRequest(tagName)))
                {
                    _switchingTable.Remove(tagName);
                    _userInterface.TagChanged(tagName);
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                throw new UnhandledException(e);
            }
        }
        /// <summary>
        /// Get the current Tag of a particular tag.
        /// See <see cref="TagState"/> for a list of TagState
        /// </summary>
        /// <param name="tagname">The name of the tag</param>
        /// <exception cref="TagNotFoundException"><see cref="Tag"/> with the given tagname  is not found.</exception>
        /// <exception cref="UnhandledException">Unhandled Exception</exception>
        /// <returns><see cref="TagState"/> representing the state of the Tag.</returns>
        public TagState GetTagState(string tagname)
        {
            try
            {
                //Retrieve the Tag from TaggingLayer.
                //If tag is not found , throw TagNotFoundException
                Tag tag = TaggingLayer.Instance.RetrieveTag(tagname);
                if (tag == null)
                {
                    throw new TagNotFoundException(tagname);
                }
                //Try to get the state from the table
                //IF the state exist ( Probably either TagState.SeamlessToManual or TagState.ManualToSeamless )
                //return the state.
                //else return Seamless / Manual.
                TagState state;
                if (_switchingTable.TryGetValue(tagname, out state))
                {
                    return state;
                }
                return tag.IsSeamless ? TagState.Seamless : TagState.Manual;

            }
            catch (TagNotFoundException)
            {
                throw;
            }
            catch (Exception e)
            {
                //Handle some unexpected exception so that it does not hang the UI.
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                throw new UnhandledException(e);
            }
        }
        /// <summary>
        /// Return a list contains only the name of all the tags.
        /// </summary>
        /// <exception cref="UnhandledException">Unhandled Exception</exception>
        /// <returns>The List containing the name of the tags</returns>
        public List<string> GetAllTags()
        {
            try
            {
                //Retrieve the list of undeleted tagList.
                List<Tag> tagList = TaggingLayer.Instance.FilteredTagList;
                List<string> tagNames = new List<string>();
                //Add only the name to the list and return.
                foreach (Tag t in tagList)
                {

                    tagNames.Add(t.TagName);
                }
                //Sort the names before returning.
                tagNames.Sort();
                return tagNames;
            }
            catch (Exception e)
            {
                //Handle some unexpected exception so that it does not hang the UI.
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                throw new UnhandledException(e);
            }
        }
        /// <summary>
        /// Return a list of tag name that a folder is tagged to.
        /// </summary>
        /// <param name="folder">The folder to find</param>
        /// <exception cref="UnhandledException">Unhandled Exception</exception>
        /// <returns>The List containing the name of the tags that the folder is tag to.</returns>
        public List<string> GetTags(DirectoryInfo folder)
        {
            try
            {
                //Convert the folder path to a logical path.
                //If the folder cannot be converted, return a empty list.
                string path = ProfilingLayer.Instance.ConvertPhysicalToLogical(folder.FullName, false);
                if (path == null)
                {
                    return new List<string>();
                }
                //Retrieve a list of tags.
                List<Tag> tagList = TaggingLayer.Instance.RetrieveTagByPath(path);
                List<string> tagNames = new List<string>();
                //Add the name of the tags to the list.
                foreach (Tag t in tagList)
                {
                    tagNames.Add(t.TagName);
                }
                //Sort the name before returning.
                tagNames.Sort();
                return tagNames;
            }
            catch (Exception e)
            {
                //Handle some unexpected exception so that it does not hang the UI.
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                throw new UnhandledException(e);
            }
        }
        /// <summary>
        /// Get a <see cref="TagView"/> Representation of a <see cref="Tag"/>
        /// </summary>
        /// <param name="tagname">The name of the tag to get.</param>
        /// <exception cref="UnhandledException">Unhandled Exception</exception>
        /// <returns>The <see cref="TagView"/> representing the <see cref="Tag"/> Object.</returns>
        public TagView GetTag(string tagname)
        {
            //if the tagname is null , return null.
            if (tagname == null)
            {
                return null;
            }
            try
            {
                //Try to get the tag with the given name.
                //if it is null , return null
                //else Convert the tag into TagView and return it.
                Tag t = TaggingLayer.Instance.RetrieveTag(tagname);
                if (t == null)
                {
                    return null;
                }
                return ConvertToTagView(t);
            }
            catch (Exception e)
            {
                //Handle some unexpected exception so that it does not hang the UI.
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                throw new UnhandledException(e);
            }
        }
        /// <summary>
        /// Preview a Sync of a Tag 
        /// </summary>
        /// <param name="tagName">The name of the tag to preview</param>
        /// <exception cref="UnhandledException">Unhandled Exception</exception>
        /// <returns>The RootCompareObject representing the Preview Result. null if the tag does not exist.</returns>
        public RootCompareObject PreviewSync(string tagName)
        {
            //if tagName is null , return null.
            //Write to Developer Log as it cannot be null. 
            if (tagName == null)
            {
                ServiceLocator.GetLogger(ServiceLocator.DEVELOPER_LOG).Write("argument (TagName) @ PreviewSync is null");
                return null;
            }
            try
            {
                //Retrieve the Tag. 
                //If tag is null , throw Tag not found exception.
                Tag tag = TaggingLayer.Instance.RetrieveTag(tagName, false);
                if (tag == null)
                {
                    throw new TagNotFoundException(tagName);
                }
                //Retrieve the list of path that is in the tag that are not deleted.
                List<string> paths = tag.FilteredPathListString;
                //Convert and Filter the file to find all the existing paths.
                List<string>[] filteredPaths = ProfilingLayer.Instance.ConvertAndFilter(paths);
                //If the filter path is more than 1, create a manual compare object.
                //  Call the Method to compare in the CompareAndSyncController.
                //Else Return null.
                if (filteredPaths[0].Count >= 2)
                {
                    ManualCompareRequest request = new ManualCompareRequest(filteredPaths[0].ToArray(), tag.Filters, SyncConfig.Instance, tagName);
                    return CompareAndSyncController.Instance.Preview(request);
                }

                return null;
            }
            catch (TagNotFoundException)
            {
                throw;
            }
            catch (Exception e)
            {
                //Handle some unexpected exception so that it does not hang the UI.
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                throw new UnhandledException(e);
            }

        }
        /// <summary>
        /// Cancel a Preview Request
        /// </summary>
        /// <param name="tagName">Name of the tag to cancel Preview</param>
        public void CancelPreview(string tagName)
        {
            try
            {
                CompareAndSyncController.Instance.CancelPreview(tagName);
            }
            catch (Exception e)
            {
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                throw new UnhandledException(e);
            }
        }
        /// <summary>
        /// Prepare the core for termination.
        /// </summary>
        /// <exception cref="UnhandledException">Unhandled Exception</exception>
        /// <returns>true if the program can terminate, false if the program is not ready for termination</returns>
        public bool PrepareForTermination()
        {
            try
            {
                //TODO might have some bug here when terminating while a save Notification is not process.
                //Initiate A save.
                InitiateSave();
                //Check if Compare and Sync Controller is ready for termination.
                if (!CompareAndSyncController.Instance.PrepareForTermination())
                {
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                //Handle some unexpected exception so that it does not hang the UI.
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                throw new UnhandledException(e);
            }
        }
        /// <summary>
        /// Terminate the program. 
        /// This is to kill the threads created and release the resources.
        /// Everything will be terminated. 
        /// If CompareAndSyncController have a job running, it will be completed before the program send a Terminate Notification to the UI.
        /// </summary>
        /// <exception cref="UnhandledException">Unhandled Exception</exception>
        public void Terminate()
        {
            try
            {
                CompareAndSyncController.Instance.Terminate();
                DeviceWatcher.Instance.Terminate();
                MonitorLayer.Instance.Terminate();
                _queueObserver.Stop();
                _deletedTaggedPathWatcher.Stop();
                //Save();
            }
            catch (Exception e)
            {
                //Handle some unexpected exception so that it does not hang the UI.
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                throw new UnhandledException(e);
            }
        }
        /// <summary>
        /// Initiate the program. This is the first command that needs to be run
        /// If this method is not run before any other method is call, the system might fail.
        /// </summary>
        /// <param name="inf">The User Interface that implements the <see cref="IUIInterface"/></param>
        /// <exception cref="UnhandledException">Unhandled Exception</exception>
        /// <returns>true if the Logic Layer successfully initialized. false if the program fail to initialize</returns>
        public bool Initiate(IUIInterface inf)
        {
            //Check if the program have write access to the root directory of The application folder.
            bool hasWriteAccess = CheckForWriteAccess(inf);
            //if does not have write access, return false.
            if (!hasWriteAccess)
            {
                return false;
            }
            try
            {
                _userInterface = inf;
                //call the internal save method.
                bool init = Initiate();
                InitiateSave();
                return init;
            }
            catch (Exception e)
            {
                //Handle some unexpected exception so that it does not hang the UI.
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                throw new UnhandledException(e);
            }
        }
        /// <summary>
        /// Update the filterlist of a <see cref="Tag"/>
        /// </summary>
        /// <param name="tagname">tagname of the <see cref="Tag"/></param>
        /// <param name="filterlist">the list of filter to set to the tag.</param>
        /// <exception cref="UnhandledException">Unhandled Exception</exception>
        /// <returns>true if succeed, false if fail.</returns>
        public bool UpdateFilterList(string tagname, List<Filter> filterlist)
        {
            try
            {
                TaggingLayer.Instance.UpdateFilter(tagname, filterlist);
                InitiateSave();
                return true;
            }
            catch (TagNotFoundException)
            {
                throw;
            }
            catch (Exception e)
            {
                //Handle some unexpected exception so that it does not hang the UI.
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                throw new UnhandledException(e);
            }
        }
        /// <summary>
        /// Get all the filters for a particular <see cref="Tag"/>
        /// </summary>
        /// <param name="tagname">the name of the tag.</param>
        /// <exception cref="UnhandledException">Unhandled Exception</exception>
        /// <returns>the list of <see cref="Filter">Filters</see></returns>
        public List<Filter> GetAllFilters(String tagname)
        {
            try
            {
                //Retrieve the Tag and return the read only Filter List.
                Tag t = TaggingLayer.Instance.RetrieveTag(tagname);
                return t.ReadOnlyFilters;
            }
            catch (Exception e)
            {
                //Handle some unexpected exception so that it does not hang the UI.
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                throw new UnhandledException(e);
            }
        }
        /// <summary>
        /// Call to release a drive so that it can be safety remove.
        /// </summary>
        /// <param name="drive">the Drive to remove</param>
        /// <exception cref="UnhandledException">Unhandled Exception</exception>
        /// <returns>true if succeess , false if fail.</returns>
        public bool AllowForRemoval(DriveInfo drive)
        {
            try
            {
                Save();//force a save.
            }
            catch (Exception e)
            {
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                //If save fail , return false.
                return false;
            }
            try
            {
                //if drive is not ready, return false.
                if (!drive.IsReady)
                {
                    return false;
                }
                //Get The logical id of the drive.
                string logicalid = ProfilingLayer.Instance.GetLogicalIdFromDrive(drive);
                if (logicalid == null)
                {
                    //Drive does not exist in the profile
                    //Return true , since drive is not registered to the program.
                    return true;
                }
                //Get the list of tag associated to the drive
                // i.e The tag that contain a path that is inside the particular drive.
                List<Tag> tagList = TaggingLayer.Instance.RetrieveTagByLogicalId(logicalid);
                // Check that non of the tag is currently synchronizing.
                foreach (Tag t in tagList)
                {
                    if (CompareAndSyncController.Instance.IsQueuedOrSyncing(t.TagName))
                    {
                        return false;
                    }
                }
                //Unmonitor the Drive
                MonitorLayer.Instance.UnMonitorDrive(drive.Name);
                //Unregister the drive from the profilingLayer.
                ProfilingLayer.Instance.RemoveDrive(drive);
                //Inform the User Interface that there is a possible drive change and path change
                _userInterface.DriveChanged();
                _userInterface.PathChanged();
                return true;
            }
            catch (DriveNotFoundException)
            {
                return false;
            }
            catch (Exception e)
            {
                //Handle some unexpected exception so that it does not hang the UI.
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                throw new UnhandledException(e);
            }
        }
        /// <summary>
        /// Clean the .syncless metadata in the selected path
        /// </summary>
        /// <param name="path">the path of the directory</param>
        /// <exception cref="UnhandledException">Unhandled Exception</exception>
        /// <returns>number of .syncless removed.</returns>
        public int Clean(string path)
        {
            try
            {
                //Run the cleaner.
                return Cleaner.CleanSynclessMeta(new DirectoryInfo(path));
            }
            catch (Exception e)
            {
                //Handle some unexpected exception so that it does not hang the UI.
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                throw new UnhandledException(e);
            }
        }
        /// <summary>
        /// Return the user log 
        /// </summary>
        /// <exception cref="LogFileCorruptedException">The log file is corrupted.</exception>
        /// <exception cref="UnhandledException">Unhandled Exception</exception>
        /// <returns>the list of log data.</returns>
        public List<LogData> ReadLog()
        {
            try
            {
                //return the log from logging layer.
                return LoggingLayer.Instance.ReadLog();
            }
            catch (LogFileCorruptedException)
            {
                throw;
            }
            catch (Exception e)
            {
                //Handle some unexpected exception so that it does not hang the UI.
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                throw new UnhandledException(e);
            }
        }
        /// <summary>
        /// Clear the user log
        /// </summary>
        public void ClearLog()
        {
            try
            {
                LoggingLayer.Instance.ClearLog();
            }
            catch (IOException)
            {
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception e)
            {
                //Handle some unexpected exception so that it does not hang the UI.
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                throw new UnhandledException(e);
            }
        }
        /// <summary>
        /// Get a Copy of the Sync Config
        /// </summary>
        /// <returns>A copy of Sync Config</returns>
        public SyncConfig GetSyncConfig()
        {
            return SyncConfig.Copy;
        }
        /// <summary>
        /// Update the Sync Config
        /// </summary>
        /// <param name="config">The new value of the Sync Config</param>
        public void UpdateSyncConfig(SyncConfig config)
        {
            SyncConfig.Instance = config;
        }
        /// <summary>
        /// Check if any drive is not detected by syncless and update them into the system.
        /// </summary>
        public void UpdateAllDrive()
        {
            try
            {
                CheckAndUpdateDrive();
                _userInterface.TagsChanged();
                _userInterface.PathChanged();
            }
            catch (Exception e)
            {
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                throw new UnhandledException(e);
            }

        }


        #endregion

        #region private /internal / delegate
        /// <summary>
        /// Set the mode of the tag to the mode.
        /// <remarks>
        /// This is a direct set method. Should only be call by the notification after a Sync is complete or if switching from seamless to manual. 
        /// If a tag is switch from manual to seamless without doing a manual sync, the tag may not be in sync and seamless mode may not work in a proper way.
        /// </remarks>
        /// </summary>
        /// <param name="tag">The <see cref="Tag"/> to set the mode.</param>
        /// <param name="mode">true if the mode is seamless, false if the mode is manual</param>
        private void SetTagMode(Tag tag, bool mode)
        {
            //set the tag to the mode.
            tag.IsSeamless = mode;
            //Initiate a Save.
            InitiateSave();
            //Retrieve all the Path string from the list of path that are not deleted from the tag.
            List<string> pathList = new List<string>();
            foreach (TaggedPath path in tag.FilteredPathList)
            {
                pathList.Add(path.PathName);
            }
            //Convert all the paths to physical path.
            //if mode is seamless, start monitoring all of them.
            //if mode is manual, unmonitor all of them.
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
            //try to remove the tag from the switching table
            //this will ensure that the ui can update the current state of the tag.
            try
            {
                if (_switchingTable.ContainsKey(tag.TagName))
                {
                    _switchingTable.Remove(tag.TagName);
                }
            }
            catch (Exception e)
            {
                //this should not happen but record it down just in case.
                //This should not affect the flow of the application so shall not report.
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
            }
        }
        /// <summary>
        /// Start a Manual Sync. The Sync will be queued and will be processed when it is its turn.
        /// </summary>
        /// <param name="tag">Tag to sync</param>
        /// <param name="switchSeamless">true if the tag needs to be set to seamless mode after synchronization</param>
        /// <returns>true if the sync is successfully queue. false if the tag is already queued or cannot be queued.</returns>  
        [MethodImpl(MethodImplOptions.Synchronized)]
        private bool ManualSync(Tag tag, bool switchSeamless)
        {
            //Try and cleanup the deletepaths first before syncing.
            //This is the find all the folder that is deleted but still tagged.
            //This will ensure that if the folder does not exist, no files will be propagated over.
            FindAndCleanDeletedPaths();
            //If the Tag is already synchronzing or is already queued , return false.
            if (CompareAndSyncController.Instance.IsQueuedOrSyncing(tag.TagName))
            {
                return false;
            }
            //Retrieve the list of paths that are not deleted.
            List<string> paths = tag.FilteredPathListString;
            //Convert the path to physical address.
            List<string>[] filterPaths = ProfilingLayer.Instance.ConvertAndFilter(paths);
            //If the number of path is less than 2,  does not sync and return a true 
            //If switch Seamless is true , set the mode of the tag to seamless.
            if (filterPaths[0].Count < 2)
            {
                if (switchSeamless)
                {
                    SetTagMode(tag, true);
                }
                return true;
            }
            //Create the manual Sync request and send it to CompareAndSyncController.
            ManualSyncRequest syncRequest = new ManualSyncRequest(filterPaths[0].ToArray(), tag.Filters, SyncConfig.Copy, tag.TagName, switchSeamless);
            CompareAndSyncController.Instance.Sync(syncRequest);

            return true;
        }
        /// <summary>
        /// Manual Sync a tag based on tagname
        /// </summary>
        /// <param name="tagname">The name of the tag</param>
        /// <param name="switchSeamless">whether to switch to seamless after the Manual Sync</param>
        /// <returns>true if the tag is successfully queued. false if tag does not exist.</returns>
        private bool ManualSync(string tagname, bool switchSeamless)
        {
            //if tag is null , return false.
            //else delegate it to the other method
            Tag tag = TaggingLayer.Instance.RetrieveTag(tagname);
            if (tag == null)
            {
                return false;
            }
            return ManualSync(tag, switchSeamless);
        }
        /// <summary>
        /// Sync the tag then monitor the tag.
        /// </summary>
        /// <param name="tag">The Tag to Start Monitor</param>
        private void StartMonitorTag(Tag tag)
        {
            ManualSync(tag, true);
        }
        /// <summary>
        /// Set the monitor mode for a tag
        /// </summary>
        /// <param name="tagname">Tagname to set</param>
        /// <param name="mode">true - set tag to seamless. , false - set tag to manual</param>
        /// <exception cref="TagNotFoundException">If the Tag is not found</exception>
        /// <exception cref="UnhandledException">Unhandled Exception</exception>
        /// <returns>whether the tag can be changed.</returns>
        private void MonitorTag(string tagname, bool mode)
        {
            try
            {
                //Retrieve a tag
                //If tag is null, throw TagNotFoundException
                Tag tag = TaggingLayer.Instance.RetrieveTag(tagname);
                if (tag == null)
                {
                    throw new TagNotFoundException(tagname);
                }
                //Call the internal method to switch the tag.
                SwitchMonitorTag(tag, mode);
                _userInterface.TagChanged(tagname);
                return;
            }
            catch (TagNotFoundException)
            {
                throw;
            }
            catch (Exception e)
            {
                //Handle some unexpected exception so that it does not hang the UI.
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                throw new UnhandledException(e);
            }
        }
        /// <summary>
        /// Switch the tag to the mode. Do the respective work.
        /// </summary>
        /// <param name="tag">Tag to switch</param>
        /// <param name="mode">true for seamless, false for manual</param>
        private void SwitchMonitorTag(Tag tag, bool mode)
        {
            // If the tag is deleted or the tag is queued or syncing, cannot switch.
            if (tag.IsDeleted || CompareAndSyncController.Instance.IsQueuedOrSyncing(tag.TagName))
            {
                return;
            }
            //Try to get the state of the tag.
            TagState state;
            _switchingTable.TryGetValue(tag.TagName, out state);
            //If the state is undefined, switch the state and add it to the switching table.
            if (state == TagState.Undefined)
            {
                _switchingTable.Add(tag.TagName, mode ? TagState.ManualToSeamless : TagState.SeamlessToManual);
            }
            //call the internal method.
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
        /// Convert a <see cref="Tag"/> to a <see cref="TagView"/> for UI.
        /// </summary>
        /// <param name="t">The tag to convert</param>
        /// <returns>return the <see cref="TagView"/> representing the Tag.</returns>
        private TagView ConvertToTagView(Tag t)
        {
            //Find and clean all the deleted folder that are still tag.
            FindAndCleanDeletedPaths();
            //Create the Tag View

            //Convert the path.
            List<string>[] pathList = ProfilingLayer.Instance.ConvertAndFilter(t.FilteredPathListString);
            //a list of available path
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
            TagView view = new TagView(t.TagName, t.LastUpdatedDate);
            view.GroupList.Add(availGrpView);

            view.Created = t.CreatedDate;
            view.IsQueued = CompareAndSyncController.Instance.IsQueued(t.TagName);
            view.IsSyncing = CompareAndSyncController.Instance.IsSyncing(t.TagName);

            view.TagState = GetTagState(view.TagName);
            return view;
        }
        /// <summary>
        /// Merge a profile from a drive. (use when a drive plug in)
        /// </summary>
        /// <param name="drive">The drive to merge.</param>
        private void Merge(DriveInfo drive)
        {
            string profilingPath = PathHelper.AddTrailingSlash(drive.RootDirectory.FullName) + ProfilingLayer.RELATIVE_PROFILING_SAVE_PATH;
            ProfilingLayer.Instance.Merge(profilingPath);

            string taggingPath = PathHelper.AddTrailingSlash(drive.RootDirectory.FullName) + TaggingLayer.RELATIVE_TAGGING_SAVE_PATH;
            TaggingLayer.Instance.Merge(taggingPath);
            InitiateSave();
        }
        /// <summary>
        /// Initialize
        /// </summary>
        /// <returns>true if initiate successful, false if initiate fail.</returns>
        private bool Initiate()
        {
            try
            {

                //Initiate the Logic Queue Observer.
                _queueObserver = new LogicQueueObserver();
                _queueObserver.Start();
                //Attempt to load the XML.
                bool loadSuccess = SaveLoadHelper.LoadAll(_userInterface.getAppPath());
                //If load fail , return false.
                if (!loadSuccess)
                {
                    _queueObserver.Stop();
                    return false;
                }
                //Starts watching for Drive Change
                DeviceWatcher.Instance.ToString();
                FindAndCleanDeletedPaths();
                //Get the List of filtered tag.
                List<Tag> tagList = TaggingLayer.Instance.FilteredTagList;
                //For each tag in the tag list
                // if tag is seamless, switch the mode to "TagState.ManualToSeamless"
                //    then do a manual sync.

                foreach (Tag t in tagList)
                {
                    if (t.IsSeamless)
                    {
                        try
                        {
                            if (_switchingTable.ContainsKey(t.TagName))
                            {
                                _switchingTable[t.TagName] = TagState.ManualToSeamless;
                            }
                            else
                            {
                                _switchingTable.Add(t.TagName, TagState.ManualToSeamless);
                            }

                        }
                        catch (Exception e)
                        {
                            ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                        }

                        StartMonitorTag(t);
                    }
                }
                //start the deleted path watcher to monitor which path is deleted but still tagged.
                _deletedTaggedPathWatcher = new DeletedTaggedPathWatcher();
                _deletedTaggedPathWatcher.Start();
                return true;
            }
            catch (Exception e)
            {
                //Initialize fail return false. 
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                if (_queueObserver != null)
                {
                    _queueObserver.Stop();
                }

                return false;
            }
        }
        /// <summary>
        /// Find the similar path for a particular file/folder.
        /// </summary>
        /// <param name="filePath">The path to search</param>
        /// <returns>The list of similar paths</returns>
        private List<string> FindSimilarSeamlessPathForFile(string filePath)
        {
            /* 2 path is consider similiar in the following case.
             *  i.e Folder A is tagged to Folder B.
             *   FolderA/1.txt is considered similiar to FolderB/1.txt
             *   
             * This is use to find similiar when a file change is detected during seamless mode.
             * 
             */

            //Convert the path to logical path
            string logicalid = TaggingHelper.GetLogicalID(filePath);

            List<string> pathList = new List<string>();
            // Get a list of that tag that contain the logical id.
            List<Tag> matchingTag = TaggingLayer.Instance.RetrieveTagByLogicalId(logicalid);
            FilterChain chain = new FilterChain();
            // Only find the tag if the tag is seamless mode.
            foreach (Tag tag in matchingTag)
            {
                if (!tag.IsSeamless)
                {
                    continue;
                }
                //Create the default filter 
                List<Filter> tempFilters = new List<Filter>
                                               {
                                                   FilterFactory.CreateArchiveFilter("_synclessArchive"),
                                                   FilterFactory.CreateConfigurationFilter()
                                               };
                tempFilters.AddRange(tag.Filters);

                string appendedPath;
                string trailingPath = tag.CreateTrailingPath(filePath);
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
            //Get all the path that exist in the TaggingLayer.
            List<string> allPaths = ProfilingLayer.Instance.ConvertAndFilterToPhysical(TaggingLayer.Instance.GetAllPaths());
            foreach (string path in allPaths)
            {
                // If the Directory Does not Exist.
                // If deleted path does not already contain the path , add it in.
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
        /// <summary>
        /// Find and Clean all the deleted Paths.
        /// </summary>
        private void FindAndCleanDeletedPaths()
        {
            //Find all the deleted path
            List<string> deletedPaths = FindAllDeletedPaths();
            foreach (string paths in deletedPaths)
            {
                //Convert the path to logical and untag it.
                string convertedPath = ProfilingLayer.Instance.ConvertPhysicalToLogical(paths, false);
                if (convertedPath != null)
                {
                    TaggingLayer.Instance.UntagFolder(convertedPath);
                }
            }
        }
        /// <summary>
        /// Clean Meta Data Delegate and Clean Meta Data.
        /// </summary>
        /// <param name="info">The Directory to clean.</param>
        private delegate void CleanMetaDataDelegate(DirectoryInfo info);
        private void CleanMetaData(DirectoryInfo folder)
        {
            //if the folder does not exist, do nothing.
            if (!folder.Exists) return;
            // Convert the path to a logical path.
            string convertedPath = ProfilingLayer.Instance.ConvertPhysicalToLogical(folder.FullName, false);
            // if the converted path is null or equals to "" , means that the path does not exist in the system , thus ignore it.
            if (convertedPath == null || convertedPath.Equals("")) return;
            // See if this folder is still tagged to any tag.
            List<Tag> tagList = TaggingLayer.Instance.RetrieveTagByPath(convertedPath);
            if (tagList.Count > 0) return; //Still have tag contain the folder , do not attempt to clean.
            // See if there is any tag contain the path as a children.
            List<string> parentPaths = TaggingLayer.Instance.RetrieveAncestors(convertedPath);
            if (parentPaths.Count != 0) return;//Parent still tagged. Do not clean.
            // Find all the child path that are tagged.
            List<string> childPaths = TaggingLayer.Instance.RetrieveDescendants(convertedPath);
            // Convert all the child path to physical path.
            List<string> convertedList = ProfilingLayer.Instance.ConvertAndFilterToPhysical(childPaths);
            // Clean the folder, but ignore all the child path.
            Cleaner.CleanSynclessMeta(folder, convertedList);
        }
        /// <summary>
        /// Clean a tag before deleting
        /// </summary>
        /// <param name="t">The Tag to clean.</param>
        private delegate void DeleteTagCleanDelegate(Tag t);
        private void DeleteTagClean(Tag t)
        {
            //For each path in the path, convert it to physical path and if it exist, Clean the meta data.
            foreach (TaggedPath path in t.UnfilteredPathList)
            {
                string convertedPath = ProfilingLayer.Instance.ConvertLogicalToPhysical(path.PathName);

                if (Directory.Exists(convertedPath))
                {
                    CleanMetaData(new DirectoryInfo(convertedPath));
                }
            }
        }
        /// <summary>
        /// Check the program folder if Syncless have write access.
        /// </summary>
        /// <param name="inf">The user interface.</param>
        /// <returns>true if Syncless have write access, otherwise false</returns>
        private bool CheckForWriteAccess(IUIInterface inf)
        {
            try
            {
                //Ensure the app folder have write access
                string path = Path.Combine(inf.getAppPath(), "temp.txt");
                FileStream stream = new FileStream(path, FileMode.Create);

                stream.Close();
                try
                {
                    FileInfo info = new FileInfo(path);
                    if (info.Exists)
                    {
                        info.Delete();
                    }
                }
                catch (Exception e)
                {
                    ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                }
            }
            catch (IOException)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// Convert a list of tag to a list of tagname
        /// </summary>
        /// <param name="tagList"></param>
        /// <returns></returns>
        private List<string> ConvertTagListToTagString(IEnumerable<Tag> tagList)
        {
            List<string> tagStringList = new List<string>();
            foreach (Tag tag in tagList)
            {
                tagStringList.Add(tag.TagName);
            }
            return tagStringList;
        }
        /// <summary>
        /// Initiate a Save
        /// </summary>
        private void InitiateSave()
        {
            // if the queue already contain a Save Notification , do not enqueue.
            if (!SllNotification.Contains(new SaveNotification()))
            {
                SllNotification.Enqueue(new SaveNotification());
            }
        }

        private void CheckAndUpdateDrive()
        {
            DriveInfo[] infos = DriveInfo.GetDrives();
            foreach (DriveInfo info in infos)
            {
                string logicalid = ProfilingLayer.Instance.GetLogicalIdFromDrive(info);
                if (logicalid == null)
                {
                    ProfilingLayer.Instance.UpdateDrive(info);
                }
            }
        }

        #endregion

        #region For Notification // SideThread
        /// <summary>
        /// Add a Tag Path ( Notify from Merging )
        /// </summary>
        /// <param name="tag">Tag that the path was added to </param>
        /// <param name="path">Path that is added</param>
        internal void AddTagPath(Tag tag, TaggedPath path)
        {
            //At the moment nothing needs to be done , just switch the tag to manual and back to seamless will do.
            if (tag.IsSeamless)
            {
                SwitchMode(tag.TagName, TagMode.Manual);
                SwitchMode(tag.TagName, TagMode.Seamless);
            }
        }
        /// <summary>
        /// Remove a Tag Path ( Notify from Merging )
        /// </summary>
        /// <param name="tag">Tag that the path was removed from</param>
        /// <param name="path">Path that is added</param>
        internal void RemoveTagPath(Tag tag, TaggedPath path)
        {
            //At the moment nothing needs to be done , just switch the tag to manual and back to seamless will do.
            if (tag.IsSeamless)
            {
                SwitchMode(tag.TagName, TagMode.Manual);
                SwitchMode(tag.TagName, TagMode.Seamless);
            }
        }
        /// <summary>
        /// Add a Tag ( Notify from Merging )
        /// </summary>
        /// <param name="tag">Tag that was added</param>
        internal void AddTag(Tag tag)
        {
            //if tag is deleted, nothing needs to be done.
            if (tag.IsDeleted)
            {
                return;
            }
            //If the tag is seamless, re-set the mode to seamless
            if (tag.IsSeamless)
            {
                SwitchMode(tag.TagName, TagMode.Manual);
                SwitchMode(tag.TagName, TagMode.Seamless);
            }
            else //set the mode to manual
            {
                SwitchMode(tag.TagName, TagMode.Manual);
            }
        }
        /// <summary>
        /// Remove a Tag ( Notify from Merging )
        /// </summary>
        /// <param name="tag">Tag that is removed.</param>
        internal void RemoveTag(Tag tag)
        {
            try
            {
                //Unmonitor the tag first then delete the tag.
                SwitchMode(tag.TagName, TagMode.Manual);
                TaggingLayer.Instance.DeleteTag(tag.TagName);
                _userInterface.TagsChanged();
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
        /// Inform Core that the tag have complete sync and can be now switch to seamless
        /// </summary>
        /// <param name="tagname">Name of the <see cref="Tag"/></param>
        internal void MonitorTag(string tagname)
        {
            //Retrieve the tag, if the tag does not exist, ignore.
            Tag t = TaggingLayer.Instance.RetrieveTag(tagname);
            if (t == null)
            {
                //Shouldn't happen
                return;
            }
            // Set the mode of the tag to seamless.
            SetTagMode(t, true);
            // Inform the UI of the changes.
            _userInterface.TagChanged(tagname);
        }
        /// <summary>
        /// Inform The Tagging Layer to untag a particular path is deleted.
        /// </summary>
        /// <param name="pathList">List of path to untag.</param>
        internal void Untag(List<string> pathList)
        {
            //foreach path , convert to logical and untag it.
            foreach (string path in pathList)
            {
                string convertedPath = ProfilingLayer.Instance.ConvertPhysicalToLogical(path, false);
                TaggingLayer.Instance.UntagFolder(convertedPath);
            }
            _userInterface.TagsChanged();
        }
        /// <summary>
        /// Save the Tagging Profile and Drive Profile.
        /// </summary>
        internal void Save()
        {
            SaveLoadHelper.SaveAll(_userInterface.getAppPath());
        }

        #endregion

    }
}
