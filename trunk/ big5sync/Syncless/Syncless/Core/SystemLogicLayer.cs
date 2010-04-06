﻿using System;
using System.Collections.Generic;
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
        private Dictionary<string, TagState> _switchingTable;
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
        private PathTable _pathTable;
        /// <summary>
        /// Internal Reader, use for debugging purpose
        /// </summary>
        private PathTableReader _reader;
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
            List<string> tagList = new List<string>();
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
                AutoSyncRequest request = new AutoSyncRequest(fe.OldPath.Name, fe.OldPath.Directory.FullName, parentList, false, AutoSyncRequestType.Update, SyncConfig.Instance,ConvertTagListToTagString(tag));
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
                AutoSyncRequest request = new AutoSyncRequest(fe.OldPath.Name, fe.OldPath.Directory.FullName, parentList, false, AutoSyncRequestType.New, SyncConfig.Instance,ConvertTagListToTagString(tag));
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
            AutoSyncRequest request = new AutoSyncRequest(fe.OldPath.Name, fe.OldPath.Parent.FullName, parentList, true, AutoSyncRequestType.New, SyncConfig.Instance,ConvertTagListToTagString(tag));
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
                                                                 SyncConfig.Instance,ConvertTagListToTagString(tag));
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

            AutoSyncRequest request = new AutoSyncRequest(dce.Path.Name, dce.Path.Parent.FullName, parentList, AutoSyncRequestType.Delete, SyncConfig.Instance,ConvertTagListToTagString(tag));
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

            AutoSyncRequest request = new AutoSyncRequest(dce.OldPath.Name, dce.OldPath.Parent.FullName, parentList, AutoSyncRequestType.Delete, SyncConfig.Instance,ConvertTagListToTagString(tag));
            SendAutoRequest(request);
            FindAndCleanDeletedPaths();
            _userInterface.TagsChanged();
        }

        private static void SendAutoRequest(AutoSyncRequest request)
        {
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
        /// Manually Sync a Tag
        /// </summary>
        /// <param name="tagname">Tagname of the Tag to sync</param>
        /// <returns>true if the sync is "queue"</returns>        
        public bool StartManualSync(string tagname)
        {
            try
            {
                return ManualSync(tagname, false);
            }
            catch (Exception e)
            {
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                throw new UnhandledException(e);
            }
        }
        /// <summary>
        /// Cancel a Manual Sync.
        /// </summary>
        /// <param name="tagName">Tagname of the Tag to sync</param>
        /// <returns>true if the sync is cancel.</returns>
        public bool CancelManualSync(string tagName)
        {
            try
            {
                if (!CompareAndSyncController.Instance.IsQueuedOrSyncing(tagName))
                {
                    return false;
                }
                return CompareAndSyncController.Instance.Cancel(new CancelSyncRequest(tagName));
            }
            catch (Exception e)
            {
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
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
            if (CompareAndSyncController.Instance.IsQueuedOrSyncing(tagname))
            {
                return false;
            }
            try
            {
                Tag t = TaggingLayer.Instance.DeleteTag(tagname);
                SaveLoadHelper.SaveAll(_userInterface.getAppPath());
                new DeleteTagCleanDelegate(DeleteTagClean).BeginInvoke(t,null,null);
                
                return t != null;
            }
            catch (TagNotFoundException)
            {
                return false;
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
            catch (TagAlreadyExistsException)
            {
                throw;
            }
            catch (Exception e) //Handle Unexpected Exception
            {
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

                Tag tag = TaggingLayer.Instance.TagFolder(path, tagname);
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
                SaveLoadHelper.SaveAll(_userInterface.getAppPath());
                new CleanMetaDataDelegate(CleanMetaData).BeginInvoke(folder, null, null);
                
                return count;
            }
            catch (TagNotFoundException)
            {
                throw;
            }
            catch (Exception e)
            {
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                throw new UnhandledException(e);
            }
        }
        /// <summary>
        /// Switch the mode of a particular Tag to a mode.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public bool SwitchMode(string name, TagMode mode)
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

        public TagState GetTagState(string tagname)
        {

            Tag tag = TaggingLayer.Instance.RetrieveTag(tagname);
            if (tag == null)
            {
                //TODO
                throw new Exception();
            }
            TagState state = TagState.Undefined;
            if (_switchingTable.TryGetValue(tagname, out state))
            {
                return state;
            }
            return tag.IsSeamless ? TagState.Seamless : TagState.Manual;


        }

        /// <summary>
        /// Set the monitor mode for a tag
        /// </summary>
        /// <param name="tagname">tagname to set</param>
        /// <param name="mode">true - set tag to seamless. , false - set tag to manual</param>
        /// <exception cref="TagNotFoundException">If the Tag is not found</exception>
        /// <returns>whether the tag can be changed.</returns>
        private void MonitorTag(string tagname, bool mode)
        {

            if (CompareAndSyncController.Instance.IsQueuedOrSyncing(tagname))
            {
                return;
            }
            try
            {
                Tag tag = TaggingLayer.Instance.RetrieveTag(tagname);
                if (tag == null)
                {
                    throw new TagNotFoundException(tagname);
                }
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
                List<Tag> tagList = TaggingLayer.Instance.TagList;
                List<string> tagNames = new List<string>();
                foreach (Tag t in tagList)
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

                return null;

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
                if (!CompareAndSyncController.Instance.PrepareForTermination())
                {
                    return false;
                }
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
        public void Terminate()
        {
            try
            {

                CompareAndSyncController.Instance.Terminate();
                DeviceWatcher.Instance.Terminate();
                MonitorLayer.Instance.Terminate();
                _queueObserver.Stop();
                _reader.Stop();
                _deletedTaggedPathWatcher.Stop();
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
            bool hasWriteAccess = CheckForWriteAccess(inf);
            if (!hasWriteAccess)
            {
                return false;
            }
            try
            {
                _userInterface = inf;

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
            catch (TagNotFoundException)
            {
                throw;
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
                return t.ReadOnlyFilters;
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
                string logicalid = ProfilingLayer.Instance.GetLogicalIdFromDrive(drive);
                if (logicalid == null)
                {
                    //Drive does not exist in the profile
                    return true;
                }
                List<Tag> tagList = TaggingLayer.Instance.RetrieveTagByLogicalId(logicalid);
                foreach (Tag t in tagList)
                {
                    if (CompareAndSyncController.Instance.IsQueuedOrSyncing(t.TagName))
                    {
                        return false;
                    }
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
            try
            {
                return Cleaner.CleanSynclessMeta(new DirectoryInfo(path));
            }
            catch (Exception e)
            {
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                throw new UnhandledException(e);
            }
        }
        /// <summary>
        /// Merge a profile from a particular path. Will only merge profile with the same name.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool Merge(string path)
        {
            try
            {
                ProfilingLayer.Instance.Merge(path);
                TaggingLayer.Instance.Merge(path);
                return true;
            }
            catch (Exception e)
            {
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                throw new UnhandledException(e);
            }
        }
        /// <summary>
        /// Set the profile name
        /// </summary>
        /// <param name="profileName">name of the profile</param>
        /// <returns></returns>
        public bool SetProfileName(string profileName)
        {
            throw new NotImplementedException();
            //return false;
        }
        /// <summary>
        /// Get the current profile name
        /// </summary>
        /// <returns>Profile name</returns>
        public string GetProfileName()
        {
            throw new NotImplementedException();
            //return ProfilingLayer.Instance.CurrentProfile.ProfileName;
        }
        /// <summary>
        /// Set the name for a drive.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="driveName"></param>
        /// <returns></returns>
        public bool SetDriveName(DriveInfo info, string driveName)
        {
            throw new NotImplementedException();
            //ProfilingLayer.Instance.SetDriveName(info, driveName);
            //return false;
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
            catch (LogFileCorruptedException)
            {
                throw;
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
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="mode"></param>
        private void SetTagMode(Tag tag, bool mode)
        {
            tag.IsSeamless = mode;
            SaveLoadHelper.SaveAll(_userInterface.getAppPath());
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

            try
            {
                if (_switchingTable.ContainsKey(tag.TagName))
                {
                    _switchingTable.Remove(tag.TagName);
                }
            }
            catch (Exception e)
            {
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
            }
        }
        /// <summary>
        /// Manual Sync
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="switchSeamless">true will mean the tag switch to seamless after sync</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        private bool ManualSync(Tag tag, bool switchSeamless)
        {
            FindAndCleanDeletedPaths();
            if (CompareAndSyncController.Instance.IsQueuedOrSyncing(tag.TagName))
            {
                return false;
            }
            List<string> paths = tag.FilteredPathListString;
            List<string>[] filterPaths = ProfilingLayer.Instance.ConvertAndFilter(paths);
            if (filterPaths[0].Count < 2)
            {
                if (switchSeamless)
                {
                    SetTagMode(tag, true);
                }
                return true;
            }

            ManualSyncRequest syncRequest = new ManualSyncRequest(filterPaths[0].ToArray(), tag.Filters, SyncConfig.Instance, tag.TagName, switchSeamless);
            CompareAndSyncController.Instance.Sync(syncRequest);

            return true;
        }
        /// <summary>
        /// Manual Sync
        /// </summary>
        /// <param name="tagname"></param>
        /// <param name="switchSeamless"></param>
        /// <returns></returns>
        private bool ManualSync(string tagname, bool switchSeamless)
        {
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
            TagState state;
            _switchingTable.TryGetValue(tag.TagName, out state);
            if (state == TagState.Undefined)
            {
                _switchingTable.Add(tag.TagName, mode ? TagState.ManualToSeamless : TagState.SeamlessToManual);
            }

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
            FindAndCleanDeletedPaths();
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


            view.IsQueued = CompareAndSyncController.Instance.IsQueued(t.TagName);
            view.IsSyncing = CompareAndSyncController.Instance.IsSyncing(t.TagName);

            //if (t.IsSeamless)
            //{
            //    view.TagState = TagState.Seamless;
            //}
            //else
            //{
            //    if (view.IsLocked)
            //    {
            //        view.TagState = TagState.Switching;
            //    }
            //    else
            //    {
            //        view.TagState = TagState.Manual;
            //    }
            //}
            view.TagState = GetTagState(view.TagName);
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
                FindAndCleanDeletedPaths();
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
                _deletedTaggedPathWatcher = new DeletedTaggedPathWatcher();
                _deletedTaggedPathWatcher.Start();
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
                tempFilters.Add(FilterFactory.CreateArchiveFilter("_synclessArchive"));
                tempFilters.Add(FilterFactory.CreateConfigurationFilter());
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
        /// <summary>
        /// Find and Clean all the deleted Paths.
        /// </summary>
        private void FindAndCleanDeletedPaths()
        {
            List<string> deletedPaths = FindAllDeletedPaths();
            foreach (string paths in deletedPaths)
            {
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
            if (!folder.Exists) return;
            string convertedPath = ProfilingLayer.Instance.ConvertPhysicalToLogical(folder.FullName, false);
            if (convertedPath == null || convertedPath.Equals("")) return;
            // See if there is any tag still contain this folder.
            List<Tag> tagList = TaggingLayer.Instance.RetrieveTagByPath(convertedPath);

            if (tagList.Count > 0) return; //Still have tag contain the folder , do not attempt to clean.

            List<string> parentPaths = TaggingLayer.Instance.RetrieveAncestors(convertedPath);
            if (parentPaths.Count != 0) return;//Parent still tagged. Do not clean.

            List<string> childPaths = TaggingLayer.Instance.RetrieveDescendants(convertedPath);
            List<string> convertedList = ProfilingLayer.Instance.ConvertAndFilterToPhysical(childPaths);

            Cleaner.CleanSynclessMeta(folder, convertedList);
        }
        /// <summary>
        /// Clean a tag before deleting
        /// </summary>
        /// <param name="t">The Tag to clean.</param>
        private delegate void DeleteTagCleanDelegate(Tag t);
        private void DeleteTagClean(Tag t)
        {
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
        /// 
        /// </summary>
        /// <param name="inf"></param>
        /// <returns></returns>
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

        private List<string> ConvertTagListToTagString(List<Tag> tagList)
        {
            List<string> tagStringList = new List<string>();
            foreach (Tag tag in tagList)
            {
                tagStringList.Add(tag.TagName);
            }
            return tagStringList;
        }

        #endregion

        #region For Notification // SideThread
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
                TaggingLayer.Instance.DeleteTag(tag.TagName);
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
            _userInterface.TagChanged(tagname);
        }
        /// <summary>
        /// Inform The Tagging Layer to untag a particular path as it is no longer available.
        /// </summary>
        /// <param name="pathList"></param>
        internal void Untag(List<string> pathList)
        {
            foreach (string path in pathList)
            {
                string convertedPath = ProfilingLayer.Instance.ConvertPhysicalToLogical(path, false);
                TaggingLayer.Instance.UntagFolder(convertedPath);
            }
            _userInterface.TagsChanged();
        }
        #endregion

    }
}
