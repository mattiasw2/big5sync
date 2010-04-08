using System;
using System.IO;
using System.Xml;
using Syncless.CompareAndSync.Enum;
using Syncless.CompareAndSync.Seamless.XMLWriteObject;

namespace Syncless.CompareAndSync.Seamless
{
    public class SeamlessXMLHelper
    {

        #region Main Method

        /// <summary>
        /// UpdateXML is the only public static method that is accessible by other classes. Given a 
        /// BaseXMLWriteObject , it will differentiate it between a folder and a file , then let the respective
        /// methods handle it
        /// </summary>
        /// <param name="xmlWriteList"></param>
        public static void UpdateXML(BaseXMLWriteObject xmlWriteList)
        {
            if (xmlWriteList is XMLWriteFolderObject)
                HandleFolder(xmlWriteList);
            else
                HandleFile(xmlWriteList);
        }

        #endregion

        #region File Operations

        /// <summary>
        /// HandleFile method will take in a BaseXMLWriteObject that is of a file type. It will check the
        /// MetaChangeType and let the respective methods handle it.
        /// </summary>
        /// <param name="xmlWriteList"></param>
        private static void HandleFile(BaseXMLWriteObject xmlWriteList)
        {
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

        /// <summary>
        /// Given a XMLWriteFileObject , it will load the respective xml file and then populate a new file
        /// element in the xml file with the values in XMLWriteFileObject. Subsequently , it will remove any nodes in 
        /// the todo file
        /// </summary>
        /// <param name="xmlWriteObj"></param>
        private static void CreateFile(XMLWriteFileObject xmlWriteObj)
        {
            XmlDocument xmlDoc = new XmlDocument();
            string xmlFilePath = Path.Combine(xmlWriteObj.Parent, CommonXMLConstants.MetadataPath);
            CommonMethods.CreateFileIfNotExist(xmlWriteObj.Parent);
            CommonMethods.LoadXML(ref xmlDoc, xmlFilePath);

            CommonMethods.DoFileCleanUp(xmlDoc, xmlWriteObj.Name);
            XmlText nameText = xmlDoc.CreateTextNode(xmlWriteObj.Name);
            XmlText hashText = xmlDoc.CreateTextNode(xmlWriteObj.Hash);
            XmlText sizeText = xmlDoc.CreateTextNode(xmlWriteObj.Size.ToString());
            XmlText createdTimeText = xmlDoc.CreateTextNode(xmlWriteObj.CreationTime.ToString());
            XmlText lastModifiedText = xmlDoc.CreateTextNode(xmlWriteObj.LastModified.ToString());
            XmlText lastUpdatedText = xmlDoc.CreateTextNode(xmlWriteObj.MetaUpdated.ToString());

            XmlElement nameElement = xmlDoc.CreateElement(CommonXMLConstants.NodeName);
            XmlElement hashElement = xmlDoc.CreateElement(CommonXMLConstants.NodeHash);
            XmlElement sizeElement = xmlDoc.CreateElement(CommonXMLConstants.NodeSize);
            XmlElement createdTimeElement = xmlDoc.CreateElement(CommonXMLConstants.NodeLastCreated);
            XmlElement lastModifiedElement = xmlDoc.CreateElement(CommonXMLConstants.NodeLastModified);
            XmlElement lastUpdatedElement = xmlDoc.CreateElement(CommonXMLConstants.NodeLastUpdated);
            XmlElement fileElement = xmlDoc.CreateElement(CommonXMLConstants.NodeFile);

            nameElement.AppendChild(nameText);
            hashElement.AppendChild(hashText);
            sizeElement.AppendChild(sizeText);
            createdTimeElement.AppendChild(createdTimeText);
            lastModifiedElement.AppendChild(lastModifiedText);
            lastUpdatedElement.AppendChild(lastUpdatedText);

            fileElement.AppendChild(nameElement);
            fileElement.AppendChild(sizeElement);
            fileElement.AppendChild(hashElement);
            fileElement.AppendChild(lastModifiedElement);
            fileElement.AppendChild(createdTimeElement);
            fileElement.AppendChild(lastUpdatedElement);

            XmlNode rootNode = xmlDoc.SelectSingleNode(CommonXMLConstants.XPathExpr);
            rootNode.AppendChild(fileElement);
            CommonMethods.SaveXML(ref xmlDoc, xmlFilePath);
            DeleteFileToDoByName(xmlWriteObj);
        }

        /// <summary>
        /// Given a XMLWriteFileObject , it will look for the respective xml file and load it.After which , it
        /// will search for the node which contains the same name as XMLWriteFileObject that is passed in , and
        /// update the different element contents. Subsequently , it will remove any nodes in the todo file 
        /// with the same name
        /// </summary>
        /// <param name="xmlWriteObj"></param>
        private static void UpdateFile(XMLWriteFileObject xmlWriteObj)
        {
            XmlDocument xmlDoc = new XmlDocument();
            string xmlFilePath = Path.Combine(xmlWriteObj.Parent, CommonXMLConstants.MetadataPath);
            CommonMethods.CreateFileIfNotExist(xmlWriteObj.Parent);
            CommonMethods.LoadXML(ref xmlDoc, xmlFilePath);

            XmlNode node = xmlDoc.SelectSingleNode(CommonXMLConstants.XPathExpr + CommonXMLConstants.XPathFile + "[name=" + CommonMethods.ParseXPathString(xmlWriteObj.Name) + "]");
            
            // if the node does not exist , then create the node
            if (node == null)
            {
                CommonMethods.SaveXML(ref xmlDoc, xmlFilePath);
                CreateFile(xmlWriteObj);
                return;
            }

            XmlNodeList childNodeList = node.ChildNodes;
            for (int i = 0; i < childNodeList.Count; i++)
            {
                XmlNode nodes = childNodeList[i];

                switch (nodes.Name)
                {
                    case CommonXMLConstants.NodeSize:
                        nodes.InnerText = xmlWriteObj.Size.ToString();
                        break;
                    case CommonXMLConstants.NodeHash:
                        nodes.InnerText = xmlWriteObj.Hash;
                        break;
                    case CommonXMLConstants.NodeName:
                        nodes.InnerText = xmlWriteObj.Name;
                        break;
                    case CommonXMLConstants.NodeLastModified:
                        nodes.InnerText = xmlWriteObj.LastModified.ToString();
                        break;
                    case CommonXMLConstants.NodeLastCreated:
                        nodes.InnerText = xmlWriteObj.CreationTime.ToString();
                        break;
                    case CommonXMLConstants.NodeLastUpdated:
                        nodes.InnerText = xmlWriteObj.MetaUpdated.ToString();
                        break;
                }
            }

            CommonMethods.SaveXML(ref xmlDoc, xmlFilePath);
            DeleteFileToDoByName(xmlWriteObj);
        }

        /// <summary>
        /// Given a XMLWriteFileObject , it will look for the respective xml file and load it. It will then
        /// rename the existing element in the xml based on the name. Upon doing so , it will create a new node
        /// in the todo file.
        /// </summary>
        /// <param name="xmlWriteObj"></param>
        private static void RenameFile(XMLWriteFileObject xmlWriteObj)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode tempNode = null;
            string xmlFilePath = Path.Combine(xmlWriteObj.Parent, CommonXMLConstants.MetadataPath);
            CommonMethods.CreateFileIfNotExist(xmlWriteObj.Parent);
            CommonMethods.LoadXML(ref xmlDoc, xmlFilePath);

            XmlNode node = xmlDoc.SelectSingleNode(CommonXMLConstants.XPathExpr + CommonXMLConstants.XPathFile + "[name=" + CommonMethods.ParseXPathString(xmlWriteObj.Name) + "]");
            
            // cif the node does not exist, then create the node
            if (node == null)
            {
                CommonMethods.SaveXML(ref xmlDoc, xmlFilePath);
                CreateFile(xmlWriteObj);
                return;
            }

            tempNode = node.Clone();
            node.FirstChild.InnerText = xmlWriteObj.NewName;
            node.LastChild.InnerText = xmlWriteObj.MetaUpdated.ToString();
            CommonMethods.SaveXML(ref xmlDoc, xmlFilePath);
            GenerateFileToDo(xmlWriteObj, tempNode);
        }

