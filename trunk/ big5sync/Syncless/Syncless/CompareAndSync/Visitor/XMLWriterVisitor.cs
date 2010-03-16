using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.CompareAndSync.CompareObject;
using System.Xml;
using System.IO;
using Syncless.CompareAndSync.Enum;

namespace Syncless.CompareAndSync.Visitor
{
    public class XMLWriterVisitor : IVisitor
    {
        #region IVisitor Members

        private const string META_DIR = ".syncless";
        private const string XML_NAME = "syncless.xml";
        private const string METADATAPATH = META_DIR + "\\" + XML_NAME;
        private const string FOLDER = "folder";
        private const string FILES = "files";
        private const string NAME_OF_FOLDER = "name_of_folder";
        private const string NODE_NAME = "name";
        private const string NODE_SIZE = "size";
        private const string NODE_HASH = "hash";
        private const string NODE_LAST_MODIFIED = "last_modified";
        private const string NODE_LAST_CREATED = "last_created";
        private const string XPATH_EXPR = "/meta-data";
        private const string LAST_MODIFIED = "/last_modified";
        private const int FIRST_POSITION = 0;
        private static readonly object syncLock = new object();
        private string[] pathList;
        private List<string> newNameList = new List<string>();
        private List<string> oldNameList = new List<string>();

        public void Visit(FileCompareObject file, string[] currentPath)
        {
            for (int i = 0; i < currentPath.Length; i++) // HANDLE ALL EXCEPT PROPAGATED
            {
                if (currentPath[i].Contains(META_DIR))
                    continue;
                ProcessMetaChangeType(currentPath[i], file, i);
            }
        }

        public void Visit(FolderCompareObject folder, string[] currentPath)
        {
            for (int i = 0; i < currentPath.Length; i++)
            {
                if (currentPath[i].Contains(META_DIR))
                    continue;
                ProcessFolderFinalState(currentPath[i], folder, i);
            }
        }

        public void Visit(RootCompareObject root)
        {
            pathList = root.Paths;
        }

        private void CreateFileIfNotExist(string path)
        {
            string xmlPath = Path.Combine(path, METADATAPATH);
            if (File.Exists(xmlPath))
                return;

            lock (syncLock)
            {
                DirectoryInfo di = Directory.CreateDirectory(Path.Combine(path, META_DIR));
                di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
                XmlTextWriter writer = new XmlTextWriter(xmlPath, null);
                writer.Formatting = Formatting.Indented;
                writer.WriteStartDocument();
                writer.WriteStartElement("meta-data");
                writer.WriteElementString("last_modified", (DateTime.Now.Ticks).ToString());
                writer.WriteElementString(NODE_NAME, GetLastFileIndex(path));
                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Flush();
                writer.Close();
            }
        }

        private void ProcessMetaChangeType(string currentPath, FileCompareObject file, int counter)
        {

            FinalState? changeType = file.FinalState[counter];

            if (changeType == null)
            {
                HandleNullCases(currentPath, file);
                return;
            }

            switch (changeType)
            {
                case FinalState.Created:
                    CreateFileObject(file, counter, currentPath);
                    break;
                case FinalState.Updated:
                    UpdateFileObject(file, counter, currentPath);
                    break;
                case FinalState.Deleted:
                    DeleteFileObject(file, currentPath);
                    break;
                case FinalState.Renamed:
                    RenameFileObject(file, counter, currentPath);
                    break;
                case FinalState.Unchanged:
                    HandleUnchangedOrPropagatedFile(file, counter, currentPath);
                    break;
                case FinalState.Propagated:
                    HandleUnchangedOrPropagatedFile(file, counter, currentPath);
                    break;
            }


        }

        private void UpdateLastModifiedTime(XmlDocument xmlDoc)
        {
            XmlNode node = xmlDoc.SelectSingleNode(XPATH_EXPR + LAST_MODIFIED);
            node.InnerText = DateTime.Now.Ticks.ToString();
        }

