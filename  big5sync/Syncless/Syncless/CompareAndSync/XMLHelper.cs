using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using Syncless.CompareAndSync.XMLWriteObject;
using Syncless.CompareAndSync.Enum;

namespace Syncless.CompareAndSync
{
    class XMLHelper
    {

        private const string META_DIR = ".syncless";
        private const string XML_NAME = "syncless.xml";
        private const string METADATAPATH = META_DIR + "\\" + XML_NAME;
        private const string XPATH_EXPR = "/meta-data";
        private const string NODE_NAME = "name";
        private const string NODE_SIZE = "size";
        private const string NODE_HASH = "hash";
        private const string NODE_LAST_MODIFIED = "last_modified";
        private const string NODE_LAST_CREATED = "last_created";
        private const string FOLDER = "folder";
        private const string FILES = "files";
        private static readonly object syncLock = new object();

        public static void UpdateXML(BaseXMLWriteObject xmlWriteList)
        {
           
            if (xmlWriteList is XMLWriteFolderObject)
            {
                HandleFolder(xmlWriteList);
                return;
            }

            switch (xmlWriteList.ChangeType)
            {
                case MetaChangeType.New:
                     CreateFile((XMLWriteFileObject)xmlWriteList);
                     break;
                case MetaChangeType.Delete:
                     DeleteFile((XMLWriteFileObject)xmlWriteList);
                     break;
                case MetaChangeType.Rename:
                     RenameFile((XMLWriteFileObject)xmlWriteList);
                     break;
                case MetaChangeType.Update:
                     UpdateFile((XMLWriteFileObject)xmlWriteList);
                     break;
            }
        }

