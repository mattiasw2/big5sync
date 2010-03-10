using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Syncless.CompareAndSync.CompareObject;
using Syncless.CompareAndSync.Enum;

namespace Syncless.CompareAndSync.Visitor
{
    public class SyncerVisitor : IVisitor
    {
        #region IVisitor Members
        private readonly string ARCHIVENAME;
        private readonly int ARCHIVELIMIT;

        public SyncerVisitor(string archiveName, int archiveLimit)
        {
            ARCHIVENAME = archiveName;
            ARCHIVELIMIT = archiveLimit;
        }

        public void Visit(FileCompareObject file, string[] currentPaths)
        {
            int maxPriorityPos = 0;
            for (int i = 0; i < currentPaths.Length; i++)
            {
                if (file.Priority[i] > file.Priority[maxPriorityPos])
                    maxPriorityPos = i;
            }

            if (file.Priority[maxPriorityPos] > 0)
            {
                switch (file.ChangeType[maxPriorityPos])
                {
                    case MetaChangeType.Delete:
                        DeleteFile(file, currentPaths, maxPriorityPos);
                        break;
                    case MetaChangeType.New:
                    case MetaChangeType.Update:
                    case MetaChangeType.NoChange:
                        CopyFile(file, currentPaths, maxPriorityPos);
                        break;

                    case MetaChangeType.Rename:
                        MoveFile(file, currentPaths, maxPriorityPos);
                        break;
                }
            }

            //Basic logic: Look for highest priority and propagate it.

        }

        public void Visit(FolderCompareObject folder, string[] currentPaths)
        {
            int maxPriorityPos = 0;
            for (int i = 0; i < currentPaths.Length; i++)
            {
                if (folder.Priority[i] > folder.Priority[maxPriorityPos])
                    maxPriorityPos = i;
            }

            if (folder.Priority[maxPriorityPos] > 0)
            {
                switch (folder.ChangeType[maxPriorityPos])
                {
                    case MetaChangeType.Delete:
                        DeleteFolder(folder, currentPaths, maxPriorityPos);
                        break;
                    case MetaChangeType.New:
                        //case MetaChangeType.NoChange:
                        CreateFolder(folder, currentPaths, maxPriorityPos);
                        break;
                }
            }
        }

        public void Visit(RootCompareObject root)
        {
            //Do nothing
        }

        #endregion

        #region File Methods

        private void CopyFile(FileCompareObject fco, string[] currentPaths, int srcFilePos)
        {
            string src = Path.Combine(currentPaths[srcFilePos], fco.Name);
            bool fileExists = false;

            for (int i = 0; i < currentPaths.Length; i++)
            {
                if (i != srcFilePos && fco.Parent.FinalState[i] != FinalState.Deleted)
                {
                    try
                    {
                        if (fco.Priority[i] != fco.Priority[srcFilePos])
                        {
                            fileExists = File.Exists(Path.Combine(currentPaths[i], fco.Name));
                            File.Copy(src, Path.Combine(currentPaths[i], fco.Name), true);
                            fco.CreationTime[i] = fco.CreationTime[srcFilePos];
                            fco.Exists[i] = true;
                            if (fileExists)
                                fco.FinalState[i] = FinalState.Updated;
                            else
                                fco.FinalState[i] = FinalState.Created;
                            fco.Hash[i] = fco.Hash[srcFilePos];
                            fco.LastWriteTime[i] = fco.LastWriteTime[srcFilePos];
                            fco.Length[i] = fco.LastWriteTime[srcFilePos];
                        }
                        else
                        {
                            fco.FinalState[i] = FinalState.Unchanged;
                        }
                    }
                    catch (Exception)
                    {
                        //Throw to conflict queue
                    }
                }
            }
            fco.FinalState[srcFilePos] = FinalState.Propagated;
        }

        private void DeleteFile(FileCompareObject fco, string[] currentPaths, int srcFilePos)
        {
            for (int i = 0; i < currentPaths.Length; i++)
            {
                if (i != srcFilePos)
                {
                    if (fco.Priority[i] != fco.Priority[srcFilePos])
                    {
                        try
                        {
                            File.Delete(Path.Combine(currentPaths[i], fco.Name));
                            fco.Exists[i] = false;
                            fco.FinalState[i] = FinalState.Deleted;
                        }
                        catch (Exception)
                        {
                            //Throw to conflict queue
                        }
                    }
                    else
                    {
                        fco.FinalState[i] = FinalState.Unchanged;
                    }
                }
            }
            fco.FinalState[srcFilePos] = FinalState.Propagated;
        }