        private string getFolderString(string filePath)
        {
            string[] splitWords = filePath.Split('\\');
            string folderPath = "";
            for (int i = 0; i < splitWords.Length; i++)
            {
                if (i == splitWords.Length - 1)
                    break;
                if (folderPath.Equals(""))
                    folderPath = splitWords[i];
                else
                    folderPath = folderPath + "\\" + splitWords[i];
            }

            return folderPath;
        }

        private string GetLastFileIndex(string filePath)
        {
            string[] splitWords = filePath.Split('\\');
            string folderPath = "";
            for (int i = 0; i < splitWords.Length; i++)
            {
                if (i == splitWords.Length - 1)
                    return splitWords[i];
            }

            return folderPath;
        }

        private void CreateFileObject(FileCompareObject file, int counter, string currentPath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            currentPath = UpdateToRenamedFolder(file.Parent.Name, currentPath);
            currentPath = RemoveOldFolderName(currentPath);

            string xmlPath = Path.Combine(currentPath, METADATAPATH);
            CreateFileIfNotExist(currentPath);

            lock (syncLock)
            {
                CommonMethods.LoadXML(ref xmlDoc, xmlPath);
            }

            int position = GetPropagated(file);
            DoFileCleanUp(xmlDoc, file.Name);
            XmlText hashText = xmlDoc.CreateTextNode(file.Hash[position]);
            XmlText nameText = xmlDoc.CreateTextNode(file.Name);
            XmlText sizeText = xmlDoc.CreateTextNode(file.Length[position].ToString());
            XmlText lastModifiedText = xmlDoc.CreateTextNode(file.LastWriteTime[counter].ToString());
            XmlText lastCreatedText = xmlDoc.CreateTextNode(file.CreationTime[counter].ToString());

            XmlElement fileElement = xmlDoc.CreateElement(FILES);
            XmlElement hashElement = xmlDoc.CreateElement(NODE_HASH);
            XmlElement nameElement = xmlDoc.CreateElement(NODE_NAME);
            XmlElement sizeElement = xmlDoc.CreateElement(NODE_SIZE);
            XmlElement lastModifiedElement = xmlDoc.CreateElement(NODE_LAST_MODIFIED);
            XmlElement lastCreatedElement = xmlDoc.CreateElement(NODE_LAST_CREATED);

            hashElement.AppendChild(hashText);
            nameElement.AppendChild(nameText);
            sizeElement.AppendChild(sizeText);
            lastModifiedElement.AppendChild(lastModifiedText);
            lastCreatedElement.AppendChild(lastCreatedText);

            fileElement.AppendChild(nameElement);
            fileElement.AppendChild(sizeElement);
            fileElement.AppendChild(hashElement);
            fileElement.AppendChild(lastModifiedElement);
            fileElement.AppendChild(lastCreatedElement);

            XmlNode node = xmlDoc.SelectSingleNode(XPATH_EXPR);
            node.AppendChild(fileElement);

            lock (syncLock)
            {
                CommonMethods.SaveXML(ref xmlDoc, xmlPath);
            }
        }

