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
    /// Static class that contains all common file and folder operation methods.
    /// </summary>
    public static class CommonMethods
    {
        private static readonly object SyncLock = new object();

        #region XML

        /// <summary>
        /// Helper method to assist with the saving of XML metadata.
        /// </summary>
        /// <param name="xmlDoc">XmlDocument object to save from.</param>
        /// <param name="xmlPath">Path to save XML to.</param>
        public static void SaveXML(ref XmlDocument xmlDoc, string xmlPath)
        {
            int count = 0;
            while (true && count < 5)
            {
                try
                {
                    lock (SyncLock)
                    {
                        XmlNode node = xmlDoc.SelectSingleNode(CommonXMLConstants.XPathExpr);
                        if (node != null)
                            node.FirstChild.InnerText = DateTime.Now.Ticks.ToString();
                        xmlDoc.Save(xmlPath);
                    }
                    break;
                }
                catch (XmlException)
                {
                    break;
                }
                catch (IOException)
                {
                    System.Threading.Thread.Sleep(500);
                    count++;
                }
                catch (UnauthorizedAccessException)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Helper method to assist with the loading of XML metadata.
        /// </summary>
        /// <param name="xmlDoc">XmlDocument object to load to.</param>
        /// <param name="xmlPath">Path to XML file to load from.</param>
        public static void LoadXML(ref XmlDocument xmlDoc, string xmlPath)
        {
            int count = 0;
            while (true && count < 5)
            {
                try
                {
                    lock (SyncLock)
                    {
                        xmlDoc.Load(xmlPath);
                    }
                    break;
                }
                catch (FileNotFoundException)
                {
                    break;
                }
                catch (IOException)
                {
                    System.Threading.Thread.Sleep(500);
                    count++;
                }
                catch (XmlException)
                {
                    if (File.Exists(xmlPath))
                    {
                        try
                        {
                            File.Delete(xmlPath);
                        }
                        catch (IOException)
                        {
                            System.Threading.Thread.Sleep(500);
                            count++;
                        }
                        catch (UnauthorizedAccessException)
                        {
                            break;
                        }
                    }
                    string synclessfolder = @"\.syncless\syncless.xml";
                    string parentfolder = xmlPath.Replace(synclessfolder, string.Empty);
                    CreateFileIfNotExist(parentfolder);
                }
                catch (UnauthorizedAccessException)
                {
                    break;
                }
            }
        }

        public static void CreateFileIfNotExist(string path)
        {
            string xmlPath = Path.Combine(path, CommonXMLConstants.MetadataPath);
            
            if (File.Exists(xmlPath))
                return;

            int count = 0;

            while (true && count < 5)
            {
                try
                {
                    DirectoryInfo di = Directory.CreateDirectory(Path.Combine(path, CommonXMLConstants.MetaDir));
                    di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
                    XmlTextWriter writer = new XmlTextWriter(xmlPath, null);
                    writer.Formatting = Formatting.Indented;
                    writer.WriteStartDocument();
                    writer.WriteStartElement(CommonXMLConstants.NodeMetaData);
                    writer.WriteElementString(CommonXMLConstants.NodeLastModified, (DateTime.Now.Ticks).ToString());
                    writer.WriteElementString(CommonXMLConstants.NodeName, GetLastFileIndex(path));
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                    writer.Flush();
                    writer.Close();
                    break;
                }
                catch (IOException)
                {
                    count++;
                    System.Threading.Thread.Sleep(500);
                }
                catch (XmlException)
                {
                    if (File.Exists(xmlPath))
                        File.Delete(xmlPath);
                    count++;
                }
                catch (UnauthorizedAccessException)
                {
                    break;
                }
            }
        }

        private static string GetLastFileIndex(string filePath)
        {
            string[] splitWords = filePath.Split('\\');
            string folderPath = string.Empty;
            for (int i = 0; i < splitWords.Length; i++)
            {
                if (i == splitWords.Length - 1)
                    return splitWords[i];
            }

            return folderPath;
        }

        // Method credited to http://stackoverflow.com/questions/642125/encoding-xpath-expressions-with-both-single-and-double-quotes
        public static string ParseXPathString(string input)
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

        public static void DoFileCleanUp(XmlDocument xmlDoc, string name)
        {
            XmlNode node = xmlDoc.SelectSingleNode(CommonXMLConstants.XPathExpr + CommonXMLConstants.XPathFile + "[name=" + CommonMethods.ParseXPathString(name) + "]");

            if (node == null)
                return;

            node.ParentNode.RemoveChild(node);
        }

        public static void DoFolderCleanUp(XmlDocument xmlDoc, string name)
        {
            XmlNode node = xmlDoc.SelectSingleNode(CommonXMLConstants.XPathExpr + CommonXMLConstants.XPathFolder + "[name=" + CommonMethods.ParseXPathString(name) + "]");

            if (node == null)
                return;

            node.ParentNode.RemoveChild(node);
        }

        public static void CreateLastKnownStateFile(string path)
        {
            string lastKnownXML = Path.Combine(path, CommonXMLConstants.LastKnownStatePath);
            if (File.Exists(lastKnownXML))
                return;

            while (true)
            {
                try
                {
                    DirectoryInfo di = Directory.CreateDirectory(Path.Combine(path, CommonXMLConstants.MetaDir));
                    di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
                    XmlTextWriter writer = new XmlTextWriter(lastKnownXML, null);
                    writer.Formatting = Formatting.Indented;
                    writer.WriteStartDocument();
                    writer.WriteStartElement(CommonXMLConstants.NodeLastKnownState);
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                    writer.Flush();
                    writer.Close();
                    break;
                }
                catch (IOException)
                {
                    break;
                }
                catch (UnauthorizedAccessException)
                {
                    break;
                }
                catch (XmlException)
                {
                    if (File.Exists(lastKnownXML))
                        File.Delete(lastKnownXML);
                }
            }
        }

        #endregion

        #region File Operations

        /// <summary>
        /// Archives a file.
        /// </summary>
        /// <param name="path">The fullpath of the file to archive.</param>
        /// <param name="archiveName">The name of the folder to archive to.</param>
        /// <param name="archiveLimit">The number of old copies to keep for the file.</param>
        /// <exception cref="ArchiveFileException"></exception>
        public static void ArchiveFile(string path, string archiveName, int archiveLimit)
        {
            Debug.Assert(path != null && archiveName != null && archiveLimit >= 0);

            try
            {
                FileInfo f = new FileInfo(path);

                if (!f.Exists)
                    return;

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
            catch (UnauthorizedAccessException e)
            {
                throw new ArchiveFileException(e);
            }
        }

        /// <summary>
        /// Calculates the MD5 hash of a given file.
        /// </summary>
        /// <param name="fileInput">Fullpath of the file to hash.</param>
        /// <returns>MD5 hash of file.</returns>
        /// <exception cref="Syncless.CompareAndSync.Exceptions.HashFileException"></exception>
        public static string CalculateMD5Hash(string fileInput)
        {
            if (!File.Exists(fileInput))
                throw new HashFileException(new FileNotFoundException());

            try
            {
                FileStream fileStream = File.OpenRead(fileInput);
                return CalculateMD5Hash(fileStream);
            }
            catch (DirectoryNotFoundException e)
            {
                throw new HashFileException(e);
            }
            catch (IOException e)
            {
                throw new HashFileException(e);
            }
            catch (UnauthorizedAccessException e)
            {
                throw new HashFileException(e);
            }
        }

        /// <summary>
        /// Calculates the MD5 hash of a given FileInfo.
        /// </summary>
        /// <param name="fileInput">FileInfo object to hash.</param>
        /// <returns>MD5 hash of file.</returns>
        /// <exception cref="Syncless.CompareAndSync.Exceptions.HashFileException"></exception>
        public static string CalculateMD5Hash(FileInfo fileInput)
        {
            if (!fileInput.Exists)
                throw new HashFileException(new FileNotFoundException());

            try
            {
                FileStream fileStream = fileInput.OpenRead();
                return CalculateMD5Hash(fileStream);
            }
            catch (DirectoryNotFoundException e)
            {
                throw new HashFileException(e);
            }
            catch (IOException e)
            {
                throw new HashFileException(e);
            }
            catch (UnauthorizedAccessException e)
            {
                throw new HashFileException(e);
            }
        }

        /// <summary>
        /// Helper method that does the actual MD5 hashing.
        /// </summary>
        /// <param name="fileStream">FileStream to hash.</param>
        /// <returns>MD5 hash in string format.</returns>
        private static string CalculateMD5Hash(FileStream fileStream)
        {
            byte[] fileHash = MD5.Create().ComputeHash(fileStream);
            fileStream.Close();
            return BitConverter.ToString(fileHash).Replace("-", "");
        }

        /// <summary>
        /// Copys a file from one place to another, overwriting the destination if it exists.
        /// </summary>
        /// <param name="sourceFile">Fullpath of file to copy.</param>
        /// <param name="destFile">Fullpath of destination file</param>
        /// <exception cref="CopyFileException"></exception>
        public static void CopyFile(string sourceFile, string destFile)
        {
            try
            {
                File.Copy(sourceFile, destFile, true);
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
            catch (UnauthorizedAccessException e)
            {
                throw new CopyFileException(e);
            }
        }

        /// <summary>
        /// Deletes a file.
        /// </summary>
        /// <param name="path">Fullpath of file to delete.</param>
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
            catch (UnauthorizedAccessException e)
            {
                throw new DeleteFileException(e);
            }
        }

        /// <summary>
        /// Deletes a file to recycle bin.
        /// </summary>
        /// <param name="path">Fullpath of file to delete to recycle bin.</param>
        /// <exception cref="DeleteFileException"></exception>
        public static void DeleteFileToRecycleBin(string path)
        {
            try
            {
                Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(path, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
            }
            catch (FileNotFoundException e)
            {
                throw new DeleteFileException(e);
            }
            catch (IOException e)
            {
                throw new DeleteFileException(e);
            }
            catch (UnauthorizedAccessException e)
            {
                throw new DeleteFileException(e);
            }
        }

        /// <summary>
        /// Moves a file from one place to another. This method is also used for file renaming.
        /// </summary>
        /// <param name="sourceFile">Fullpath of file to move.</param>
        /// <param name="destFile">Fullpath of destination to move to.</param>
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
            catch (UnauthorizedAccessException e)
            {
                throw new MoveFileException(e);
            }
        }

        #endregion

        #region Folder Operations

        /// <summary>
        /// Archives a folder given the full path of the folder.
        /// </summary>
        /// <param name="path">Full path of the folder.</param>
        /// <param name="archiveName">Name of folder to archive to.</param>
        /// <param name="archiveLimit">Number of folders to limit.</param>
        /// <exception cref="ArchiveFolderException"></exception>
        public static void ArchiveFolder(string path, string archiveName, int archiveLimit)
        {
            Debug.Assert(path != null && archiveName != null && archiveLimit >= 0);

            try
            {
                DirectoryInfo f = new DirectoryInfo(path);

                if (!f.Exists)
                    return;

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
            catch (UnauthorizedAccessException e)
            {
                throw new ArchiveFolderException(e);
            }

        }

        /// <summary>
        /// Copy folder from source to destination.
        /// </summary>
        /// <param name="source">Fullpath of source folder.</param>
        /// <param name="destination">Fullpath of destination folder.</param>
        /// <exception cref="CopyFolderException"></exception>
        public static void CopyDirectory(string source, string destination)
        {
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
            catch (UnauthorizedAccessException e)
            {
                throw new CopyFolderException(e);
            }
        }

        /// <summary>
        /// Creates a folder given the fullpath.
        /// </summary>
        /// <param name="path">The path to the folder to create.</param>
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
            catch (UnauthorizedAccessException e)
            {
                throw new CreateFolderException(e);
            }
        }

        /// <summary>
        /// Deletes a given folder and all the contents inside it.
        /// </summary>
        /// <param name="path">The path of the folder to delete.</param>
        public static void DeleteFolder(string path)
        {
            try
            {
                Directory.Delete(path, true);
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
            catch (UnauthorizedAccessException e)
            {
                throw new DeleteFolderException(e);
            }
        }

        /// <summary>
        /// Deletes the folder and all contents inside it into the recycle bin.
        /// </summary>
        /// <param name="path">The path of the folder to delete.</param>
        public static void DeleteFolderToRecycleBin(string path)
        {
            try
            {
                Microsoft.VisualBasic.FileIO.FileSystem.DeleteDirectory(path, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
            }
            catch (DirectoryNotFoundException e)
            {
                throw new DeleteFolderException(e);
            }
            catch (IOException e)
            {
                throw new DeleteFolderException(e);
            }
            catch (UnauthorizedAccessException e)
            {
                throw new DeleteFolderException(e);
            }
        }

        /// <summary>
        /// Moves a folder given the source and destination. This method is also used for folder renaming.
        /// </summary>
        /// <param name="sourceFullPath">Fullpath of source to move.</param>
        /// <param name="destFullPath">Fullpath of destination to move to.</param>
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
            catch (UnauthorizedAccessException e)
            {
                throw new MoveFolderException(e);
            }
        }

        #endregion

    }
}