        /// <summary>
        /// Looks for the xml file given the XMLWriteFileObject , it will delete node and generate a new node
        /// in the todo file
        /// </summary>
        /// <param name="xmlWriteObj"></param>
        private static void DeleteFile(XMLWriteFileObject xmlWriteObj)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode tempNode = null;
            string xmlFilePath = Path.Combine(xmlWriteObj.Parent, CommonXMLConstants.MetadataPath);
            if (File.Exists(xmlFilePath))
            {
                CommonMethods.LoadXML(ref xmlDoc, xmlFilePath);
                XmlNode node = xmlDoc.SelectSingleNode(CommonXMLConstants.XPathExpr + CommonXMLConstants.XPathFile + "[name=" + CommonMethods.ParseXPathString(xmlWriteObj.Name) + "]");
                if (node == null)
                    return;
                tempNode = node.Clone();
                node.ParentNode.RemoveChild(node);
                CommonMethods.SaveXML(ref xmlDoc, xmlFilePath);
            }

            GenerateFileToDo(xmlWriteObj, tempNode);
        }

        #endregion

        #region Folder Operations

        /// <summary>
        /// Given a BaseXMLWriteObject which is a folder , checks for the MetaChangeType and let the respective
        /// method handle it.
        /// </summary>
        /// <param name="xmlWriteObj"></param>
        private static void HandleFolder(BaseXMLWriteObject xmlWriteObj)
        {
            switch (xmlWriteObj.ChangeType)
            {
                case MetaChangeType.New:
                    CreateFolder((XMLWriteFolderObject)xmlWriteObj);
                    break;
                case MetaChangeType.Rename:
                    RenameFolder((XMLWriteFolderObject)xmlWriteObj);
                    break;
                case MetaChangeType.Delete:
                    DeleteFolder((XMLWriteFolderObject)xmlWriteObj);
                    break;
            }
        }