        private void UpdateFileObject(FileCompareObject file, int counter, string currentPath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            currentPath = UpdateToRenamedFolder(file.Parent.Name, currentPath);
            currentPath = RemoveOldFolderName(currentPath);

            string xmlPath = Path.Combine(currentPath, METADATAPATH);
            CreateFileIfNotExist(currentPath);

            lock (syncLock)
            {
                CommonMethods.LoadXML(ref xmlDoc, xmlPath);
            }

            int position = GetPropagated(file);
            XmlNode node = xmlDoc.SelectSingleNode(XPATH_EXPR + "/files" + "[name='" + file.Name + "']");
            if (node == null)
            {
                CreateFileObject(file, counter, currentPath);
                return;
            }

            XmlNodeList childNodeList = node.ChildNodes;
            for (int i = 0; i < childNodeList.Count; i++)
            {
                XmlNode nodes = childNodeList[i];
                if (nodes.Name.Equals(NODE_SIZE))
                {
                    nodes.InnerText = file.Length[position].ToString();
                }
                else if (nodes.Name.Equals(NODE_HASH))
                {
                    nodes.InnerText = file.Hash[position];
                }
                else if (nodes.Name.Equals(NODE_NAME))
                {
                    nodes.InnerText = file.Name;
                }
                else if (nodes.Name.Equals(NODE_LAST_MODIFIED))
                {
                    nodes.InnerText = file.LastWriteTime[counter].ToString();
                }
                else if (nodes.Name.Equals(NODE_LAST_CREATED))
                {
                    nodes.InnerText = file.CreationTime[counter].ToString();
                }
            }

            lock (syncLock)
            {
                CommonMethods.SaveXML(ref xmlDoc, xmlPath);
            }
        }

        private void RenameFileObject(FileCompareObject file, int counter, string currentPath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            currentPath = UpdateToRenamedFolder(file.Parent.Name, currentPath);
            currentPath = RemoveOldFolderName(currentPath);

            string xmlPath = Path.Combine(currentPath, METADATAPATH);
            CreateFileIfNotExist(currentPath);
            lock (syncLock)
            {
                CommonMethods.LoadXML(ref xmlDoc, xmlPath);
            }

            XmlNode node = xmlDoc.SelectSingleNode(XPATH_EXPR + "/files" + "[name='" + file.Name + "']");
            if (node == null)
            {
                CreateFileObject(file, counter, currentPath);
                return;
            }
            node.FirstChild.InnerText = file.NewName;

            lock (syncLock)
            {
                CommonMethods.SaveXML(ref xmlDoc, xmlPath);
            }
        }

        private void DeleteFileObject(FileCompareObject file, string currentPath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            currentPath = UpdateToRenamedFolder(file.Parent.Name, currentPath);
            currentPath = RemoveOldFolderName(currentPath);

            string xmlPath = Path.Combine(currentPath, METADATAPATH);
            if (File.Exists(xmlPath))
            {
                lock (syncLock)
                {
                    CommonMethods.LoadXML(ref xmlDoc, xmlPath);
                }

                XmlNode node = xmlDoc.SelectSingleNode(XPATH_EXPR + "/files" + "[name='" + file.Name + "']");
                if (node == null)
                    return;
                node.ParentNode.RemoveChild(node);

                lock (syncLock)
                {
                    CommonMethods.SaveXML(ref xmlDoc, xmlPath);
                }
            }
        }

        private string UpdateToRenamedFolder(string parentFolderName, string currentPath)
        {
            int flag = CheckForRename(parentFolderName);
            if (flag != -1)
            {
                return Swap(currentPath, newNameList[flag], parentFolderName);
            }
            else
            {
                return currentPath;
            }

        }

        private string RemoveOldFolderName(string currentPath)
        {
            string[] splitUpPaths = currentPath.Split('\\');
            string parentFolderName = splitUpPaths[splitUpPaths.Length - 1];
            int flag = CheckForRename(parentFolderName);
            if (flag == -1)
            {
                return currentPath;
            }
            else
            {
                currentPath = currentPath.Replace(parentFolderName, newNameList[flag]);
                return currentPath;
            }
        }

        private int CheckForRename(string name)
        {
            for (int i = 0; i < oldNameList.Count; i++)
            {
                if (oldNameList[i].Equals(name))
                    return i;
            }
            return -1;
        }

        private string Swap(string name, string newName, string oldName)
        {
            return name.Replace(oldName, newName);
        }

        private int GetPropagated(FileCompareObject file)
        {
            FinalState?[] states = file.FinalState;
            for (int i = 0; i < states.Length; i++)
            {
                if (FinalState.Propagated == states[i])
                    return i;
            }

            return -1; // never happen
        }

