using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Syncless.CompareAndSync.Enum;
using Syncless.CompareAndSync.Request;
using Syncless.CompareAndSync.Exceptions;
using Syncless.Core;
using Syncless.CompareAndSync.XMLWriteObject;
using Syncless.Logging;

namespace Syncless.CompareAndSync
{
    public static class SeamlessSyncer
    {
        private static long metaUpdated;

        public static void Sync(AutoSyncRequest request)
        {
            metaUpdated = DateTime.Now.Ticks;
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
                            XMLHelper.UpdateXML(new XMLWriteFileObject(request.SourceName, request.SourceParent,
                                                                       CommonMethods.CalculateMD5Hash(currFile),
                                                                       currFile.Length, currFile.CreationTime.Ticks,
                                                                       currFile.LastWriteTime.Ticks, MetaChangeType.New,
                                                                       metaUpdated));
                            break;
                        case AutoSyncRequestType.Update:
                            XMLHelper.UpdateXML(new XMLWriteFileObject(request.SourceName, request.SourceParent,
                                                                       CommonMethods.CalculateMD5Hash(currFile),
                                                                       currFile.Length, currFile.CreationTime.Ticks,
                                                                       currFile.LastWriteTime.Ticks,
                                                                       MetaChangeType.Update, metaUpdated));
                            break;
                        case AutoSyncRequestType.Rename:
                            XMLHelper.UpdateXML(new XMLWriteFileObject(request.OldName, request.NewName,
                                                                       request.SourceParent,
                                                                       CommonMethods.CalculateMD5Hash(currFile),
                                                                       currFile.Length, currFile.CreationTime.Ticks,
                                                                       currFile.LastWriteTime.Ticks,
                                                                       MetaChangeType.Rename, metaUpdated));
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
                                    if (request.Config.ArchiveLimit >= 0)
                                        CommonMethods.ArchiveFile(destFullPath, request.Config.ArchiveName, request.Config.ArchiveLimit);
                                    if (request.Config.Recycle)
                                        CommonMethods.DeleteFileToRecycleBin(destFullPath);
                                    CommonMethods.CopyFile(sourceFullPath, destFullPath, true);
                                    currFile = new FileInfo(destFullPath);
                                    XMLHelper.UpdateXML(new XMLWriteFileObject(request.SourceName, dest, CommonMethods.CalculateMD5Hash(currFile), currFile.Length, currFile.CreationTime.Ticks, currFile.LastWriteTime.Ticks, request.ChangeType == AutoSyncRequestType.New ? MetaChangeType.New : MetaChangeType.Update, metaUpdated));
                                    break;
                                case AutoSyncRequestType.Rename:
                                    string oldFullPath = Path.Combine(dest, request.OldName);
                                    string newFullPath = Path.Combine(dest, request.NewName);
                                    if (!File.Exists(oldFullPath))
                                        CommonMethods.CopyFile(sourceFullPath, newFullPath, true);
                                    else
                                        CommonMethods.MoveFile(oldFullPath, newFullPath);
                                    currFile = new FileInfo(newFullPath);
                                    XMLHelper.UpdateXML(new XMLWriteFileObject(request.OldName, request.NewName, dest, CommonMethods.CalculateMD5Hash(currFile), currFile.Length, currFile.CreationTime.Ticks, currFile.LastWriteTime.Ticks, MetaChangeType.Rename, metaUpdated));
                                    break;
                            }
                        }
                        catch (ArchiveFileException e)
                        {
                            //TODO: Throw to notification queue in future
                            ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                        }
                        catch (DeleteFileException e)
                        {
                            //TODO: Throw to notification queue in future
                            ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                        }
                        catch (CopyFileException e)
                        {
                            //TODO: Throw to notification queue in future
                            ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                        }
                        catch (MoveFileException e)
                        {
                            //TODO: Throw to notification queue in future
                            ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                        }
                    }
                }
            }
            else if (request.ChangeType == AutoSyncRequestType.Delete)
            {
                string destFullPath = null;
                XMLHelper.UpdateXML(new XMLWriteFileObject(request.SourceName, request.SourceParent, MetaChangeType.Delete, metaUpdated));

                foreach (string dest in request.DestinationFolders)
                {
                    destFullPath = Path.Combine(dest, request.SourceName);                    
                    if (File.Exists(destFullPath))
                    {
                        try
                        {
                            CommonMethods.CalculateMD5Hash(new FileInfo(destFullPath));
                            if (request.Config.ArchiveLimit >= 0)
                                CommonMethods.ArchiveFile(destFullPath, request.Config.ArchiveName, request.Config.ArchiveLimit);
                            if (request.Config.Recycle)
                                CommonMethods.DeleteFileToRecycleBin(destFullPath);
                            else
                                CommonMethods.DeleteFile(destFullPath);
                            XMLHelper.UpdateXML(new XMLWriteFileObject(request.SourceName, dest, MetaChangeType.Delete, metaUpdated));
                        }
                        catch (ArchiveFileException e)
                        {
                            //TODO: Throw to notification queue in future
                            ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                        }
                        catch (DeleteFileException e)
                        {
                            //TODO: Throw to notification queue in future
                            ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
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
            catch (HashFileException e)
            {
                //TODO: Throw to notification queue in future
                ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                return true;
            }
        }

        #endregion

        #region Folders

        private static void SyncFolder(AutoSyncRequest request)
        {
            string sourceFullPath = Path.Combine(request.SourceParent, request.ChangeType == AutoSyncRequestType.Rename ? request.NewName : request.SourceName);
            DirectoryInfo currFolder = null;

            if (Directory.Exists(sourceFullPath))
            {
                currFolder = new DirectoryInfo(sourceFullPath);
                switch (request.ChangeType)
                {
                    case AutoSyncRequestType.New:
                        XMLHelper.UpdateXML(new XMLWriteFolderObject(request.SourceName, request.SourceParent, currFolder.CreationTime.Ticks, MetaChangeType.New, metaUpdated));
                        break;
                    case AutoSyncRequestType.Rename:
                        XMLHelper.UpdateXML(new XMLWriteFolderObject(request.OldName, request.NewName, request.SourceParent, currFolder.CreationTime.Ticks, MetaChangeType.Rename, metaUpdated));
                        break;
                }

                string destFullPath = null;

                foreach (string dest in request.DestinationFolders)
                {
                    destFullPath = Path.Combine(dest, request.SourceName);

                    try
                    {
                        switch (request.ChangeType)
                        {
                            case AutoSyncRequestType.New:
                                CommonMethods.CreateFolder(destFullPath);
                                currFolder = new DirectoryInfo(destFullPath);
                                XMLHelper.UpdateXML(new XMLWriteFolderObject(request.SourceName, dest, currFolder.CreationTime.Ticks, MetaChangeType.New, metaUpdated));
                                break;
                            case AutoSyncRequestType.Rename:
                                string oldFullPath = Path.Combine(dest, request.OldName);
                                string newFullPath = Path.Combine(dest, request.NewName);
                                if (!Directory.Exists(newFullPath))
                                {
                                    CommonMethods.MoveFolder(oldFullPath, newFullPath);
                                    currFolder = new DirectoryInfo(newFullPath);
                                    XMLHelper.UpdateXML(new XMLWriteFolderObject(request.OldName, request.NewName, dest, currFolder.CreationTime.Ticks, MetaChangeType.Rename, metaUpdated));
                                }
                                break;
                        }
                    }
                    catch (CreateFolderException e)
                    {
                        //TODO: Throw to notification queue in future
                        ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                    }
                    catch (MoveFolderException e)
                    {
                        //TODO: Throw to notification queue in future
                        ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                    }
                }
            }
            else if (request.ChangeType == AutoSyncRequestType.Delete)
            {
                string destFullPath = null;
                XMLHelper.UpdateXML(new XMLWriteFolderObject(request.SourceName, request.SourceParent, MetaChangeType.Delete, metaUpdated));

                foreach (string dest in request.DestinationFolders)
                {
                    destFullPath = Path.Combine(dest, request.SourceName);
                    if (Directory.Exists(destFullPath))
                    {
                        try
                        {
                            if (request.Config.ArchiveLimit >= 0)
                                CommonMethods.ArchiveFolder(destFullPath, request.Config.ArchiveName, request.Config.ArchiveLimit);
                            if (request.Config.Recycle)
                                CommonMethods.DeleteFolderToRecycleBin(destFullPath);
                            else
                                CommonMethods.DeleteFolder(destFullPath, true);
                            XMLHelper.UpdateXML(new XMLWriteFolderObject(request.SourceName, dest, MetaChangeType.Delete, metaUpdated));
                        }
                        catch (ArchiveFolderException e)
                        {
                            //TODO: Throw to notification queue in future
                            ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                        }
                        catch (DeleteFolderException e)
                        {
                            //TODO: Throw to notification queue in future
                            ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                        }
                    }
                }
            }

        }

        #endregion

        private static bool IsFolder(string sourceName, string sourceParent, List<string> destinations)
        {
            bool result = false;
            string destTest;

            for (int i = 0; i < destinations.Count; i++)
            {
                destTest = Path.Combine(destinations[i], sourceName);
                if (Directory.Exists(destTest))
                {
                    result = true;
                    break;
                }
                else if (File.Exists(destTest))
                {
                    result = false;
                    break;
                }
            }

            return result;
        }
    }
}
