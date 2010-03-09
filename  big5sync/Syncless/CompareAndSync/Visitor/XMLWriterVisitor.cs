using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CompareAndSync.CompareObject;
using System.Xml;
using System.IO;

namespace CompareAndSync.Visitor
{
    public class XMLWriterVisitor : IVisitor
    {
        #region IVisitor Members

        private const string META_DIR = ".syncless";
        private const string METADATAPATH = META_DIR + @"\syncless.xml";
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
        private static readonly object syncLock = new object(); 
        private string[] pathList = null;

        public void Visit(FileCompareObject file, int level, string[] currentPath)
        {
           
            int highestPriority = -1;
            int counter = 0;

            int [] priorityList = file.Priority;
            for (int i = 0; i < priorityList.Length; i++)
            {
                if (highestPriority > priorityList[i])
                {
                    highestPriority = priorityList[i];
                    counter = i;
                }
            }

            for (int i = 0; i < currentPath.Length; i++)
            {
                if (currentPath[i].Contains(META_DIR))
                    continue;
                ProcessMetaChangeType(currentPath[i], file, counter);

            }
        }

        public void Visit(FolderCompareObject folder, int level, string[] currentPath)
        {
            /**/
            
        }

        public void Visit(RootCompareObject root)
        {
            pathList = root.Paths;
        }

        private void CreateFileIfNotExist(string path)
        {
            string xmlPath = Path.Combine(path, METADATAPATH);
            if (File.Exists(xmlPath))
                return ;

            lock (syncLock)
            {
                Directory.CreateDirectory(Path.Combine(path, META_DIR));
                XmlTextWriter writer = new XmlTextWriter(xmlPath, null);
                writer.Formatting = Formatting.Indented;
                writer.WriteStartDocument();
                writer.WriteStartElement("meta-data");
                writer.WriteElementString("last_modified", (DateTime.Now.Ticks).ToString());
                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Flush();
                writer.Close();
            }
        }

        private void ProcessMetaChangeType(string currentPath , FileCompareObject file,int counter)
        {

            string xmlPath = Path.Combine(currentPath, METADATAPATH);
            CreateFileIfNotExist(currentPath);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);
            FinalState? changeType = file.FinalState[counter];
           
            string hash = "";
            string size = "";
            
            switch (changeType)
            {
                case FinalState.Created:
                    CreateFileObject(xmlDoc, file, counter);
                    break;
                case FinalState.Updated:
                    UpdateFile(xmlDoc, file, counter);
                    break;
                case FinalState.Deleted:
                    DeleteFile(xmlDoc, file, counter);
                    break;
            }

            xmlDoc.Save(xmlPath);
        }

        private void UpdateLastModifiedTime(XmlDocument xmlDoc)
        {
            XmlNode node = xmlDoc.SelectSingleNode(XPATH_EXPR+LAST_MODIFIED);
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
                if(folderPath.Equals(""))
                    folderPath = splitWords[i];
                else
                    folderPath = folderPath + "\\" + splitWords[i];
            }

            return folderPath;
        }

        private void CreateFolderObject(XmlDocument xmlDoc, FolderCompareObject folder, int position)
        {
            XmlText nameText = xmlDoc.CreateTextNode(folder.Name);
            XmlElement nameOfFolder = xmlDoc.CreateElement(NAME_OF_FOLDER);
            XmlElement nameElement = xmlDoc.CreateElement(FOLDER);
            nameOfFolder.AppendChild(nameText);
            nameElement.AppendChild(nameOfFolder);
            XmlNode node = xmlDoc.SelectSingleNode(XPATH_EXPR);
            node.AppendChild(nameElement);
        }

        private void CreateFileObject(XmlDocument xmlDoc , FileCompareObject file, int position)
        {
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

        private void UpdateFile(XmlDocument xmlDoc, FileCompareObject file, int position)
        {
            XmlNode node = xmlDoc.SelectSingleNode(XPATH_EXPR + "/files" + "[name='" + file.Name + "']");
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

        private void DeleteFile(XmlDocument xmlDoc, FileCompareObject file, int position)
        {
            XmlNode node = xmlDoc.SelectSingleNode(XPATH_EXPR + "/files" + "[name='" + file.Name + "']");
            node.ParentNode.RemoveChild(node);
        }

        #endregion
    }
}