        private int GetPropagated(FolderCompareObject folder)
        {
            FinalState?[] states = folder.FinalState;
            for (int i = 0; i < states.Length; i++)
            {
                if (FinalState.Propagated == states[i])
                    return i;
            }

            return -1; // never happen
        }


        private void DoFileCleanUp(XmlDocument xmlDoc, string name)
        {
            XmlNode node = xmlDoc.SelectSingleNode(XPATH_EXPR + "/files" + "[name='" + name + "']");
            if (node == null)
                return;
            node.ParentNode.RemoveChild(node);
        }

        private void DoFolderCleanUp(XmlDocument xmlDoc, string name)
        {
            XmlNode node = xmlDoc.SelectSingleNode(XPATH_EXPR + "/folder" + "[name='" + name + "']");
            if (node == null)
                return;
            node.ParentNode.RemoveChild(node);
        }

        private void HandleUnchangedOrPropagatedFile(FileCompareObject file, int position, string filePath)
        {
            filePath = UpdateToRenamedFolder(file.Parent.Name, filePath);
            filePath = RemoveOldFolderName(filePath);

            string name = Path.Combine(filePath, file.Name);
            if (File.Exists(name)) //CREATE OR UPDATED
            {
                bool metaExist = file.MetaExists[position];
                bool fileExist = file.Exists[position];
                if (metaExist == true && fileExist == true) //UPDATE
                {
                    UpdateFileObject(file, position, filePath);
                }
                else  //NEW
                {
                    CreateFileObject(file, position, filePath);
                }
            }
            else                 //DELETE OR RENAME
            {
                if (file.NewName != null)
                    RenameFileObject(file, position, filePath);
                else
                    DeleteFileObject(file, filePath);
            }
        }

        private void HandleNullCases(string currentPath, FileCompareObject file)
        {

            string fullPath = Path.Combine(currentPath, file.Name);
            if (File.Exists(fullPath))
                return;

            XmlDocument xmlDoc = new XmlDocument();
            string xmlPath = Path.Combine(currentPath, METADATAPATH);

            lock (syncLock)
            {
                CommonMethods.LoadXML(ref xmlDoc, xmlPath);
            }

            XmlNode node = xmlDoc.SelectSingleNode(XPATH_EXPR + "/files" + "[name='" + file.Name + "']");
            if (node != null)
                node.ParentNode.RemoveChild(node);

            lock (syncLock)
            {
                CommonMethods.SaveXML(ref xmlDoc, xmlPath);
            }
        }

        private void ProcessFolderFinalState(string currentPath, FolderCompareObject folder, int counter)
        {

            FinalState?[] finalStateList = folder.FinalState;
            FinalState? changeType = finalStateList[counter];
            switch (changeType)
            {
                case FinalState.Created:
                    CreateFolderObject(folder, currentPath);
                    break;
                case FinalState.Deleted:
                    DeleteFolderObject(folder, currentPath);
                    break;
                case FinalState.Renamed:
                    RenameFolderObject(folder, currentPath);
                    break;
                case FinalState.Propagated:
                    HandleUnchangedOrPropagatedFolder(folder, counter, currentPath);
                    break;
            }
        }


        private void CreateFolderObject(FolderCompareObject folder, string currentPath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            string xmlPath = Path.Combine(currentPath, METADATAPATH);
            CreateFileIfNotExist(currentPath);

            lock (syncLock)
            {
                CommonMethods.LoadXML(ref xmlDoc, xmlPath);
            }

            DoFolderCleanUp(xmlDoc, folder.Name);
            XmlText nameText = xmlDoc.CreateTextNode(folder.Name);
            XmlElement nameOfFolder = xmlDoc.CreateElement(NODE_NAME);
            XmlElement nameElement = xmlDoc.CreateElement(FOLDER);
            nameOfFolder.AppendChild(nameText);
            nameElement.AppendChild(nameOfFolder);
            XmlNode node = xmlDoc.SelectSingleNode(XPATH_EXPR);
            node.AppendChild(nameElement);

            string subFolderXML = Path.Combine(currentPath, folder.Name);
            CreateFileIfNotExist(subFolderXML);

            lock (syncLock)
            {
                CommonMethods.SaveXML(ref xmlDoc, xmlPath);
            }
        }

