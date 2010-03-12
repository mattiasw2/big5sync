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

        public static void UpdateXML(List<BaseXMLWriteObject> xmlWriteList)
        {
            XmlDocument xmlDoc = new XmlDocument();
            foreach (BaseXMLWriteObject xmlWriteObj in xmlWriteList)
            {
                string xmlPath = Path.Combine(xmlWriteObj.FullPath, METADATAPATH);
                CreateFileIfNotExist(xmlWriteObj.FullPath);
                xmlDoc.Load(xmlPath);
                if (xmlWriteObj is XMLWriteFolderObject)
                {
                    HandleFolder(xmlDoc, xmlWriteObj);
                    xmlDoc.Save(xmlPath);
                    continue;
                }

                switch (xmlWriteObj.ChangeType)
                {
                    case MetaChangeType.New:
                        CreateFile(xmlDoc, (XMLWriteFileObject)xmlWriteObj);
                        break;
                    case MetaChangeType.Delete:
                        DeleteFile(xmlDoc, (XMLWriteFileObject)xmlWriteObj);
                        break;
                    case MetaChangeType.Rename:
                        RenameFile(xmlDoc, (XMLWriteFileObject)xmlWriteObj);
                        break;
                    case MetaChangeType.Update:
                        UpdateFile(xmlDoc, (XMLWriteFileObject)xmlWriteObj);
                        break;
                }

                xmlDoc.Save(xmlPath);
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
        private static void CreateFile(XmlDocument xmlDoc, XMLWriteFileObject xmlWriteObj)
        {
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
        }

        private static void UpdateFile(XmlDocument xmlDoc, XMLWriteFileObject xmlWriteObj)
        {
            XmlNode node = xmlDoc.SelectSingleNode(XPATH_EXPR + "/files" + "[name='" + xmlWriteObj.Name + "']");
            if (node == null)
            {
                CreateFile(xmlDoc, xmlWriteObj);
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
        }

        private static void RenameFile(XmlDocument xmlDoc, BaseXMLWriteObject xmlWriteObj)
        {
            XmlNode node = xmlDoc.SelectSingleNode(XPATH_EXPR + "/files" + "[name='" + xmlWriteObj.Name + "']");
            if (node == null)
                return;
            node.FirstChild.InnerText = xmlWriteObj.NewName;
        }

        private static void DeleteFile(XmlDocument xmlDoc, BaseXMLWriteObject xmlWriteObj)
        {
            XmlNode node = xmlDoc.SelectSingleNode(XPATH_EXPR + "/files" + "[name='" + xmlWriteObj.Name + "']");
            if (node == null)
                return;
            node.ParentNode.RemoveChild(node);
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

        private static void HandleFolder(XmlDocument xmlDoc, BaseXMLWriteObject xmlWriteObj)
        {
            switch (xmlWriteObj.ChangeType)
            {
                case MetaChangeType.New:
                    CreateFolder(xmlDoc, xmlWriteObj);
                    break;
                case MetaChangeType.Rename:
                    RenameFolder(xmlDoc, xmlWriteObj);
                    break;
                case MetaChangeType.Delete:
                    break;
            }
        }

        private static void CreateFolder(XmlDocument xmlDoc, BaseXMLWriteObject xmlWriteObj)
        {
            DoFolderCleanUp(xmlDoc, xmlWriteObj.Name);
            XmlText nameText = xmlDoc.CreateTextNode(xmlWriteObj.Name);
            XmlElement nameOfFolder = xmlDoc.CreateElement(NODE_NAME);
            XmlElement folder = xmlDoc.CreateElement(FOLDER);
            nameOfFolder.AppendChild(nameText);
            folder.AppendChild(nameOfFolder);

            XmlNode rootNode = xmlDoc.SelectSingleNode(XPATH_EXPR);
            rootNode.AppendChild(folder);
        }

        private static void RenameFolder(XmlDocument xmlDoc, BaseXMLWriteObject xmlWriteObj)
        {
            XmlNode node = xmlDoc.SelectSingleNode(XPATH_EXPR + "/folder" + "[name='" + xmlWriteObj.Name + "']");
            if (node == null)
                return;
            node.FirstChild.InnerText = xmlWriteObj.NewName;
        }

        private static void DeleteFolder(XmlDocument xmlDoc, BaseXMLWriteObject xmlWriteObj)
        {
            XmlNode node = xmlDoc.SelectSingleNode(XPATH_EXPR + "/folder" + "[name='" + xmlWriteObj.Name + "']");
            if (node == null)
                return;
            node.ParentNode.RemoveChild(node);
        }
    }
}