        private static void CreateFileIfNotExist(string path)
        {
            string xmlPath = Path.Combine(path, METADATAPATH);
            if (File.Exists(xmlPath))
                return;

            lock (syncLock)
            {
                DirectoryInfo di = Directory.CreateDirectory(Path.Combine(path, META_DIR));
                di.Attributes = FileAttributes.Directory |FileAttributes.Hidden;
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
        private static void CreateFile(XMLWriteFileObject xmlWriteObj)
        {
            XmlDocument xmlDoc = new XmlDocument();
            string xmlFilePath = Path.Combine(xmlWriteObj.FullPath, METADATAPATH);
            CreateFileIfNotExist(xmlWriteObj.FullPath);
            xmlDoc.Load(xmlFilePath);

            DoFileCleanUp(xmlDoc, xmlWriteObj.Name);
            XmlText nameText = xmlDoc.CreateTextNode(xmlWriteObj.Name);
            XmlText hashText = xmlDoc.CreateTextNode(xmlWriteObj.Hash);
            XmlText sizeText = xmlDoc.CreateTextNode(xmlWriteObj.Size.ToString());
            XmlText createdTimeText = xmlDoc.CreateTextNode(xmlWriteObj.CreationTime.ToString());
            XmlText lastModifiedText = xmlDoc.CreateTextNode(xmlWriteObj.LastModified.ToString());

            XmlElement nameElement = xmlDoc.CreateElement(NODE_NAME);
            XmlElement hashElement = xmlDoc.CreateElement(NODE_HASH);
            XmlElement sizeElement = xmlDoc.CreateElement(NODE_SIZE);
            XmlElement createdTimeElement = xmlDoc.CreateElement(NODE_LAST_CREATED);
            XmlElement lastModifiedElement = xmlDoc.CreateElement(NODE_LAST_MODIFIED);
            XmlElement fileElement = xmlDoc.CreateElement(FILES);

            nameElement.AppendChild(nameText);
            hashElement.AppendChild(hashText);
            sizeElement.AppendChild(sizeText);
            createdTimeElement.AppendChild(createdTimeText);
            lastModifiedElement.AppendChild(lastModifiedText);

            fileElement.AppendChild(nameElement);
            fileElement.AppendChild(sizeElement);
            fileElement.AppendChild(hashElement);
            fileElement.AppendChild(lastModifiedElement);
            fileElement.AppendChild(createdTimeElement);

            XmlNode rootNode = xmlDoc.SelectSingleNode(XPATH_EXPR);
            rootNode.AppendChild(fileElement);

            xmlDoc.Save(xmlFilePath);
        }

        private static void UpdateFile(XMLWriteFileObject xmlWriteObj)
        {
            XmlDocument xmlDoc = new XmlDocument();
            string xmlFilePath = Path.Combine(xmlWriteObj.FullPath, METADATAPATH);
            CreateFileIfNotExist(xmlWriteObj.FullPath);
            xmlDoc.Load(xmlFilePath);

            XmlNode node = xmlDoc.SelectSingleNode(XPATH_EXPR + "/files" + "[name='" + xmlWriteObj.Name + "']");
            if (node == null)
            {
                xmlDoc.Save(xmlFilePath);
                CreateFile(xmlWriteObj);
                return;
            }

            XmlNodeList childNodeList = node.ChildNodes;
            for (int i = 0; i < childNodeList.Count; i++)
            {
                XmlNode nodes = childNodeList[i];
                if (nodes.Name.Equals(NODE_SIZE))
                {
                    nodes.InnerText = xmlWriteObj.Size.ToString();
                }
                else if (nodes.Name.Equals(NODE_HASH))
                {
                    nodes.InnerText = xmlWriteObj.Hash.ToString();
                }
                else if (nodes.Name.Equals(NODE_NAME))
                {
                    nodes.InnerText = xmlWriteObj.Name.ToString();
                }
                else if (nodes.Name.Equals(NODE_LAST_MODIFIED))
                {
                    nodes.InnerText = xmlWriteObj.LastModified.ToString();
                }
                else if (nodes.Name.Equals(NODE_LAST_CREATED))
                {
                    nodes.InnerText = xmlWriteObj.CreationTime.ToString();
                }
            }

            xmlDoc.Save(xmlFilePath);
        }

       
        private static void RenameFile(BaseXMLWriteObject xmlWriteObj)
        {
            XmlDocument xmlDoc = new XmlDocument();
            string xmlFilePath = Path.Combine(xmlWriteObj.FullPath, METADATAPATH);
            CreateFileIfNotExist(xmlWriteObj.FullPath);
            xmlDoc.Load(xmlFilePath);

            XmlNode node = xmlDoc.SelectSingleNode(XPATH_EXPR + "/files" + "[name='" + xmlWriteObj.Name + "']");
            if (node == null)
                return;
            node.FirstChild.InnerText = xmlWriteObj.NewName;

            xmlDoc.Save(xmlFilePath);
        }

        private static void DeleteFile(BaseXMLWriteObject xmlWriteObj)
        {
            XmlDocument xmlDoc = new XmlDocument();
            string xmlFilePath = Path.Combine(xmlWriteObj.FullPath, METADATAPATH);
            if (File.Exists(xmlFilePath))
            {
                xmlDoc.Load(xmlFilePath);
                XmlNode node = xmlDoc.SelectSingleNode(XPATH_EXPR + "/files" + "[name='" + xmlWriteObj.Name + "']");
                if (node == null)
                    return;
                node.ParentNode.RemoveChild(node);
                xmlDoc.Save(xmlFilePath);
            }

            
        }

        private static string GetLastFileIndex(string filePath)
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

        private static void DoFileCleanUp(XmlDocument xmlDoc, string name)
        {
            XmlNode node = xmlDoc.SelectSingleNode(XPATH_EXPR + "/files" + "[name='" + name + "']");
            if (node == null)
                return;
            node.ParentNode.RemoveChild(node);
        }

        private static void DoFolderCleanUp(XmlDocument xmlDoc, string name)
        {
            XmlNode node = xmlDoc.SelectSingleNode(XPATH_EXPR + "/folder" + "[name='" + name + "']");
            if (node == null)
                return;
            node.ParentNode.RemoveChild(node);
        }

        private static void HandleFolder(BaseXMLWriteObject xmlWriteObj)
        {
            switch (xmlWriteObj.ChangeType)
            {
                case MetaChangeType.New:
                    CreateFolder(xmlWriteObj);
                    break;
                case MetaChangeType.Rename:
                    RenameFolder(xmlWriteObj);
                    break;
                case MetaChangeType.Delete:
                    DeleteFolder(xmlWriteObj);
                    break;
            }
        }

        private static void CreateFolder(BaseXMLWriteObject xmlWriteObj)
        {
            XmlDocument xmlDoc = new XmlDocument();
            string xmlFilePath = Path.Combine(xmlWriteObj.FullPath, METADATAPATH);
            CreateFileIfNotExist(xmlWriteObj.FullPath);
            xmlDoc.Load(xmlFilePath);

            DoFolderCleanUp(xmlDoc, xmlWriteObj.Name);
            XmlText nameText = xmlDoc.CreateTextNode(xmlWriteObj.Name);
            XmlElement nameOfFolder = xmlDoc.CreateElement(NODE_NAME);
            XmlElement folder = xmlDoc.CreateElement(FOLDER);
            nameOfFolder.AppendChild(nameText);
            folder.AppendChild(nameOfFolder);

            XmlNode rootNode = xmlDoc.SelectSingleNode(XPATH_EXPR);
            rootNode.AppendChild(folder);

            xmlDoc.Save(xmlFilePath);
        }

        private static void RenameFolder(BaseXMLWriteObject xmlWriteObj)
        {
            XmlDocument xmlDoc = new XmlDocument();
            string xmlPath = Path.Combine(xmlWriteObj.FullPath , METADATAPATH);
            CreateFileIfNotExist(xmlWriteObj.FullPath);
            xmlDoc.Load(xmlPath);

            XmlNode node = xmlDoc.SelectSingleNode(XPATH_EXPR + "/folder" + "[name='" + xmlWriteObj.Name + "']");
            if (node == null)
                return;
            node.FirstChild.InnerText = xmlWriteObj.NewName;
            xmlDoc.Save(xmlPath);

            XmlDocument subFolderXmlDoc = new XmlDocument();
            string subFolder = Path.Combine(xmlWriteObj.FullPath, xmlWriteObj.NewName);
            string subFolderXmlPath = Path.Combine(subFolder, METADATAPATH);
            CreateFileIfNotExist(subFolder);
            subFolderXmlDoc.Load(subFolderXmlPath);
            XmlNode subFolderNode = subFolderXmlDoc.SelectSingleNode(XPATH_EXPR + "/name");
            if (subFolderNode == null)
                return;
            subFolderNode.InnerText = xmlWriteObj.NewName;
            subFolderXmlDoc.Save(subFolderXmlPath);

        }

        private static void DeleteFolder(BaseXMLWriteObject xmlWriteObj)
        {
            string xmlFilePath = Path.Combine(xmlWriteObj.FullPath, METADATAPATH);
            XmlDocument xmlDoc = new XmlDocument();
            if (File.Exists(xmlFilePath))
            {
                xmlDoc.Load(xmlFilePath);
                XmlNode node = xmlDoc.SelectSingleNode(XPATH_EXPR + "/folder" + "[name='" + xmlWriteObj.Name + "']");
                if (node == null)
                    return;
                node.ParentNode.RemoveChild(node);
                xmlDoc.Save(xmlFilePath);
            }
        }
    }
}
