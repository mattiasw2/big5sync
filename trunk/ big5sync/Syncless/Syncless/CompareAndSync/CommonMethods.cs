using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Syncless.CompareAndSync.Exceptions;

namespace Syncless.CompareAndSync
{
    /// <summary>
    /// Static class that contains all common filer and folder operation methods.
    /// </summary>
    public static class CommonMethods
    {
        private static readonly object syncLock = new object();

        #region XML

        public static void SaveXML(ref XmlDocument xmlDoc, string xmlPath)
        {
            while (true)
            {
                try
                {
                    lock (syncLock)
                    {
                        XmlNode node = xmlDoc.SelectSingleNode("/meta-data");
                        if (node != null)
                            node.FirstChild.InnerText = DateTime.Now.Ticks.ToString();
                        xmlDoc.Save(xmlPath);
                    }
                    break;
                }
                catch (IOException)
                {
                    System.Threading.Thread.Sleep(250);
                }
            }
        }

        public static void LoadXML(ref XmlDocument xmlDoc, string xmlPath)
        {
            while (true)
            {
                try
                {
                    xmlDoc.Load(xmlPath);
                    break;
                }
                catch (FileNotFoundException)
                {
                    throw new FileNotFoundException("Error :(");
                }
                catch (IOException)
                {
                    System.Threading.Thread.Sleep(250);
                }
            }
        }

        #endregion

        #region File Operations

        /// <summary>
        /// Archives a file
        /// </summary>
        /// <param name="path">The fullpath of the file to archive</param>
        /// <param name="archiveName">The name of the folder to archive to</param>
        /// <param name="archiveLimit">The number of old copies to keep for the file</param>
        /// <exception cref="ArchiveFileException"></exception>
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

        /// <summary>
        /// Calculates the MD5 hash of a given FileInfo
        /// </summary>
        /// <param name="fileInput">FileInfo object to hash</param>
        /// <returns>MD5 hash of file</returns>
        /// <exception cref="Syncless.CompareAndSync.Exceptions.HashFileException"></exception>
        public static string CalculateMD5Hash(FileInfo fileInput)
        {
            if (!fileInput.Exists)
                throw new HashFileException(new FileNotFoundException());

            try
            {
                FileStream fileStream = fileInput.OpenRead();
                byte[] fileHash = MD5.Create().ComputeHash(fileStream);
                fileStream.Close();
                return BitConverter.ToString(fileHash).Replace("-", "");
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

        /// <summary>
        /// Copys a file from one place to another
        /// </summary>
        /// <param name="sourceFile">Fullpath of file to copy</param>
        /// <param name="destFile">Fullpath of destination file</param>
        /// <param name="overwrite">Set to true to overwrite a file if it exists</param>
        /// <exception cref="CopyFileException"></exception>
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

        /// <summary>
        /// Deletes a file
        /// </summary>
        /// <param name="path">Fullpath of file to delete</param>
        /// <exception cref="DeleteFileException"></exception>
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

        /// <summary>
        /// Deletes a file to recyclebin
        /// </summary>
        /// <param name="path">Fullpath of file to delete to recycle bin</param>
        /// <exception cref="DeleteFileException"></exception>
        public static void DeleteFileToRecycleBin(string path)
        {
            try
            {
                Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(path,
                                                                   Microsoft.VisualBasic.FileIO.UIOption.
                                                                       OnlyErrorDialogs,
                                                                   Microsoft.VisualBasic.FileIO.RecycleOption.
                                                                       SendToRecycleBin);
            }
            catch (FileNotFoundException e)
            {
                throw new DeleteFileException(e);
            }
            catch (IOException e)
            {
                throw new DeleteFileException(e);
            }
        }

        /// <summary>
        /// Moves a file from one place to another
        /// </summary>
        /// <param name="sourceFile">Fullpath of file to move</param>
        /// <param name="destFile">Fullpath of destination to move to</param>
        /// <exception cref="MoveFileException"></exception>
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

        /// <summary>
        /// Archives a folder given the full path of the folder
        /// </summary>
        /// <param name="path">Full path of the folder</param>
        /// <param name="archiveName">Name of folder to archive to</param>
        /// <param name="archiveLimit">Number of folders to limit</param>
        /// <exception cref="ArchiveFolderException"></exception>
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

        /// <summary>
        /// Copy folder from source to destination
        /// </summary>
        /// <param name="source">Fullpath of source folder</param>
        /// <param name="destination">Fullpath of destination folder</param>
        /// <exception cref="CopyFolderException"></exception>
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


        // Method credited to http://stackoverflow.com/questions/642125/encoding-xpath-expressions-with-both-single-and-double-quotes
        public static string ParseXpathString(string input)
        {
            // If we don't have any " then encase string in " 
            if (!input.Contains("\""))
                return String.Format("\"{0}\"", input);

            // If we have some " but no ' then encase in ' 
            if (!input.Contains("'"))
                return String.Format("'{0}'", input);

            // If we get here we have both " and ' in the string so must use Concat 
            StringBuilder sb = new StringBuilder("concat(");

            // Going to look for " as they are LESS likely than ' in our data so will minimise 
            // number of arguments to concat. 
            int lastPos = 0;
            int nextPos = input.IndexOf("\"");
            while (nextPos != -1)
            {
                // If this is not the first time through the loop then seperate arguments with , 
                if (lastPos != 0)
                    sb.Append(",");

                sb.AppendFormat("\"{0}\",'\"'", input.Substring(lastPos, nextPos - lastPos));
                lastPos = ++nextPos;

                // Find next occurance 
                nextPos = input.IndexOf("\"", lastPos);
            }

            sb.Append(")");
            return sb.ToString();
        }


    }
}
