using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Syncless.CompareAndSync.Enum;
using Syncless.CompareAndSync.Request;

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
                        switch (request.ChangeType)
                        {
                            case AutoSyncRequestType.Update:
                            case AutoSyncRequestType.New:
                                CopyFile(sourceFullPath, destFullPath);                                
                                break;
                            case AutoSyncRequestType.Rename:
                                string oldFullPath = Path.Combine(dest, request.OldName);
                                string newFullPath = Path.Combine(dest, request.NewName);
                                MoveFile(oldFullPath, newFullPath);
                                break;
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
                        DeleteFile(destFullPath);
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
            catch (FileNotFoundException)
            {
                return true;
            }
        }

        private static void DeleteFile(string destFullPath)
        {
            File.Delete(destFullPath);
        }

        private static void CopyFile(string sourceFullPath, string destFullPath)
        {
            File.Copy(sourceFullPath, destFullPath, true);
        }

        private static void MoveFile(string sourceFullPath, string destFullPath)
        {
            File.Move(sourceFullPath, destFullPath);
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
                        switch (request.ChangeType)
                        {
                            case AutoSyncRequestType.New:
                                CreateFolder(destFullPath);
                                break;
                            case AutoSyncRequestType.Rename:
                                string oldFullPath = Path.Combine(dest, request.OldName);
                                string newFullPath = Path.Combine(dest, request.NewName);
                                MoveFolder(oldFullPath, newFullPath);
                                break;
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
                        DeleteFolder(destFullPath);
                    }
                }
            }

        }

        private static void CreateFolder(string destFullPath)
        {
            if (!Directory.Exists(destFullPath))
                Directory.CreateDirectory(destFullPath);
        }

        private static void MoveFolder(string sourceFullPath, string destFullPath)
        {
            Directory.Move(sourceFullPath, destFullPath);
        }

        private static void DeleteFolder(string destFullPath)
        {
            Directory.Delete(destFullPath, true);
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
