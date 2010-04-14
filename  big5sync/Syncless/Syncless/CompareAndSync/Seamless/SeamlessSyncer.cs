/*
 * 
 * Author: Soh Yuan Chin
 * 
 */

using System;
using System.Collections.Generic;
using System.IO;
using Syncless.CompareAndSync.Enum;
using Syncless.CompareAndSync.Exceptions;
using Syncless.CompareAndSync.Request;
using Syncless.CompareAndSync.Seamless.XMLWriteObject;
using Syncless.Core;
using Syncless.Logging;

namespace Syncless.CompareAndSync.Seamless
{
    /// <summary>
    /// <c>SeamlessSyncer</c> contains all the methods for a seamless/auto synchronization job.
    /// </summary>
    public static class SeamlessSyncer
    {
        private static long _metaUpdated;

        /// <summary>
        /// Handles some basic logic to decide whether the incoming request is a file or folder request.
        /// </summary>
        /// <param name="request">The <see cref="AutoSyncRequest"/> to process.</param>
        public static void Sync(AutoSyncRequest request)
        {
            ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.SYNC_STARTED, "Started Auto Sync for " + request.SourceName));

            _metaUpdated = DateTime.UtcNow.Ticks;
            bool? isFolder = request.IsFolder;
            bool isFldr;

            if (isFolder.HasValue)
                isFldr = (bool)isFolder;
            else
                isFldr = IsFolder(request.SourceName, request.DestinationFolders);

