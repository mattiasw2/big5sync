using System;
using System.Collections.Generic;
using System.IO;
using Syncless.CompareAndSync.Enum;
using Syncless.CompareAndSync.Exceptions;
using Syncless.CompareAndSync.Request;
using Syncless.CompareAndSync.XMLWriteObject;
using Syncless.Core;
using Syncless.Logging;

namespace Syncless.CompareAndSync.Seamless
{
    public static class SeamlessSyncer
    {
        private static long _metaUpdated;

        public static void Sync(AutoSyncRequest request)
        {
            ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.SYNC_STARTED, "Started Auto Sync for " + request.SourceName));

            _metaUpdated = DateTime.Now.Ticks;
            bool? isFolder = request.IsFolder;
            bool isFldr;

            if (isFolder.HasValue)
                isFldr = (bool)isFolder;
            else
                isFldr = IsFolder(request.SourceName, request.SourceParent, request.DestinationFolders);

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
            FileInfo currFile;

            if (File.Exists(sourceFullPath))
            {
                currFile = new FileInfo(sourceFullPath);
                try
                {
                    switch (request.ChangeType)
                    {
                        case AutoSyncRequestType.New:
                            SeamlessXMLHelper.UpdateXML(new XMLWriteFileObject(request.SourceName, request.SourceParent,
                                                                               CommonMethods.CalculateMD5Hash(currFile),
                                                                               currFile.Length, currFile.CreationTime.Ticks,
                                                                               currFile.LastWriteTime.Ticks, MetaChangeType.New,
                                                                               _metaUpdated));
                            break;
                        case AutoSyncRequestType.Update:
                            SeamlessXMLHelper.UpdateXML(new XMLWriteFileObject(request.SourceName, request.SourceParent,
                                                                               CommonMethods.CalculateMD5Hash(currFile),
                                                                               currFile.Length, currFile.CreationTime.Ticks,
                                                                               currFile.LastWriteTime.Ticks,
                                                                               MetaChangeType.Update, _metaUpdated));
                            break;
                        case AutoSyncRequestType.Rename:
                            SeamlessXMLHelper.UpdateXML(new XMLWriteFileObject(request.OldName, request.NewName,
                                                                               request.SourceParent,
                                                                               CommonMethods.CalculateMD5Hash(currFile),
                                                                               currFile.Length, currFile.CreationTime.Ticks,
                                                                               currFile.LastWriteTime.Ticks,
                                                                               MetaChangeType.Rename, _metaUpdated));
                            break;
                    }
                }
                catch (HashFileException)
                {
                    ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ERROR, "Error hashing " + sourceFullPath + "."));
                    return;
                }

                foreach (string dest in request.DestinationFolders)
                {
                    string destFullPath = Path.Combine(dest, request.SourceName);
                    if (DoSync(sourceFullPath, destFullPath))
                    {
                        try
                        {
                            switch (request.ChangeType)
                            {
                                case AutoSyncRequestType.Update:
                                case AutoSyncRequestType.New:
                                    try
                                    {
                                        if (request.Config.ArchiveLimit >= 0 && File.Exists(destFullPath))
                                        {
                                            CommonMethods.ArchiveFile(destFullPath, request.Config.ArchiveName,
                                                                      request.Config.ArchiveLimit);
                                            ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(
                                                new LogData(LogEventType.FSCHANGE_ARCHIVED,
                                                            "File archived " + destFullPath));
                                        }
                                        if (request.Config.Recycle && File.Exists(destFullPath))
                                        {
                                            CommonMethods.DeleteFileToRecycleBin(destFullPath);
                                            ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(
                                                new LogData(LogEventType.FSCHANGE_DELETED,
                                                            "File deleted to recycle bin " + destFullPath));
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
                                    CommonMethods.CopyFile(sourceFullPath, destFullPath, true);
                                    if (File.Exists(destFullPath))
                                        ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_MODIFIED, "File updated from " + sourceFullPath + " to " + destFullPath));
                                    else
                                        ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_CREATED, "File copied from " + sourceFullPath + " to " + destFullPath));
                                    currFile = new FileInfo(destFullPath);
                                    SeamlessXMLHelper.UpdateXML(new XMLWriteFileObject(request.SourceName, dest, CommonMethods.CalculateMD5Hash(currFile), currFile.Length, currFile.CreationTime.Ticks, currFile.LastWriteTime.Ticks, request.ChangeType == AutoSyncRequestType.New ? MetaChangeType.New : MetaChangeType.Update, _metaUpdated));
                                    break;
                                case AutoSyncRequestType.Rename:
                                    string oldFullPath = Path.Combine(dest, request.OldName);
                                    string newFullPath = Path.Combine(dest, request.NewName);
                                    if (!File.Exists(oldFullPath))
                                        CommonMethods.CopyFile(sourceFullPath, newFullPath, true);
                                    else
                                        CommonMethods.MoveFile(oldFullPath, newFullPath);
                                    currFile = new FileInfo(newFullPath);
                                    SeamlessXMLHelper.UpdateXML(new XMLWriteFileObject(request.OldName, request.NewName, dest, CommonMethods.CalculateMD5Hash(currFile), currFile.Length, currFile.CreationTime.Ticks, currFile.LastWriteTime.Ticks, MetaChangeType.Rename, _metaUpdated));
                                    break;
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
                            return;
                        }
                    }
                }
            }
            else if (request.ChangeType == AutoSyncRequestType.Delete)
            {
                SeamlessXMLHelper.UpdateXML(new XMLWriteFileObject(request.SourceName, request.SourceParent, MetaChangeType.Delete, _metaUpdated));

                foreach (string dest in request.DestinationFolders)
                {
                    string destFullPath = Path.Combine(dest, request.SourceName);
                    if (File.Exists(destFullPath))
                    {
                        try
                        {
                            CommonMethods.CalculateMD5Hash(new FileInfo(destFullPath));
                            if (request.Config.ArchiveLimit >= 0)
                            {
                                CommonMethods.ArchiveFile(destFullPath, request.Config.ArchiveName,
                                                          request.Config.ArchiveLimit);
                                ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(
                                    new LogData(LogEventType.FSCHANGE_ARCHIVED, "File archived " + destFullPath));
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
                        catch (HashFileException)
                        {
                            ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ERROR, "Error hashing " + sourceFullPath + "."));
                            return;
                        }
                    }
                }
            }
        }

        private static bool DoSync(string sourceFullPath, string destFullPath)
        {
            FileInfo sourceFile = new FileInfo(sourceFullPath);
            FileInfo destFile = new FileInfo(destFullPath);

            try
            {
                return (CommonMethods.CalculateMD5Hash(sourceFile) != CommonMethods.CalculateMD5Hash(destFile));
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
                DirectoryInfo currFolder = new DirectoryInfo(sourceFullPath);
                switch (request.ChangeType)
                {
                    case AutoSyncRequestType.New:
                        SeamlessXMLHelper.UpdateXML(new XMLWriteFolderObject(request.SourceName, request.SourceParent, currFolder.CreationTime.Ticks, MetaChangeType.New, _metaUpdated));
                        break;
                    case AutoSyncRequestType.Rename:
                        SeamlessXMLHelper.UpdateXML(new XMLWriteFolderObject(request.OldName, request.NewName, request.SourceParent, currFolder.CreationTime.Ticks, MetaChangeType.Rename, _metaUpdated));
                        break;
                }

                foreach (string dest in request.DestinationFolders)
                {
                    string destFullPath = Path.Combine(dest, request.SourceName);
                    string oldFullPath = string.Empty;
                    string newFullPath = string.Empty;

                    try
                    {
                        switch (request.ChangeType)
                        {
                            case AutoSyncRequestType.New:
                                CommonMethods.CreateFolder(destFullPath);
                                ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_CREATED, "Folder created " + destFullPath));
                                currFolder = new DirectoryInfo(destFullPath);
                                SeamlessXMLHelper.UpdateXML(new XMLWriteFolderObject(request.SourceName, dest, currFolder.CreationTime.Ticks, MetaChangeType.New, _metaUpdated));
                                break;
                            case AutoSyncRequestType.Rename:
                                oldFullPath = Path.Combine(dest, request.OldName);
                                newFullPath = Path.Combine(dest, request.NewName);
                                if (!Directory.Exists(newFullPath))
                                {
                                    CommonMethods.MoveFolder(oldFullPath, newFullPath);
                                    ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_CREATED, "Folder renamed from " + oldFullPath + " to " + newFullPath));
                                    currFolder = new DirectoryInfo(newFullPath);
                                    SeamlessXMLHelper.UpdateXML(new XMLWriteFolderObject(request.OldName, request.NewName, dest, currFolder.CreationTime.Ticks, MetaChangeType.Rename, _metaUpdated));
                                }
                                break;
                        }
                    }
                    catch (CreateFolderException)
                    {
                        ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ERROR, "Error creating folder " + destFullPath));
                    }
                    catch (MoveFolderException)
                    {
                        ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ERROR, "Error renaming folder from " + oldFullPath + " to " + newFullPath));
                    }
                }
            }
            else if (request.ChangeType == AutoSyncRequestType.Delete)
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
                                CommonMethods.ArchiveFolder(destFullPath, request.Config.ArchiveName,
                                                            request.Config.ArchiveLimit);
                                ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(
                                    new LogData(LogEventType.FSCHANGE_ARCHIVED, "Folder archived " + destFullPath));
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
                                CommonMethods.DeleteFolder(destFullPath, true);
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

        }

        #endregion

        private static bool IsFolder(string sourceName, string sourceParent, List<string> destinations)
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