        /// <summary>
        /// Given a XMLWriteFolderObject , it will load the xml file. 
        /// After which , it will create a new node based on the values given by XMLWriteFolderObject. Next , it
        /// will try to delete the node with the same name in the todo file
        /// </summary>
        /// <param name="xmlWriteObj"></param>
        private static void CreateFolder(XMLWriteFolderObject xmlWriteObj)
        {
            XmlDocument xmlDoc = new XmlDocument();
            string xmlFilePath = Path.Combine(xmlWriteObj.Parent, CommonXMLConstants.MetadataPath);
            CommonMethods.CreateFileIfNotExist(xmlWriteObj.Parent);
            CommonMethods.LoadXML(ref xmlDoc, xmlFilePath);

            CommonMethods.DoFolderCleanUp(xmlDoc, xmlWriteObj.Name);
            XmlText nameText = xmlDoc.CreateTextNode(xmlWriteObj.Name);
            XmlText lastUpdatedText = xmlDoc.CreateTextNode(xmlWriteObj.MetaUpdated.ToString());

            XmlElement nameElement = xmlDoc.CreateElement(CommonXMLConstants.NodeName);
            XmlElement lastUpdatedElement = xmlDoc.CreateElement(CommonXMLConstants.NodeLastUpdated);
            XmlElement folder = xmlDoc.CreateElement(CommonXMLConstants.NodeFolder);
            nameElement.AppendChild(nameText);
            lastUpdatedElement.AppendChild(lastUpdatedText);
            folder.AppendChild(nameElement);
            folder.AppendChild(lastUpdatedElement);

            XmlNode rootNode = xmlDoc.SelectSingleNode(CommonXMLConstants.XPathExpr);
            rootNode.AppendChild(folder);
            CommonMethods.SaveXML(ref xmlDoc, xmlFilePath);
            DeleteFolderToDoByName(xmlWriteObj);
        }

