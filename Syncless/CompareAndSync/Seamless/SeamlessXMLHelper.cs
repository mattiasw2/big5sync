using System.IO;
using System.Xml;
using Syncless.CompareAndSync.Enum;
using Syncless.CompareAndSync.Seamless.XMLWriteObject;

namespace Syncless.CompareAndSync.Seamless
{
    /// <summary>
    /// Contains the necessary methods for writing to XML during a seamless/auto synchronization.
    /// </summary>
    public class SeamlessXMLHelper
    {

        #region Main Method

        /// <summary>
        /// This method will differentiate between an XMLWriteFolderObject or an XMLWriteFileObject. Then 
        /// base on it's MetaChangeType , it will update the xml document accordingly
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

        // Handle the different MetaChangeType of the XMLWriteFileObject
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

        //Create a new file node in the xml document
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
            XmlText createdTimeText = xmlDoc.CreateTextNode(xmlWriteObj.CreationTimeUtc.ToString());
            XmlText lastModifiedText = xmlDoc.CreateTextNode(xmlWriteObj.LastModifiedUtc.ToString());
            XmlText lastUpdatedText = xmlDoc.CreateTextNode(xmlWriteObj.MetaUpdatedUtc.ToString());

            XmlElement nameElement = xmlDoc.CreateElement(CommonXMLConstants.NodeName);
            XmlElement hashElement = xmlDoc.CreateElement(CommonXMLConstants.NodeHash);
            XmlElement sizeElement = xmlDoc.CreateElement(CommonXMLConstants.NodeSize);
            XmlElement createdTimeElement = xmlDoc.CreateElement(CommonXMLConstants.NodeLastCreatedUtc);
            XmlElement lastModifiedElement = xmlDoc.CreateElement(CommonXMLConstants.NodeLastModifiedUtc);
            XmlElement lastUpdatedElement = xmlDoc.CreateElement(CommonXMLConstants.NodeLastUpdatedUtc);
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
            DeleteFileLastKnownState(xmlWriteObj);
        }

