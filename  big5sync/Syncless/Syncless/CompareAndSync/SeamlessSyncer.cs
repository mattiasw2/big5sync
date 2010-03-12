using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Syncless.CompareAndSync.Enum;
using Syncless.CompareAndSync.Request;
using Syncless.CompareAndSync.Exceptions;

namespace Syncless.CompareAndSync
{
    public class SeamlessSyncer
    {
        public static void Sync(AutoSyncRequest request)
        {
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

            if (File.Exists(sourceFullPath))
            {
                string destFullPath = null;

                foreach (string dest in request.DestinationFolders)
                {
                    destFullPath = Path.Combine(dest, request.SourceName);
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
                                    break;
                                case AutoSyncRequestType.Rename:
                                    string oldFullPath = Path.Combine(dest, request.OldName);
                                    string newFullPath = Path.Combine(dest, request.NewName);
                                    if (!File.Exists(oldFullPath))
                                        CommonMethods.CopyFile(sourceFullPath, newFullPath, true);
                                    else
                                        CommonMethods.MoveFile(oldFullPath, newFullPath);
                                    break;
                            }
                        }
                        catch (ArchiveFileException e)
                        {
                        }
                        catch (DeleteFileException e)
                        {
                        }
                        catch (CopyFileException e)
                        {
                        }
                        catch (MoveFileException e)
                        {
                        }
                    }
                }
            }
            else if (request.ChangeType == AutoSyncRequestType.Delete)
            {
                string destFullPath = null;

                foreach (string dest in request.DestinationFolders)
                {
                    destFullPath = Path.Combine(dest, request.SourceName);
                    if (File.Exists(destFullPath))
                    {
                        try
                        {
                            if (request.Config.ArchiveLimit >= 0)
                                CommonMethods.ArchiveFile(destFullPath, request.Config.ArchiveName, request.Config.ArchiveLimit);
                            if (request.Config.Recycle)
                                CommonMethods.DeleteFileToRecycleBin(destFullPath);
                            else
                                CommonMethods.DeleteFile(destFullPath);
                        }
                        catch (ArchiveFileException e)
                        {
                        }
                        catch (DeleteFileException e)
                        {
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
                string destFullPath = null;

                foreach (string dest in request.DestinationFolders)
                {
                    destFullPath = Path.Combine(dest, request.SourceName);
                    if (DoSync(sourceFullPath, destFullPath))
                    {
                        try
                        {
                            switch (request.ChangeType)
                            {
                                case AutoSyncRequestType.New:
                                    CommonMethods.CreateFolder(destFullPath);
                                    break;
                                case AutoSyncRequestType.Rename:
                                    string oldFullPath = Path.Combine(dest, request.OldName);
                                    string newFullPath = Path.Combine(dest, request.NewName);
                                    CommonMethods.MoveFolder(oldFullPath, newFullPath);
                                    break;
                            }
                        }
                        catch (CreateFolderException e)
                        {
                        }
                        catch (MoveFolderException e)
                        {
                        }
                    }
                }
            }
            else if (request.ChangeType == AutoSyncRequestType.Delete)
            {
                string destFullPath = null;

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
                        }
                        catch (ArchiveFolderException e)
                        {
                        }
                        catch (DeleteFolderException e)
                        {
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
                destTest = Path.Combine(destinations[0], sourceName);
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
