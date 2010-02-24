using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Syncless.CompareAndSync
{
    public class Syncer
    {
        public List<SyncResult> SyncFile(List<string> paths, List<CompareResult> results)
        {
            return null;
        }

        public List<SyncResult> SyncFolder(List<string> paths, List<CompareResult> results)
        {
            List<SyncResult> syncResults = new List<SyncResult>();
            SyncResult currResult = null;

            foreach (CompareResult result in results)
            {
                switch (result.ChangeType)
                {
                    case FileChangeType.Create:
                        currResult = Create(result);                                                  
                        break;
                    case FileChangeType.Delete:
                        currResult = Delete(result);                    
                        break;
                    case FileChangeType.Rename:
                        currResult = Move(result);                   
                        break;
                    case FileChangeType.Update:
                        currResult = Update(result);
                        break;
                }

                syncResults.Add(currResult);
                if (currResult.Success)
                {
                    //YC: Write XML
                    //Check if it's folder or file and write accordingly?
                    //Hash, Filename, Filesize, Creation Time, Last Write Time
                }
            }

            //TODO: Will handle each change separately in future
            foreach (string path in paths)
            {
                XMLHelper.GenerateXMLFile(path);
                //RemoveEmptyFolders(path);
            }

            return syncResults;
        }

        public bool ModifyXml(string xmlPath , FileChangeType type , string changedFilePath)
        {
            XMLHelper.EditXml(xmlPath, type, changedFilePath);
            return false; //TEMP
        }

        #region Create

        private SyncResult Create(CompareResult result)
        {
            if (result is FileCompareResult)
            {
                return CreateFile(result.From, result.To);
            }
            else
            {
                return CreateFolder(result.From, result.To);
            }
        }

        private SyncResult CreateFile(string from, string to)
        {
            return CopyFile(from, to, FileChangeType.Create);
        }

        private SyncResult CreateFolder(string from, string to)
        {
            DirectoryInfo source = new DirectoryInfo(from);
            DirectoryInfo target = new DirectoryInfo(to);

            if (!target.Exists)
            {
                Directory.CreateDirectory(target.FullName);
            }

            try
            {
                CopyDirectory(from, to, true);
                return new SyncResult(FileChangeType.Create, from, to, true);
            }
            catch (Exception)
            {
                return new SyncResult(FileChangeType.Create, from, to, false);
            }
        }

        #endregion

        #region Move

        private SyncResult Move(CompareResult result)
        {
            if (result is FileCompareResult)
            {
                return MoveFile(result.From, result.To);
            }
            else
            {
                return MoveFolder(result.From, result.To);
            }
        }

        private SyncResult MoveFile(string from, string to)
        {
            FileInfo source = new FileInfo(from);
            FileInfo target = new FileInfo(to);

            try
            {
                if (!target.Exists)
                {
                    if (!Directory.Exists(target.Directory.FullName))
                    {
                        Directory.CreateDirectory(target.Directory.FullName);
                    }
                }
                File.Move(from, to);
                return new SyncResult(FileChangeType.Rename, from, to, true);
            }
            catch (Exception)
            {
                return new SyncResult(FileChangeType.Rename, from, to, false);
            }
        }

        private SyncResult MoveFolder(string from, string to)
        {
            DirectoryInfo source = new DirectoryInfo(from);
            DirectoryInfo target = new DirectoryInfo(to);

            try
            {
                Directory.Move(from, to);
                return new SyncResult(FileChangeType.Rename, from, to, true);
            }
            catch (Exception)
            {
                return new SyncResult(FileChangeType.Rename, from, to, false);
            }
        }

        #endregion

        #region Delete

        private SyncResult Delete(CompareResult result)
        {
            if (result is FileCompareResult)
            {
                return DeleteFile(result.From);
            }
            else
            {
                return DeleteFolder(result.From);
            }
        }

        private SyncResult DeleteFile(string from)
        {
            try
            {
                File.Delete(from);
                return new SyncResult(FileChangeType.Delete, from, true);
            }
            catch (Exception)
            {
                return new SyncResult(FileChangeType.Delete, from, false);
            }
        }

        private SyncResult DeleteFolder(string from)
        {
            try
            {
                Directory.Delete(from);
                return new SyncResult(FileChangeType.Delete, from, true);
            }
            catch (Exception)
            {
                return new SyncResult(FileChangeType.Delete, from, false);
            }
        }
        
        #endregion

        #region Update

        private SyncResult Update(CompareResult result)
        {
            if (result is FileCompareResult)
            {
                return UpdateFile(result.From, result.To);
            }
            else
            {
                return null;
            }
        }

        private SyncResult UpdateFile(string from, string to)
        {
            return CopyFile(from, to, FileChangeType.Update);
        }

        #endregion

        #region Helper Methods

        private SyncResult CopyFile(string from, string to, FileChangeType changeType)
        {
            FileInfo source = new FileInfo(from);
            FileInfo target = new FileInfo(to);
            
            if (!target.Exists)
            {
                if (!Directory.Exists(target.Directory.FullName))
                {
                    Directory.CreateDirectory(target.Directory.FullName);
                }               
            }
            
            try {
                source.CopyTo(to, true);
                return new SyncResult(changeType, from, to, true);
            }
            catch (Exception)
            {
                return new SyncResult(changeType, from, to, false);
            }
        }

        public static string CopyDirectory(string source, string destination, bool overwrite)
        {
            DirectoryInfo sourceInfo = new DirectoryInfo(source);
            DirectoryInfo[] directoryInfos = sourceInfo.GetDirectories();
            DirectoryInfo destinationInfo = new DirectoryInfo(destination);
            if (!destinationInfo.Exists)
            {
                Directory.CreateDirectory(destination);
            }
            foreach (DirectoryInfo tempInfo in directoryInfos)
            {
                DirectoryInfo newDirectory = destinationInfo.CreateSubdirectory(tempInfo.Name);
                CopyDirectory(tempInfo.FullName, newDirectory.FullName, overwrite);
            }
            FileInfo[] fileInfos = sourceInfo.GetFiles();
            foreach (FileInfo fileInfo in fileInfos)
            {
                fileInfo.CopyTo(destination + "\\" + fileInfo.Name, overwrite);
            }
            return destinationInfo.FullName;
        }

        private void RemoveEmptyFolders(string path)
        {
            DirectoryInfo[] directories = new DirectoryInfo(path).GetDirectories("*", SearchOption.AllDirectories);
            foreach (DirectoryInfo dInfo in directories)
            {
                FileInfo[] files = dInfo.GetFiles();
                if (files.Length == 0)
                {
                    dInfo.Delete();
                }
            }
        }

        #endregion

    }
}
