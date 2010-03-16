using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;
using Syncless.Core;
using Syncless.CompareAndSync.Exceptions;

namespace Syncless.CompareAndSync
{
    /// <summary>
    /// Static class that contains all common filer and folder operation methods.
    /// </summary>
    public static class CommonMethods
    {

        #region File Operations

        public static void ArchiveFile(string path, string archiveName, int archiveLimit)
        {
            Debug.Assert(path != null && archiveName != null && archiveLimit >= 0);

            FileInfo f = new FileInfo(path);

            if (!f.Exists)
                return;

            try
            {
                string parent = f.DirectoryName;
                string archiveDir = Path.Combine(parent, archiveName);
                if (!Directory.Exists(archiveDir))
                    Directory.CreateDirectory(archiveDir);
                string currTime = String.Format("{0:yyyyMMddHHmmss}", DateTime.Now) + "_";
                File.Copy(path, Path.Combine(archiveDir, currTime + f.Name), true); //Very rare to have same time

                if (archiveLimit > 0)
                {
                    DirectoryInfo d = new DirectoryInfo(archiveDir);
                    FileInfo[] files = d.GetFiles();
                    List<string> archivedFiles = new List<string>();

                    for (int i = 0; i < files.Length; i++)
                    {
                        if (files[i].Name.EndsWith(f.Name))
                        {
                            archivedFiles.Add(files[i].Name);
                        }
                    }

                    var sorted = (from element in archivedFiles orderby element descending select element).Skip(archiveLimit);

                    foreach (string s in sorted)
                    {
                        File.Delete(Path.Combine(archiveDir, s));
                    }
                }
            }
            catch (PathTooLongException e)
            {
                throw new ArchiveFileException(e);
            }
            catch (IOException e)
            {
                throw new ArchiveFileException(e);
            }

        }

        public static string CalculateMD5Hash(FileInfo fileInput)
        {
            if (!fileInput.Exists)
                throw new HashFileException(new FileNotFoundException());

            try
            {
                FileStream fileStream = fileInput.OpenRead();
                byte[] fileHash = MD5.Create().ComputeHash(fileStream);
                fileStream.Close();
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < fileHash.Length; i++)
                    sb.Append(fileHash[i].ToString("X2"));

                return sb.ToString();
            }
            catch (UnauthorizedAccessException e)
            {
                throw new HashFileException(e);
            }
            catch (DirectoryNotFoundException e)
            {
                throw new HashFileException(e);
            }
            catch (IOException e)
            {
                throw new HashFileException(e);
            }
        }

        public static void CopyFile(string sourceFile, string destFile, bool overwrite)
        {
            Debug.Assert(File.Exists(sourceFile));

            try
            {
                File.Copy(sourceFile, destFile, overwrite);
            }
            catch (PathTooLongException e)
            {
                throw new CopyFileException(e);
            }
            catch (DirectoryNotFoundException e)
            {
                throw new CopyFileException(e);
            }
            catch (IOException e)
            {
                throw new CopyFileException(e);
            }
        }

        public static void DeleteFile(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch (PathTooLongException e)
            {
                throw new DeleteFileException(e);
            }
            catch (DirectoryNotFoundException e)
            {
                throw new DeleteFileException(e);
            }
            catch (IOException e)
            {
                throw new DeleteFileException(e);
            }
        }