        private void RenameFolderObject(FolderCompareObject folder, string currentPath)
        {

            if (Directory.Exists(Path.Combine(currentPath, folder.Name)))
            {
                XmlDocument xmlDoc = new XmlDocument();
                string xmlPath = Path.Combine(currentPath, METADATAPATH);

                lock (syncLock)
                {
                    CommonMethods.LoadXML(ref xmlDoc, xmlPath);
                }

                XmlNode node = xmlDoc.SelectSingleNode(XPATH_EXPR + "/folder" + "[name='" + folder.Name + "']");
                if (node == null)
                {
                    CreateFolderObject(folder, currentPath);
                    return;
                }

                node.FirstChild.InnerText = folder.NewName;

                lock (syncLock)
                {
                    CommonMethods.SaveXML(ref xmlDoc, xmlPath);
                }
            }
            else
            {
                XmlDocument newXmlDoc = new XmlDocument();
                string editOldXML = Path.Combine(Path.Combine(currentPath, folder.NewName), METADATAPATH);

                lock (syncLock)
                {
                    CommonMethods.LoadXML(ref newXmlDoc, editOldXML);
                }

                XmlNode xmlNameNode = newXmlDoc.SelectSingleNode(XPATH_EXPR + "/name");
                xmlNameNode.InnerText = folder.NewName;

                lock (syncLock)
                {
                    CommonMethods.SaveXML(ref newXmlDoc, editOldXML);
                }

                string parentXML = Path.Combine(currentPath, METADATAPATH);
                XmlDocument parentXmlDoc = new XmlDocument();

                lock (syncLock)
                {
                    CommonMethods.LoadXML(ref parentXmlDoc, parentXML);
                }

                XmlNode parentXmlFolderNode = parentXmlDoc.SelectSingleNode(XPATH_EXPR + "/folder" + "[name='" + folder.Name + "']");
                parentXmlFolderNode.FirstChild.InnerText = folder.NewName;

                lock (syncLock)
                {
                    CommonMethods.SaveXML(ref parentXmlDoc, Path.Combine(currentPath, METADATAPATH));
                }

                newNameList.Add(folder.NewName);
                oldNameList.Add(folder.Name);
            }
        }

        private void DeleteFolderObject(FolderCompareObject folder, string currentPath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            string xmlPath = Path.Combine(currentPath, METADATAPATH);
            if (File.Exists(xmlPath))
            {
                lock (syncLock)
                {
                    CommonMethods.LoadXML(ref xmlDoc, xmlPath);
                }

                XmlNode node = xmlDoc.SelectSingleNode(XPATH_EXPR + "/folder" + "[name='" + folder.Name + "']");
                if (node == null)
                    return;
                node.ParentNode.RemoveChild(node);
                lock (syncLock)
                {
                    CommonMethods.SaveXML(ref xmlDoc, xmlPath);
                }

            }
        }

        private void HandleUnchangedOrPropagatedFolder(FolderCompareObject folder,
            int position, string folderPath)
        {
            string name = Path.Combine(folderPath, folder.Name);
            if (Directory.Exists(name)) //CREATE OR UPDATED
            {
                bool metaExist = folder.MetaExists[position];
                bool folderExist = folder.Exists[position];
                if (folderExist == true) //UPDATE
                {
                    CreateFolderObject(folder, folderPath);
                }
            }
            else                 //DELETE OR RENAME
            {
                if (folder.NewName != null)
                    RenameFolderObject(folder, folderPath);
                else
                    DeleteFolderObject(folder, folderPath);
            }
        }

        #endregion
    }
}
