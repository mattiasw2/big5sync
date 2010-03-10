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
        private string[] pathList = null;

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
            if (File.Exists(xmlPath) )
                return;

            lock (syncLock)
            {
                Directory.CreateDirectory(Path.Combine(path, META_DIR));
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

            string xmlPath = Path.Combine(currentPath, METADATAPATH);
            CreateFileIfNotExist(currentPath);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);
            FinalState? changeType = file.FinalState[counter];

            switch (changeType)
            {
                case FinalState.Created:
                    CreateFileObject(xmlDoc, file);
                    break;
                case FinalState.Updated:
                    UpdateFileObject(xmlDoc, file);
                    break;
                case FinalState.Deleted:
                    DeleteFileObject(xmlDoc, file);
                    break;
                case FinalState.Unchanged:
                    HandleUnchangedOrPropagatedFile(xmlDoc, file, counter, currentPath);
                    break;
                case FinalState.Propagated:
                    HandleUnchangedOrPropagatedFile(xmlDoc, file, counter, currentPath);
                    break;
            }

            xmlDoc.Save(xmlPath);
        }

        private void UpdateLastModifiedTime(XmlDocument xmlDoc)
        {
            XmlNode node = xmlDoc.SelectSingleNode(XPATH_EXPR + LAST_MODIFIED);
            node.InnerText = "3333333333333333333";
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


        private void CreateFileObject(XmlDocument xmlDoc, FileCompareObject file)
        {
            int position = GetPropagated(file);
            DoFileCleanUp(xmlDoc, file.Name);
            XmlText hashText = xmlDoc.CreateTextNode(file.Hash[position]);
            XmlText nameText = xmlDoc.CreateTextNode(file.Name);
            XmlText sizeText = xmlDoc.CreateTextNode(file.Length[position].ToString());
            XmlText lastModifiedText = xmlDoc.CreateTextNode(file.LastWriteTime[position].ToString());
            XmlText lastCreatedText = xmlDoc.CreateTextNode(file.CreationTime[position].ToString());


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
        }

        private void UpdateFileObject(XmlDocument xmlDoc, FileCompareObject file)
        {
            int position = GetPropagated(file);

            XmlNode node = xmlDoc.SelectSingleNode(XPATH_EXPR + "/files" + "[name='" + file.Name + "']");
            if (node == null)
            {
                CreateFileObject(xmlDoc, file);
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
                    nodes.InnerText = file.LastWriteTime[position].ToString();
                }
                else if (nodes.Name.Equals(NODE_LAST_CREATED))
                {
                    nodes.InnerText = file.CreationTime[position].ToString();
                }
            }
        }

        private void DeleteFileObject(XmlDocument xmlDoc, FileCompareObject file)
        {
            XmlNode node = xmlDoc.SelectSingleNode(XPATH_EXPR + "/files" + "[name='" + file.Name + "']");
            if (node == null)
                return;
            node.ParentNode.RemoveChild(node);
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

        private void HandleUnchangedOrPropagatedFile(XmlDocument xmlDoc, FileCompareObject file, int position, string filePath)
        {
            string name = Path.Combine(filePath, file.Name);
            if (File.Exists(name)) //CREATE OR UPDATED
            {
                bool metaExist = file.MetaExists[position];
                bool fileExist = file.Exists[position];
                if (metaExist == true && fileExist == true) //UPDATE
                {
                    UpdateFileObject(xmlDoc, file);
                }
                else  //NEW
                {
                    CreateFileObject(xmlDoc, file);
                }
            }
            else                 //DELETE OR RENAME
            {
                DeleteFileObject(xmlDoc, file);
            }
        }

        private void ProcessFolderFinalState(string currentPath, FolderCompareObject folder, int counter)
        {
            string xmlPath = Path.Combine(currentPath, METADATAPATH);
            CreateFileIfNotExist(currentPath);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);
            FinalState?[] finalStateList = folder.FinalState;
            FinalState? changeType = finalStateList[counter];
            switch (changeType)
            {
                case FinalState.Created:
                    CreateFolderObject(xmlDoc, folder);
                    break;
                case FinalState.Deleted:
                    DeleteFolderObject(xmlDoc, folder);
                    break;
                case FinalState.Propagated:
                    HandleUnchangedOrPropagatedFolder(xmlDoc, folder, counter, currentPath);
                    break;
            }
            xmlDoc.Save(xmlPath);
        }


        private void CreateFolderObject(XmlDocument xmlDoc, FolderCompareObject folder)
        {
            DoFolderCleanUp(xmlDoc, folder.Name);
            XmlText nameText = xmlDoc.CreateTextNode(folder.Name);
            XmlElement nameOfFolder = xmlDoc.CreateElement(NODE_NAME);
            XmlElement nameElement = xmlDoc.CreateElement(FOLDER);
            nameOfFolder.AppendChild(nameText);
            nameElement.AppendChild(nameOfFolder);
            XmlNode node = xmlDoc.SelectSingleNode(XPATH_EXPR);
            node.AppendChild(nameElement);
        }

        private void DeleteFolderObject(XmlDocument xmlDoc, FolderCompareObject folder)
        {
            XmlNode node = xmlDoc.SelectSingleNode(XPATH_EXPR + "/folder" + "[name='" + folder.Name + "']");
            if (node == null)
                return;
            node.ParentNode.RemoveChild(node);
        }

        private void HandleUnchangedOrPropagatedFolder(XmlDocument xmlDoc, FolderCompareObject folder,
            int position, string folderPath)
        {
            string name = Path.Combine(folderPath, folder.Name);
            if (Directory.Exists(name)) //CREATE OR UPDATED
            {
                bool metaExist = folder.MetaExists[position];
                bool folderExist = folder.Exists[position];
                if (folderExist == true) //UPDATE
                {
                    CreateFolderObject(xmlDoc, folder);
                }
            }
            else                 //DELETE OR RENAME
            {
                DeleteFolderObject(xmlDoc, folder);
            }
        }

        #endregion
    }
}