        /// <summary>
        /// Given a XMLWriteFolderObject , it will first load the xml file. Then it will locate the current node
        /// and rename it to the new name. After which  , it will a new folder node in the todo file
        /// </summary>
        /// <param name="xmlWriteObj"></param>
        private static void RenameFolder(XMLWriteFolderObject xmlWriteObj)
        {
            XmlDocument xmlDoc = new XmlDocument();
            string xmlPath = Path.Combine(xmlWriteObj.Parent, CommonXMLConstants.MetadataPath);
            CommonMethods.CreateFileIfNotExist(xmlWriteObj.Parent);
            CommonMethods.LoadXML(ref xmlDoc, xmlPath);

            XmlNode node = xmlDoc.SelectSingleNode(CommonXMLConstants.XPathExpr + CommonXMLConstants.XPathFolder + "[name=" + CommonMethods.ParseXPathString(xmlWriteObj.Name) + "]");
            if (node == null)
            {
                CreateFolder(xmlWriteObj);
            }
            else
            {
                node.FirstChild.InnerText = xmlWriteObj.NewName;
                node.LastChild.InnerText = xmlWriteObj.MetaUpdated.ToString();
                CommonMethods.SaveXML(ref xmlDoc, xmlPath);
            }

            XmlDocument subFolderXmlDoc = new XmlDocument();
            string subFolder = Path.Combine(xmlWriteObj.Parent, xmlWriteObj.NewName);
            string subFolderXmlPath = Path.Combine(subFolder, CommonXMLConstants.MetadataPath);
            CommonMethods.CreateFileIfNotExist(subFolder);
            CommonMethods.LoadXML(ref subFolderXmlDoc, subFolderXmlPath);

            XmlNode subFolderNode = subFolderXmlDoc.SelectSingleNode(CommonXMLConstants.XPathExpr + "/name");
            if (subFolderNode != null)
            {
                subFolderNode.InnerText = xmlWriteObj.NewName;
                CommonMethods.SaveXML(ref subFolderXmlDoc, subFolderXmlPath);
                GenerateFolderToDo(xmlWriteObj);
            }
        }

        /// <summary>
        /// Given the XMLWriteFolderObject , it will first load the xml file. Then it will delete the folder
        /// node based on the name and create a new node in the todo file
        /// </summary>
        /// <param name="xmlWriteObj"></param>
        private static void DeleteFolder(XMLWriteFolderObject xmlWriteObj)
        {
            string xmlFilePath = Path.Combine(xmlWriteObj.Parent, CommonXMLConstants.MetadataPath);
            XmlDocument xmlDoc = new XmlDocument();
            if (File.Exists(xmlFilePath))
            {
                CommonMethods.LoadXML(ref xmlDoc, xmlFilePath);
                XmlNode node = xmlDoc.SelectSingleNode(CommonXMLConstants.XPathExpr + CommonXMLConstants.XPathFolder + "[name=" + CommonMethods.ParseXPathString(xmlWriteObj.Name) + "]");
                if (node == null)
                    return;
                node.ParentNode.RemoveChild(node);
                CommonMethods.SaveXML(ref xmlDoc, xmlFilePath);
            }

            GenerateFolderToDo(xmlWriteObj);
        }

        #endregion

        #region ToDo Operations

        /// <summary>
        /// Given a XMLWriteFileObject and an XmlNode , load the xml file and create a extra file node in the
        /// todo file
        /// </summary>
        /// <param name="xmlWriteObj"></param>
        /// <param name="deletedNode"> A cloned node </param>
        private static void GenerateFileToDo(XMLWriteFileObject xmlWriteObj, XmlNode deletedNode)
        {
            if (deletedNode == null)
                return;

            string fullPath = xmlWriteObj.Parent;
            XmlDocument xmlTodoDoc = new XmlDocument();
            string todoPath = Path.Combine(fullPath, CommonXMLConstants.LastKnownStatePath);
            CommonMethods.CreateLastKnownStateFile(fullPath);
            CommonMethods.LoadXML(ref xmlTodoDoc, todoPath);
            AppendActionFileToDo(xmlTodoDoc, xmlWriteObj, CommonXMLConstants.ActionDeleted, deletedNode);
            CommonMethods.SaveXML(ref xmlTodoDoc, todoPath);
        }