            if (isFldr)
                SyncFolder(request);
            else
                SyncFile(request);

            ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.SYNC_STOPPED, "Completed Auto Sync for " + request.SourceName));
        }

        #region Files

        private static void SyncFile(AutoSyncRequest request)
        {
            string sourceFullPath = Path.Combine(request.SourceParent, request.ChangeType == AutoSyncRequestType.Rename ? request.NewName : request.SourceName);

            if (File.Exists(sourceFullPath))
            {
                switch (request.ChangeType)
                {
                    case AutoSyncRequestType.New:
                    case AutoSyncRequestType.Update:
                        HandleCreateUpdate(request, sourceFullPath);
                        break;
                    case AutoSyncRequestType.Rename:
                        HandleFileRename(request, sourceFullPath);
                        break;
                    case AutoSyncRequestType.Delete:
                        HandleFileDelete(request);
                        break;
                }
            }
            else if (request.ChangeType == AutoSyncRequestType.Delete)
            {
                HandleFileDelete(request);
            }
        }

        private static void HandleCreateUpdate(AutoSyncRequest request, string sourceFullPath)
        {
            try
            {
                FileInfo currFile = new FileInfo(sourceFullPath);

                switch (request.ChangeType)
                {
                    case AutoSyncRequestType.New:
                        SeamlessXMLHelper.UpdateXML(new XMLWriteFileObject(request.SourceName, request.SourceParent, CommonMethods.CalculateMD5Hash(currFile), currFile.Length, currFile.CreationTimeUtc.Ticks, currFile.LastWriteTimeUtc.Ticks, MetaChangeType.New, _metaUpdated));
                        break;
                    case AutoSyncRequestType.Update:
                        SeamlessXMLHelper.UpdateXML(new XMLWriteFileObject(request.SourceName, request.SourceParent, CommonMethods.CalculateMD5Hash(currFile), currFile.Length, currFile.CreationTimeUtc.Ticks, currFile.LastWriteTimeUtc.Ticks, MetaChangeType.Update, _metaUpdated));
                        break;
                    default:
                        break;
                }
            }
            catch (HashFileException)
            {
                ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ERROR, "Error hashing " + sourceFullPath + "."));
            }

            foreach (string dest in request.DestinationFolders)
            {
                string destFullPath = Path.Combine(dest, request.SourceName);

                if (DoSync(sourceFullPath, destFullPath))
                {
                    try
                    {
                        try
                        {
                            if (request.Config.ArchiveLimit >= 0 && File.Exists(destFullPath))
                            {
                                CommonMethods.ArchiveFile(destFullPath, request.Config.ArchiveName, request.Config.ArchiveLimit);
                                ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ARCHIVED, "File archived " + destFullPath));
                            }

                            if (request.Config.Recycle && File.Exists(destFullPath))
                            {
                                CommonMethods.DeleteFileToRecycleBin(destFullPath);
                                ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_DELETED, "File deleted to recycle bin " + destFullPath));
                            }
                        }
                        catch (ArchiveFileException)
                        {
                            ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ERROR, "Error archiving file " + destFullPath));
                        }
                        catch (DeleteFileException)
                        {
                            ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ERROR, "Error deleting file " + destFullPath));
                        }
                        CommonMethods.CopyFile(sourceFullPath, destFullPath);

                        if (File.Exists(destFullPath))
                            ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_MODIFIED, "File updated from " + sourceFullPath + " to " + destFullPath));
                        else
                            ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_CREATED, "File copied from " + sourceFullPath + " to " + destFullPath));

                        FileInfo destFile = new FileInfo(destFullPath);
                        SeamlessXMLHelper.UpdateXML(new XMLWriteFileObject(request.SourceName, dest, CommonMethods.CalculateMD5Hash(destFile), destFile.Length, destFile.CreationTimeUtc.Ticks, destFile.LastWriteTimeUtc.Ticks, request.ChangeType == AutoSyncRequestType.New ? MetaChangeType.New : MetaChangeType.Update, _metaUpdated));
                    }
                    catch (CopyFileException)
                    {
                        ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ERROR, "Error copying file from " + sourceFullPath + " to " + destFullPath));
                    }
                    catch (MoveFileException)
                    {
                        ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ERROR, "Error renaming file from " + sourceFullPath + " to " + destFullPath));
                    }
                    catch (HashFileException)
                    {
                        ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ERROR, "Error hashing " + sourceFullPath + "."));
                    }
                }
            }
        }

        private static void HandleFileRename(AutoSyncRequest request, string sourceFullPath)
        {
            SeamlessXMLHelper.UpdateXML(new XMLWriteFileObject(request.OldName, request.NewName, request.SourceParent, MetaChangeType.Rename, _metaUpdated));

            foreach (string dest in request.DestinationFolders)
            {
                string destFullPath = Path.Combine(dest, request.SourceName);

                if (DoSync(sourceFullPath, destFullPath))
                {
                    try
                    {
                        string oldFullPath = Path.Combine(dest, request.OldName);
                        string newFullPath = Path.Combine(dest, request.NewName);

                        if (!File.Exists(oldFullPath))
                        {
                            CommonMethods.CopyFile(sourceFullPath, newFullPath);
                            FileInfo destFile = new FileInfo(destFullPath);
                            SeamlessXMLHelper.UpdateXML(new XMLWriteFileObject(request.SourceName, dest, CommonMethods.CalculateMD5Hash(destFile), destFile.Length, destFile.CreationTimeUtc.Ticks, destFile.LastWriteTimeUtc.Ticks, MetaChangeType.New, _metaUpdated));
                        }
                        else
                        {
                            CommonMethods.MoveFile(oldFullPath, newFullPath);
                            SeamlessXMLHelper.UpdateXML(new XMLWriteFileObject(request.OldName, request.NewName, dest, MetaChangeType.Rename, _metaUpdated));
                        }
                    }
                    catch (CopyFileException)
                    {
                        ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ERROR, "Error copying file from " + sourceFullPath + " to " + destFullPath));
                    }
                    catch (MoveFileException)
                    {
                        ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ERROR, "Error renaming file from " + sourceFullPath + " to " + destFullPath));
                    }
                    catch (HashFileException)
                    {
                        ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ERROR, "Error hashing " + sourceFullPath + "."));
                    }
                }
            }
        }

        private static void HandleFileDelete(AutoSyncRequest request)
        {
            SeamlessXMLHelper.UpdateXML(new XMLWriteFileObject(request.SourceName, request.SourceParent, MetaChangeType.Delete, _metaUpdated));

            foreach (string dest in request.DestinationFolders)
            {
                string destFullPath = Path.Combine(dest, request.SourceName);

                if (File.Exists(destFullPath))
                {
                    try
                    {
                        if (request.Config.ArchiveLimit >= 0)
                        {
                            CommonMethods.ArchiveFile(destFullPath, request.Config.ArchiveName, request.Config.ArchiveLimit);
                            ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ARCHIVED, "File archived " + destFullPath));
                        }
                    }
                    catch (ArchiveFileException)
                    {
                        ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ERROR, "Error archiving file " + destFullPath));
                    }

                    try
                    {
                        if (request.Config.Recycle)
                        {
                            CommonMethods.DeleteFileToRecycleBin(destFullPath);
                            ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_DELETED, "File deleted to recycle bin " + destFullPath));
                        }
                        else
                        {
                            CommonMethods.DeleteFile(destFullPath);
                            ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_DELETED, "File deleted " + destFullPath));
                        }
                        SeamlessXMLHelper.UpdateXML(new XMLWriteFileObject(request.SourceName, dest, MetaChangeType.Delete, _metaUpdated));
                    }
                    catch (DeleteFileException)
                    {
                        ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ERROR, "Error deleting file " + destFullPath));
                    }
                }
            }
        }

        private static bool DoSync(string sourceFullPath, string destFullPath)
        {
            try
            {
                return (CommonMethods.CalculateMD5Hash(sourceFullPath) != CommonMethods.CalculateMD5Hash(destFullPath));
            }
            catch (HashFileException)
            {
                return true;
            }
        }

        #endregion

        #region Folders

        private static void SyncFolder(AutoSyncRequest request)
        {
            string sourceFullPath = Path.Combine(request.SourceParent, request.ChangeType == AutoSyncRequestType.Rename ? request.NewName : request.SourceName);

            if (Directory.Exists(sourceFullPath))
            {
                switch (request.ChangeType)
                {
                    case AutoSyncRequestType.New:
                        HandleFolderCreate(request, sourceFullPath);
                        break;
                    case AutoSyncRequestType.Rename:
                        HandleFolderRename(request);
                        break;
                }
            }
            else if (request.ChangeType == AutoSyncRequestType.Delete)
            {
                HandleFolderDelete(request);
            }
        }

        private static void HandleFolderCreate(AutoSyncRequest request, string sourceFullPath)
        {
            SeamlessXMLHelper.UpdateXML(new XMLWriteFolderObject(request.SourceName, request.SourceParent, Directory.GetCreationTimeUtc(sourceFullPath).Ticks, MetaChangeType.New, _metaUpdated));

            foreach (string dest in request.DestinationFolders)
            {
                string destFullPath = Path.Combine(dest, request.SourceName);

                try
                {
                    CommonMethods.CreateFolder(destFullPath);
                    ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_CREATED, "Folder created " + destFullPath));
                    SeamlessXMLHelper.UpdateXML(new XMLWriteFolderObject(request.SourceName, dest, Directory.GetCreationTimeUtc(destFullPath).Ticks, MetaChangeType.New, _metaUpdated));
                }
                catch (CreateFolderException)
                {
                    ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ERROR, "Error creating folder " + destFullPath));
                }
            }
        }

        private static void HandleFolderRename(AutoSyncRequest request)
        {
            SeamlessXMLHelper.UpdateXML(new XMLWriteFolderObject(request.OldName, request.NewName, request.SourceParent, MetaChangeType.Rename, _metaUpdated));

            foreach (string dest in request.DestinationFolders)
            {
                string oldFullPath = string.Empty;
                string newFullPath = string.Empty;

                try
                {
                    oldFullPath = Path.Combine(dest, request.OldName);
                    newFullPath = Path.Combine(dest, request.NewName);

                    if (!Directory.Exists(newFullPath))
                    {
                        CommonMethods.MoveFolder(oldFullPath, newFullPath);
                        ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_CREATED, "Folder renamed from " + oldFullPath + " to " + newFullPath));
                        SeamlessXMLHelper.UpdateXML(new XMLWriteFolderObject(request.OldName, request.NewName, dest, MetaChangeType.Rename, _metaUpdated));
                    }
                }
                catch (MoveFolderException)
                {
                    ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ERROR, "Error renaming folder from " + oldFullPath + " to " + newFullPath));
                }
            }

        }

        private static void HandleFolderDelete(AutoSyncRequest request)
        {
            SeamlessXMLHelper.UpdateXML(new XMLWriteFolderObject(request.SourceName, request.SourceParent, MetaChangeType.Delete, _metaUpdated));

            foreach (string dest in request.DestinationFolders)
            {
                string destFullPath = Path.Combine(dest, request.SourceName);
                if (Directory.Exists(destFullPath))
                {
                    try
                    {
                        if (request.Config.ArchiveLimit >= 0)
                        {
                            CommonMethods.ArchiveFolder(destFullPath, request.Config.ArchiveName, request.Config.ArchiveLimit);
                            ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ARCHIVED, "Folder archived " + destFullPath));
                        }
                    }
                    catch (ArchiveFolderException)
                    {
                        ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ERROR, "Error archiving folder " + destFullPath));
                    }

                    try
                    {
                        if (request.Config.Recycle)
                        {
                            CommonMethods.DeleteFolderToRecycleBin(destFullPath);
                            ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_DELETED, "Folder deleted to recycle bin " + destFullPath));
                        }
                        else
                        {
                            CommonMethods.DeleteFolder(destFullPath);
                            ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_DELETED, "Folder deleted " + destFullPath));
                        }
                        SeamlessXMLHelper.UpdateXML(new XMLWriteFolderObject(request.SourceName, dest, MetaChangeType.Delete, _metaUpdated));
                    }
                    catch (DeleteFolderException)
                    {
                        ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ERROR, "Error deleting folder " + destFullPath));
                    }
                }
            }
        }

        #endregion

        private static bool IsFolder(string sourceName, List<string> destinations)
        {
            bool result = false;

            for (int i = 0; i < destinations.Count; i++)
            {
                string destTest = Path.Combine(destinations[i], sourceName);
                if (Directory.Exists(destTest))
                {
                    result = true;
                    break;
                }
                if (File.Exists(destTest))
                    break;
            }

            return result;
        }
    }
}