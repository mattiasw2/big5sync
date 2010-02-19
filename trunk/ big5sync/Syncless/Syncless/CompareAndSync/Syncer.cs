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
            foreach (CompareResult result in results)
            {
                switch (result.ChangeType)
                {
                    case FileChangeType.Create:
                        if (result.IsFolder)
                        {
                            syncResults.Add(CreateFolder(result.From, result.To));
                        }
                        else
                        {
                            syncResults.Add(CreateFile(result.From, result.To));
                        }
                        break;
                    case FileChangeType.Delete:
                        if (result.IsFolder)
                        {
                            syncResults.Add(DeleteFolder(result.From));
                        }
                        else
                        {
                            syncResults.Add(DeleteFile(result.From));
                        }                       
                        break;
                    case FileChangeType.Rename:
                        if (result.IsFolder)
                        {
                            syncResults.Add(MoveFolder(result.From, result.To));
                        }
                        else
                        {
                            syncResults.Add(MoveFile(result.From, result.To));
                        }
                        break;
                    case FileChangeType.Update:
                        syncResults.Add(UpdateFile(result.From, result.To));
                        break;
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

        private SyncResult MoveFolder(string from, string to)
        {
            DirectoryInfo source = new DirectoryInfo(from);
            DirectoryInfo target = new DirectoryInfo(to);

            try
            {
                if (!target.Exists)
                {
                    Directory.CreateDirectory(target.FullName);
                }
                Directory.Move(from, to);
                return new SyncResult(FileChangeType.Rename, from, to, true);
            }
            catch (Exception)
            {
                Console.WriteLine("RENAME FROM: " + from);
                Console.WriteLine("RENAME TO: " + to);
                return new SyncResult(FileChangeType.Rename, from, to, false);
                

            }
        }

        private SyncResult CreateFolder(string from, string to)
        {
            DirectoryInfo source = new DirectoryInfo(from);
            DirectoryInfo target = new DirectoryInfo(to);

            if (!target.Exists)
            {
                Directory.CreateDirectory(target.FullName);
            }

            Console.WriteLine("YCYCYC: CREATE FOLDER");

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

        private SyncResult CreateFile(string from, string to)
        {
            return CopyFile(from, to, FileChangeType.Create);
        }

        private SyncResult UpdateFile(string from, string to)
        {
            return CopyFile(from, to, FileChangeType.Update);
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
    }
}