        /// <summary>
        /// Given a XMLWriteFolderObject and an XmlNode , load the xml file and create a extra folder node
        /// in the todo file
        /// </summary>
        /// <param name="xmlWriteObj"></param>
        private static void GenerateFolderToDo(XMLWriteFolderObject xmlWriteObj)
        {
            string parentPath = xmlWriteObj.Parent;
            if (!Directory.Exists(parentPath))
                return;

            XmlDocument xmlTodoDoc = new XmlDocument();
            string todoPath = Path.Combine(parentPath, CommonXMLConstants.LastKnownStatePath);
            CommonMethods.CreateLastKnownStateFile(parentPath);
            CommonMethods.LoadXML(ref xmlTodoDoc, todoPath);
            AppendActionFolderToDo(xmlTodoDoc, xmlWriteObj, CommonXMLConstants.ActionDeleted);
            CommonMethods.SaveXML(ref xmlTodoDoc, todoPath);
        }

        /// <summary>
        /// Based on the XMLWriteFileObject , this method will extract the data and create an extra node in the
        /// todo file
        /// </summary>
        /// <param name="xmlDoc"> XmlDocument that is loaded with the xml file </param>
        /// <param name="xmlWriteObj"> The XMLWriteFileObject to be written in the todo file </param>
        /// <param name="changeType"> Deleted Changetype </param>
        /// <param name="node"> Node with the hash and last modified details </param>
        private static void AppendActionFileToDo(XmlDocument xmlDoc, XMLWriteFileObject xmlWriteObj, string changeType, XmlNode node)
        {
            string hash = string.Empty;
            string lastModified = string.Empty;
            XmlNodeList nodeList = node.ChildNodes;
            for (int i = 0; i < nodeList.Count; i++)
            {
                XmlNode childNode = nodeList[i];
                switch (childNode.Name)
                {
                    case CommonXMLConstants.NodeHash:
                        hash = childNode.InnerText;
                        break;
                    case CommonXMLConstants.NodeLastModified:
                        lastModified = childNode.InnerText;
                        break;
                }
            }

            XmlText hashText = xmlDoc.CreateTextNode(hash);
            XmlText actionText = xmlDoc.CreateTextNode(changeType);
            XmlText lastModifiedText = xmlDoc.CreateTextNode(lastModified);
            XmlText nameText = xmlDoc.CreateTextNode(xmlWriteObj.Name);
            XmlText lastUpdatedText = xmlDoc.CreateTextNode(xmlWriteObj.MetaUpdated.ToString());

            XmlElement fileElement = xmlDoc.CreateElement(CommonXMLConstants.NodeFile);
            XmlElement nameElement = xmlDoc.CreateElement(CommonXMLConstants.NodeName);
            XmlElement hashElement = xmlDoc.CreateElement(CommonXMLConstants.NodeHash);
            XmlElement actionElement = xmlDoc.CreateElement(CommonXMLConstants.NodeAction);
            XmlElement lastModifiedElement = xmlDoc.CreateElement(CommonXMLConstants.NodeLastModified);
            XmlElement lastUpdatedElement = xmlDoc.CreateElement(CommonXMLConstants.NodeLastUpdated);

            hashElement.AppendChild(hashText);
            actionElement.AppendChild(actionText);
            lastModifiedElement.AppendChild(lastModifiedText);
            lastUpdatedElement.AppendChild(lastUpdatedText);
            nameElement.AppendChild(nameText);

            fileElement.AppendChild(nameElement);
            fileElement.AppendChild(actionElement);
            fileElement.AppendChild(hashElement);
            fileElement.AppendChild(lastModifiedElement);
            fileElement.AppendChild(lastUpdatedElement);

            XmlNode rootNode = xmlDoc.SelectSingleNode(CommonXMLConstants.XPathLastKnownState);
            rootNode.AppendChild(fileElement);
        }