        //TODO: Handle exceptions?
        public static void DeleteFileToRecycleBin(string path)
        {
            Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(path, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
        }

        public static void MoveFile(string sourceFile, string destFile)
        {
            try
            {
                File.Move(sourceFile, destFile);
            }
            catch (PathTooLongException e)
            {
                throw new MoveFileException(e);
            }
            catch (DirectoryNotFoundException e)
            {
                throw new MoveFileException(e);
            }
            catch (IOException e)
            {
                throw new MoveFileException(e);
            }
        }

        #endregion

        #region Folder Operations

        public static void ArchiveFolder(string path, string archiveName, int archiveLimit)
        {
            Debug.Assert(path != null && archiveName != null && archiveLimit >= 0);

            DirectoryInfo f = new DirectoryInfo(path);

            if (!f.Exists)
                return;

            try
            {
                string parent = f.Parent.FullName;
                string archiveDir = Path.Combine(parent, archiveName);
                if (!Directory.Exists(archiveDir))
                    Directory.CreateDirectory(archiveDir);
                string currTime = String.Format("{0:yyyyMMddHHmmss}", DateTime.Now) + "_";

                CopyDirectory(path, Path.Combine(archiveDir, currTime + f.Name));

                if (archiveLimit > 0)
                {
                    DirectoryInfo d = new DirectoryInfo(archiveDir);
                    DirectoryInfo[] folders = d.GetDirectories();
                    List<string> archivedFiles = new List<string>();

                    for (int i = 0; i < folders.Length; i++)
                    {
                        if (folders[i].Name.EndsWith(f.Name))
                            archivedFiles.Add(folders[i].Name);
                    }

                    var sorted = (from element in archivedFiles orderby element descending select element).Skip(archiveLimit);

                    foreach (string s in sorted)
                        Directory.Delete(Path.Combine(archiveDir, s), true);
                }
            }
            catch (CopyFolderException e)
            {
                throw new ArchiveFolderException(e);
            }
            catch (PathTooLongException e)
            {
                throw new ArchiveFolderException(e);
            }
            catch (DirectoryNotFoundException e)
            {
                throw new ArchiveFolderException(e);
            }
            catch (IOException e)
            {
                throw new ArchiveFolderException(e);
            }

        }

        public static void CopyDirectory(string source, string destination)
        {
            Debug.Assert(Directory.Exists(source));

            try
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
                    fileInfo.CopyTo(Path.Combine(destination, fileInfo.Name), true);
                }
            }
            catch (PathTooLongException e)
            {
                throw new CopyFolderException(e);
            }
            catch (DirectoryNotFoundException e)
            {
                throw new CopyFolderException(e);
            }
            catch (IOException e)
            {
                throw new CopyFolderException(e);
            }
        }

        public static void CreateFolder(string path)
        {
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (PathTooLongException e)
            {
                throw new CreateFolderException(e);
            }
            catch (DirectoryNotFoundException e)
            {
                throw new CreateFolderException(e);
            }
            catch (IOException e)
            {
                throw new CreateFolderException(e);
            }
        }

        public static void DeleteFolder(string path, bool recursive)
        {
            try
            {
                Directory.Delete(path, recursive);
            }
            catch (PathTooLongException e)
            {
                throw new DeleteFolderException(e);
            }
            catch (DirectoryNotFoundException e)
            {
                throw new DeleteFolderException(e);
            }
            catch (IOException e)
            {
                throw new DeleteFolderException(e);
            }
        }

        //TODO: Handle exceptions?
        public static void DeleteFolderToRecycleBin(string path)
        {
            Microsoft.VisualBasic.FileIO.FileSystem.DeleteDirectory(path, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
        }

        public static void MoveFolder(string sourceFullPath, string destFullPath)
        {
            try
            {
                Directory.Move(sourceFullPath, destFullPath);
            }
            catch (PathTooLongException e)
            {
                throw new MoveFolderException(e);
            }
            catch (DirectoryNotFoundException e)
            {
                throw new MoveFolderException(e);
            }
            catch (IOException e)
            {
                throw new MoveFolderException(e);
            }
        }

        #endregion

        /// <summary>
        /// Checks if all the give file system objects are the same type, that is, file or folder.
        /// </summary>
        /// <param name="paths"></param>
        private static void CheckIfSameType(List<string> paths)
        {
            bool isFile = System.IO.File.Exists(paths[0]);

            for (int i = 1; i < paths.Count; i++)
            {
                if (isFile != System.IO.File.Exists(paths[i]))
                    throw new IncompatibleTypeException();

            }
        }
    }
}