        private void MoveFile(FileCompareObject fco, string[] currentPaths, int srcFilePos)
        {
            for (int i = 0; i < currentPaths.Length; i++)
            {
                if (i != srcFilePos)
                {
                    if (fco.Priority[i] != fco.Priority[srcFilePos])
                    {
                        try
                        {
                            File.Move(Path.Combine(currentPaths[i], fco.Name), Path.Combine(currentPaths[i], fco.NewName));
                            fco.FinalState[i] = FinalState.Renamed;
                        }
                        catch (Exception)
                        {
                            //Throw to conflict queue
                        }
                    }
                    else
                    {
                        fco.FinalState[i] = FinalState.Unchanged;
                    }
                }
            }
            fco.FinalState[srcFilePos] = FinalState.Propagated;
        }

        #endregion

        #region Folder Methods

        private void CreateFolder(FolderCompareObject folder, string[] currentPaths, int srcFilePos)
        {
            for (int i = 0; i < currentPaths.Length; i++)
            {
                if (i != srcFilePos)
                {
                    if (folder.Priority[i] != folder.Priority[srcFilePos])
                    {
                        if (!Directory.Exists(Path.Combine(currentPaths[i], folder.Name)))
                        {
                            try
                            {
                                Directory.CreateDirectory(Path.Combine(currentPaths[i], folder.Name));
                                folder.Exists[i] = true;
                                folder.FinalState[i] = FinalState.Created;
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                    else
                    {
                        folder.FinalState[i] = FinalState.Unchanged;
                    }
                }
            }
            folder.FinalState[srcFilePos] = FinalState.Propagated;

        }

        private void DeleteFolder(FolderCompareObject folder, string[] currentPaths, int srcFilePos)
        {
            for (int i = 0; i < currentPaths.Length; i++)
            {
                if (i != srcFilePos)
                {
                    if (folder.Priority[i] != folder.Priority[srcFilePos])
                    {
                        try
                        {
                            Directory.Delete(Path.Combine(currentPaths[i], folder.Name), true);
                            folder.Exists[i] = false;
                            folder.FinalState[i] = FinalState.Deleted;
                            folder.Contents.Clear(); //Experimental
                        }
                        catch (Exception)
                        {
                            //Throw to conflict queue
                        }
                    }
                    else
                    {
                        folder.FinalState[i] = FinalState.Unchanged;
                    }
                }
            }
            folder.FinalState[srcFilePos] = FinalState.Propagated;
        }

        public void DeleteFolderToRecycleBin(string path)
        {
            Microsoft.VisualBasic.FileIO.FileSystem.DeleteDirectory(path, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
        }

        public void ArchiveFolder(string path)
        {
            FileInfo f = new FileInfo(path);
            string parent = f.DirectoryName;
            string archiveDir = Path.Combine(parent, ARCHIVENAME);
            if (!Directory.Exists(archiveDir))
                Directory.CreateDirectory(archiveDir);
            string currTime = String.Format("{0:yyyyMMddHHmmss}", DateTime.Now) + "_";

            CopyDirectory(path, Path.Combine(archiveDir, currTime + f.Name));

            DirectoryInfo d = new DirectoryInfo(archiveDir);
            DirectoryInfo[] files = d.GetDirectories();
            List<string> archivedFiles = new List<string>();

            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].Name.EndsWith(f.Name))
                {
                    archivedFiles.Add(files[i].Name);
                }
            }

            var sorted = (from element in archivedFiles orderby element descending select element).Skip(ARCHIVELIMIT);

            foreach (string s in sorted)
            {
                Directory.Delete(Path.Combine(archiveDir, s), true);
            }

        }

        public static string CopyDirectory(string source, string destination)
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
                CopyDirectory(tempInfo.FullName, newDirectory.FullName);
            }
            FileInfo[] fileInfos = sourceInfo.GetFiles();
            foreach (FileInfo fileInfo in fileInfos)
            {
                try
                {
                    fileInfo.CopyTo(destination + "\\" + fileInfo.Name, true);
                }
                catch (IOException)
                {
                    //File Exist?
                }
            }
            return destinationInfo.FullName;
        }

        #endregion

    }


}