        /// <summary>
        /// Given an XMLWriteFileObject , it looks for the same name in the todo file and delete the node
        /// with the same name
        /// </summary>
        /// <param name="xmlWriteObj"></param>
        private static void DeleteFileToDoByName(XMLWriteFileObject xmlWriteObj)
        {
            string todoXmlPath = Path.Combine(xmlWriteObj.Parent, CommonXMLConstants.LastKnownStatePath);
            if (!File.Exists(todoXmlPath))
                return;

            XmlDocument todoXmlDoc = new XmlDocument();
            CommonMethods.LoadXML(ref todoXmlDoc, todoXmlPath);
            XmlNode fileNode = todoXmlDoc.SelectSingleNode(CommonXMLConstants.XPathLastKnownState + CommonXMLConstants.XPathFile + "[name=" + CommonMethods.ParseXPathString(xmlWriteObj.Name) + "]");
            if (fileNode != null)
                fileNode.ParentNode.RemoveChild(fileNode);
            CommonMethods.SaveXML(ref todoXmlDoc, todoXmlPath);
        }

        /// <summary>
        /// Based on the XMLWriteFolderObject , this method will extract the data and create an extra node in the
        /// todo file
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="folder"> XMLWriteFolderObject to be written to a todo file</param>
        /// <param name="changeType"> Deleted </param>
        private static void AppendActionFolderToDo(XmlDocument xmlDoc, XMLWriteFolderObject folder, string changeType)
        {
            XmlText nameText = xmlDoc.CreateTextNode(folder.Name);
            XmlText action = xmlDoc.CreateTextNode(changeType);
            XmlText lastUpdatedText = xmlDoc.CreateTextNode(folder.MetaUpdated.ToString());

            XmlElement folderElement = xmlDoc.CreateElement(CommonXMLConstants.NodeFolder);
            XmlElement nameElement = xmlDoc.CreateElement(CommonXMLConstants.NodeName);
            XmlElement actionElement = xmlDoc.CreateElement(CommonXMLConstants.NodeAction);
            XmlElement lastUpdatedElement = xmlDoc.CreateElement(CommonXMLConstants.NodeLastUpdated);

            nameElement.AppendChild(nameText);
            actionElement.AppendChild(action);
            lastUpdatedElement.AppendChild(lastUpdatedText);

            folderElement.AppendChild(nameElement);
            folderElement.AppendChild(actionElement);
            folderElement.AppendChild(lastUpdatedElement);
            XmlNode rootNode = xmlDoc.SelectSingleNode(CommonXMLConstants.XPathLastKnownState);
            rootNode.AppendChild(folderElement);
        }

        /// <summary>
        /// Given a XMLWriteFolderobject , it will search for the same name in the todo file and delete the
        /// node
        /// </summary>
        /// <param name="xmlWriteObj"></param>
        private static void DeleteFolderToDoByName(XMLWriteFolderObject xmlWriteObj)
        {
            string todoXmlPath = Path.Combine(xmlWriteObj.Parent, CommonXMLConstants.LastKnownStatePath);
            if (!File.Exists(todoXmlPath))
                return;

            XmlDocument todoXmlDoc = new XmlDocument();
            CommonMethods.LoadXML(ref todoXmlDoc, todoXmlPath);
            XmlNode folderNode = todoXmlDoc.SelectSingleNode(CommonXMLConstants.XPathLastKnownState + CommonXMLConstants.XPathFolder + "[name=" + CommonMethods.ParseXPathString(xmlWriteObj.Name) + "]");
            if (folderNode != null)
                folderNode.ParentNode.RemoveChild(folderNode);
            CommonMethods.SaveXML(ref todoXmlDoc, todoXmlPath);
        }

        #endregion

    }
}