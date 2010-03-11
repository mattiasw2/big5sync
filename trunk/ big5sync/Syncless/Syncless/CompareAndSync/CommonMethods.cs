using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;
using Syncless.Core;

namespace Syncless.CompareAndSync
{
    public static class CommonMethods
    {

        #region File Operations

        public static void ArchiveFile(string path, string archiveName, int archiveLimit)
        {
            FileInfo f = new FileInfo(path);
            string parent = f.DirectoryName;
            string archiveDir = Path.Combine(parent, archiveName);
            if (!Directory.Exists(archiveDir))
                Directory.CreateDirectory(archiveDir);
            string currTime = String.Format("{0:yyyyMMddHHmmss}", DateTime.Now) + "_";

            File.Copy(path, Path.Combine(archiveDir, currTime + f.Name), true); //Very rare to have same time

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

        public static string CalculateMD5Hash(FileInfo fileInput)
        {
            if (!fileInput.Exists)
                throw new FileNotFoundException(fileInput.Name + ": Unable to hash non-existent file.");

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
                return string.Empty;
            }
            catch (DirectoryNotFoundException e)
            {
                return string.Empty;
            }
            catch (IOException e)
            {
                return string.Empty;
            }
        }

        public static void CopyFile(string sourceFile, string destFile, bool overwrite)
        {
            File.Copy(sourceFile, destFile, overwrite);
        }

        public static void DeleteFile(string path)
        {
            File.Delete(path);
        }

        public static void DeleteFileToRecycleBin(string path)
        {
            Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(path, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
        }

        public static void MoveFile(string sourceFile, string destFile)
        {
            File.Move(sourceFile, destFile);
        }

        #endregion

        #region Folder Operations

        public static void ArchiveFolder(string path, string archiveName, int archiveLimit)
        {
            FileInfo f = new FileInfo(path);
            string parent = f.DirectoryName;
            string archiveDir = Path.Combine(parent, archiveName);
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

            var sorted = (from element in archivedFiles orderby element descending select element).Skip(archiveLimit);

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
                fileInfo.CopyTo(destination + "\\" + fileInfo.Name, true);

            }
            return destinationInfo.FullName;
        }

        public static void CreateFolder(string path)
        {
            Directory.CreateDirectory(path);
        }

        public static void DeleteFolder(string path, bool recursive)
        {
            Directory.Delete(path, recursive);
        }

        public static void DeleteFolderToRecycleBin(string path)
        {
            Microsoft.VisualBasic.FileIO.FileSystem.DeleteDirectory(path, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
        }

        #endregion

    }
}