        //Update the existing file node in the xml document 
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
                    case CommonXMLConstants.NodeLastModifiedUtc:
                        nodes.InnerText = xmlWriteObj.LastModifiedUtc.ToString();
                        break;
                    case CommonXMLConstants.NodeLastCreatedUtc:
                        nodes.InnerText = xmlWriteObj.CreationTimeUtc.ToString();
                        break;
                    case CommonXMLConstants.NodeLastUpdatedUtc:
                        nodes.InnerText = xmlWriteObj.MetaUpdatedUtc.ToString();
                        break;
                }
            }

            CommonMethods.SaveXML(ref xmlDoc, xmlFilePath);
            DeleteFileLastKnownState(xmlWriteObj);
        }

        //Rename the existing file node in the xml document
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
            node.LastChild.InnerText = xmlWriteObj.MetaUpdatedUtc.ToString();
            CommonMethods.SaveXML(ref xmlDoc, xmlFilePath);
            GenerateFileLastKnownState(xmlWriteObj, tempNode);
        }

        // Delete the file node in the xml document
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

            GenerateFileLastKnownState(xmlWriteObj, tempNode);
        }

        #endregion

        #region Folder Operations

        // Handles the different folder MetaChangeType
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

        // Create a new folder node in the xml document
        private static void CreateFolder(XMLWriteFolderObject xmlWriteObj)
        {
            XmlDocument xmlDoc = new XmlDocument();
            string xmlFilePath = Path.Combine(xmlWriteObj.Parent, CommonXMLConstants.MetadataPath);
            CommonMethods.CreateFileIfNotExist(xmlWriteObj.Parent);
            CommonMethods.LoadXML(ref xmlDoc, xmlFilePath);

            CommonMethods.DoFolderCleanUp(xmlDoc, xmlWriteObj.Name);
            XmlText nameText = xmlDoc.CreateTextNode(xmlWriteObj.Name);
            XmlText lastUpdatedText = xmlDoc.CreateTextNode(xmlWriteObj.MetaUpdatedUtc.ToString());

            XmlElement nameElement = xmlDoc.CreateElement(CommonXMLConstants.NodeName);
            XmlElement lastUpdatedElement = xmlDoc.CreateElement(CommonXMLConstants.NodeLastUpdatedUtc);
            XmlElement folder = xmlDoc.CreateElement(CommonXMLConstants.NodeFolder);
            nameElement.AppendChild(nameText);
            lastUpdatedElement.AppendChild(lastUpdatedText);
            folder.AppendChild(nameElement);
            folder.AppendChild(lastUpdatedElement);

            XmlNode rootNode = xmlDoc.SelectSingleNode(CommonXMLConstants.XPathExpr);
            rootNode.AppendChild(folder);
            CommonMethods.SaveXML(ref xmlDoc, xmlFilePath);
            DeleteFolderLastKnownState(xmlWriteObj);
        }

        // Rename the folder node in the xml document
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
                node.LastChild.InnerText = xmlWriteObj.MetaUpdatedUtc.ToString();
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
                GenerateFolderLastKnownState(xmlWriteObj);
            }
        }

        // Delete the folder node in the existing xml document
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

            GenerateFolderLastKnownState(xmlWriteObj);
        }

        #endregion

        #region LastKnownState Operations

        // Generates a new file node in the last known state file based on the XMLWriteFileObject and deleted node
        private static void GenerateFileLastKnownState(XMLWriteFileObject xmlWriteObj, XmlNode deletedNode)
        {
            if (deletedNode == null)
                return;

            string fullPath = xmlWriteObj.Parent;
            XmlDocument xmlTodoDoc = new XmlDocument();
            string todoPath = Path.Combine(fullPath, CommonXMLConstants.LastKnownStatePath);
            CommonMethods.CreateLastKnownStateFile(fullPath);
            CommonMethods.LoadXML(ref xmlTodoDoc, todoPath);
            AppendActionFileLastKnownState(xmlTodoDoc, xmlWriteObj, CommonXMLConstants.ActionDeleted, deletedNode);
            CommonMethods.SaveXML(ref xmlTodoDoc, todoPath);
        }

        // Generates a new folder node in the last known state file based on the XMLWriteFolderObject
        private static void GenerateFolderLastKnownState(XMLWriteFolderObject xmlWriteObj)
        {
            string parentPath = xmlWriteObj.Parent;
            if (!Directory.Exists(parentPath))
                return;

            XmlDocument xmlTodoDoc = new XmlDocument();
            string todoPath = Path.Combine(parentPath, CommonXMLConstants.LastKnownStatePath);
            CommonMethods.CreateLastKnownStateFile(parentPath);
            CommonMethods.LoadXML(ref xmlTodoDoc, todoPath);
            AppendActionFolderLastKnownState(xmlTodoDoc, xmlWriteObj, CommonXMLConstants.ActionDeleted);
            CommonMethods.SaveXML(ref xmlTodoDoc, todoPath);
        }

        // Called by method GenerateFileLastKnownState to access the xml document and add a new file node in the last
        // known state file
        private static void AppendActionFileLastKnownState(XmlDocument xmlDoc, XMLWriteFileObject xmlWriteObj, string changeType, XmlNode node)
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
                    case CommonXMLConstants.NodeLastModifiedUtc:
                        lastModified = childNode.InnerText;
                        break;
                }
            }

            XmlText hashText = xmlDoc.CreateTextNode(hash);
            XmlText actionText = xmlDoc.CreateTextNode(changeType);
            XmlText lastModifiedText = xmlDoc.CreateTextNode(lastModified);
            XmlText nameText = xmlDoc.CreateTextNode(xmlWriteObj.Name);
            XmlText lastUpdatedText = xmlDoc.CreateTextNode(xmlWriteObj.MetaUpdatedUtc.ToString());

            XmlElement fileElement = xmlDoc.CreateElement(CommonXMLConstants.NodeFile);
            XmlElement nameElement = xmlDoc.CreateElement(CommonXMLConstants.NodeName);
            XmlElement hashElement = xmlDoc.CreateElement(CommonXMLConstants.NodeHash);
            XmlElement actionElement = xmlDoc.CreateElement(CommonXMLConstants.NodeAction);
            XmlElement lastModifiedElement = xmlDoc.CreateElement(CommonXMLConstants.NodeLastModifiedUtc);
            XmlElement lastUpdatedElement = xmlDoc.CreateElement(CommonXMLConstants.NodeLastUpdatedUtc);

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

        //Deletes a file node in the last known state file by searching for the name
        private static void DeleteFileLastKnownState(XMLWriteFileObject xmlWriteObj)
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

        // Called by GenerateFolderLastKnownState to add a new folder node into the last known state file
        private static void AppendActionFolderLastKnownState(XmlDocument xmlDoc, XMLWriteFolderObject folder, string changeType)
        {
            XmlText nameText = xmlDoc.CreateTextNode(folder.Name);
            XmlText action = xmlDoc.CreateTextNode(changeType);
            XmlText lastUpdatedText = xmlDoc.CreateTextNode(folder.MetaUpdatedUtc.ToString());

            XmlElement folderElement = xmlDoc.CreateElement(CommonXMLConstants.NodeFolder);
            XmlElement nameElement = xmlDoc.CreateElement(CommonXMLConstants.NodeName);
            XmlElement actionElement = xmlDoc.CreateElement(CommonXMLConstants.NodeAction);
            XmlElement lastUpdatedElement = xmlDoc.CreateElement(CommonXMLConstants.NodeLastUpdatedUtc);

            nameElement.AppendChild(nameText);
            actionElement.AppendChild(action);
            lastUpdatedElement.AppendChild(lastUpdatedText);

            folderElement.AppendChild(nameElement);
            folderElement.AppendChild(actionElement);
            folderElement.AppendChild(lastUpdatedElement);
            XmlNode rootNode = xmlDoc.SelectSingleNode(CommonXMLConstants.XPathLastKnownState);
            rootNode.AppendChild(folderElement);
        }

        // Deletes a folder node in the last known state document by searching for its name
        private static void DeleteFolderLastKnownState(XMLWriteFolderObject xmlWriteObj)
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