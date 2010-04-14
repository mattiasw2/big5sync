/*
 * 
 * Author: Gordon Hoi Chi Kit
 * 
 */

using System.Collections.Generic;
using System.IO;
using System.Xml;
using Syncless.CompareAndSync.Enum;
using Syncless.CompareAndSync.Manual.CompareObject;

namespace Syncless.CompareAndSync.Manual.Visitor
{   
    /// <summary>
    /// Based on the trees built by the BuilderVistor , XMLMetadataVistor will populate the data based on
    /// the meta data documents or the last known state
    /// </summary>
    public class XMLMetadataVisitor : IVisitor
    {

        #region IVisitor Members

        /// <summary>
        /// Visits each file node and return them and it will update the values either through the meta data
        /// or the last known state document
        /// </summary>
        /// <param name="file"></param>
        /// <param name="numOfPaths"></param>
        public void Visit(FileCompareObject file, int numOfPaths)
        {
            XmlDocument xmlDoc = new XmlDocument();

            for (int i = 0; i < numOfPaths; i++)
            {
                string path = Path.Combine(file.GetSmartParentPath(i), CommonXMLConstants.MetadataPath);

                if (!File.Exists(path))
                    continue;

                CommonMethods.LoadXML(ref xmlDoc, path);
                file = PopulateFileWithMetaData(xmlDoc, file, i);
            }
        }

        /// <summary>
        /// Visits each folder node and return them and it will update the values either through the meta data
        /// or the last known state document
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="numOfPaths"></param>
        public void Visit(FolderCompareObject folder, int numOfPaths)
        {
            XmlDocument xmlDoc = new XmlDocument();
            PopulateFolderMetaName(folder, numOfPaths);

            for (int i = 0; i < numOfPaths; i++)
            {
                string path = Path.Combine(folder.GetSmartParentPath(i), CommonXMLConstants.MetadataPath);

                if (!File.Exists(path))
                    continue;

                CommonMethods.LoadXML(ref xmlDoc, path);
                folder = PopulateFolderWithMetaData(xmlDoc, folder, i);
            }

            AddXmlNodes(folder, numOfPaths, xmlDoc);
        }

        //Appends the any xml file/folder nodes onto the folder
        private void AddXmlNodes(FolderCompareObject folder, int numOfPaths, XmlDocument xmlDoc)
        {
            List<XMLCompareObject> xmlObjList = new List<XMLCompareObject>();
            List<string> xmlFolderList = new List<string>();

            for (int i = 0; i < numOfPaths; i++)
            {
                string path = Path.Combine(folder.GetSmartParentPath(i), folder.Name);

                if (Directory.Exists(path))
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(path);
                    FileInfo[] fileList = dirInfo.GetFiles();
                    DirectoryInfo[] dirInfoList = dirInfo.GetDirectories();
                    string xmlPath = Path.Combine(path, CommonXMLConstants.MetadataPath);

                    if (!File.Exists(xmlPath))
                        continue;

                    CommonMethods.LoadXML(ref xmlDoc, xmlPath);
                    xmlObjList = GetAllFilesInXML(xmlDoc);
                    xmlFolderList = GetAllFoldersInXML(xmlDoc);
                    RemoveSimilarFiles(xmlObjList, fileList);
                    RemoveSimilarFolders(xmlFolderList, dirInfoList);
                }

                AddFileToChild(xmlObjList, folder, i, numOfPaths);
                AddFolderToChild(xmlFolderList, folder, i, numOfPaths);
                xmlObjList.Clear();
                xmlFolderList.Clear();
            }
        }

        /// <summary>
        /// Extracts metadata files and folders and minus them off existing files and folders.
        /// </summary>
        /// <param name="root"></param>
        public void Visit(RootCompareObject root)
        {
            XmlDocument xmlDoc = new XmlDocument();
            string[] listOfPaths = root.Paths;
            DirectoryInfo di;
            List<XMLCompareObject> xmlObjList;
            List<string> name = new List<string>();

            for (int i = 0; i < listOfPaths.Length; i++)
            {
                string xmlPath = Path.Combine(listOfPaths[i], CommonXMLConstants.MetadataPath);
                if (!File.Exists(xmlPath))
                    continue;

                CommonMethods.LoadXML(ref xmlDoc, xmlPath);
                di = new DirectoryInfo(listOfPaths[i]);
                FileInfo[] fileInfoList = di.GetFiles();
                DirectoryInfo[] dirInfoList = di.GetDirectories();
                List<string> folderNames = GetAllFoldersInXML(xmlDoc);
                xmlObjList = GetAllFilesInXML(xmlDoc);
                RemoveSimilarFiles(xmlObjList, fileInfoList);
                RemoveSimilarFolders(folderNames, dirInfoList);
                AddFolderToRoot(folderNames, root, i, root.Paths.Length);
                AddFileToRoot(xmlObjList, root, i, root.Paths.Length);
                xmlObjList.Clear();
                folderNames.Clear();
            }

        }

        #endregion

        #region Files

        //Populates the file from meta data. If it does not exist, try to look it up from the last known state
        //instead
        private FileCompareObject PopulateFileWithMetaData(XmlDocument xmlDoc, FileCompareObject file, int counter)
        {
            XmlNode node = xmlDoc.SelectSingleNode(CommonXMLConstants.XPathExpr + CommonXMLConstants.XPathFile + "[name=" + CommonMethods.ParseXPathString(file.Name) + "]");
            if (node != null)
            {

                PopulateFromMetaData(file, node, counter);
            }
            else
            {
                PopulateFromLastKnownState(file, counter);
            }

            return file;
        }

        // Populate the file's details from the last known state
        private void PopulateFromLastKnownState(FileCompareObject file, int counter)
        {
            string path = Path.Combine(file.GetSmartParentPath(counter), CommonXMLConstants.LastKnownStatePath);

            if (File.Exists(path))
            {
                XmlDocument lastKnownXMLDoc = new XmlDocument();
                CommonMethods.LoadXML(ref lastKnownXMLDoc, path);
                XmlNode fileNode = lastKnownXMLDoc.SelectSingleNode(CommonXMLConstants.XPathLastKnownState + CommonXMLConstants.XPathFile + "[name=" + CommonMethods.ParseXPathString(file.Name) + "]");

                if (fileNode != null)
                {
                    XmlNodeList nodeList = fileNode.ChildNodes;
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        XmlNode childNode = nodeList[i];
                        switch (childNode.Name)
                        {
                            case CommonXMLConstants.NodeAction:
                                string action = childNode.InnerText;
                                file.LastKnownState[counter] = action.Equals(CommonXMLConstants.ActionDeleted) ? LastKnownState.Deleted : LastKnownState.Renamed;
                                break;
                            case CommonXMLConstants.NodeLastModifiedUtc:
                                file.MetaLastWriteTimeUtc[counter] = long.Parse(childNode.InnerText);
                                break;
                            case CommonXMLConstants.NodeHash:
                                file.MetaHash[counter] = childNode.InnerText;
                                break;
                            case CommonXMLConstants.NodeLastUpdatedUtc:
                                file.MetaUpdated[counter] = long.Parse(childNode.InnerText);
                                break;
                        }
                    }
                }
            }
        }

        // Loads the xml document , and populate the file's data from the meta data
        private void PopulateFromMetaData(FileCompareObject file, XmlNode node, int counter)
        {
            XmlNodeList childNodeList = node.ChildNodes;
            for (int i = 0; i < childNodeList.Count; i++)
            {
                XmlNode childNode = childNodeList[i];

                switch (childNode.Name)
                {
                    case CommonXMLConstants.NodeSize:
                        file.MetaLength[counter] = long.Parse(childNode.InnerText);
                        break;
                    case CommonXMLConstants.NodeHash:
                        file.MetaHash[counter] = childNode.InnerText;
                        break;
                    case CommonXMLConstants.NodeLastModifiedUtc:
                        file.MetaLastWriteTimeUtc[counter] = long.Parse(childNode.InnerText);
                        break;
                    case CommonXMLConstants.NodeLastCreatedUtc:
                        file.MetaCreationTimeUtc[counter] = long.Parse(childNode.InnerText);
                        break;
                    case CommonXMLConstants.NodeLastUpdatedUtc:
                        file.MetaUpdated[counter] = long.Parse(childNode.InnerText);
                        break;
                }
            }

            file.MetaExists[counter] = true;
        }

        // Gets all the file nodes in meta data and return them as a list of XMLCompareObject
        private List<XMLCompareObject> GetAllFilesInXML(XmlDocument xmlDoc)
        {
            string hash = "";
            string name = "";
            long size = 0;
            long createdTimeUtc = 0;
            long modifiedTimeUtc = 0;
            long updatedTimeUtc = 0;

            List<XMLCompareObject> objectList = new List<XMLCompareObject>();
            XmlNodeList xmlNodeList = xmlDoc.SelectNodes(CommonXMLConstants.XPathExpr + CommonXMLConstants.XPathFile);

            if (xmlNodeList == null)
                return objectList;

            foreach (XmlNode nodes in xmlNodeList)
            {
                XmlNodeList list = nodes.ChildNodes;
                foreach (XmlNode node in list)
                {
                    switch (node.Name)
                    {
                        case CommonXMLConstants.NodeName:
                            name = node.InnerText;
                            break;
                        case CommonXMLConstants.NodeSize:
                            size = long.Parse(node.InnerText);
                            break;
                        case CommonXMLConstants.NodeHash:
                            hash = node.InnerText;
                            break;
                        case CommonXMLConstants.NodeLastCreatedUtc:
                            createdTimeUtc = long.Parse(node.InnerText);
                            break;
                        case CommonXMLConstants.NodeLastModifiedUtc:
                            modifiedTimeUtc = long.Parse(node.InnerText);
                            break;
                        case CommonXMLConstants.NodeLastUpdatedUtc:
                            updatedTimeUtc = long.Parse(node.InnerText);
                            break;
                    }
                }
                objectList.Add(new XMLCompareObject(name, hash, size, createdTimeUtc, modifiedTimeUtc, updatedTimeUtc));
            }

            return objectList;
        }

        //Extracts all folder nodes in the meta data and return them as a string of folder names
        private List<string> GetAllFoldersInXML(XmlDocument xmlDoc)
        {
            List<string> folderList = new List<string>();

            XmlNodeList xmlFolderList = xmlDoc.SelectNodes(CommonXMLConstants.XPathExpr + CommonXMLConstants.XPathFolder);
            if (xmlFolderList == null)
                return folderList;

            foreach (XmlNode folderNodes in xmlFolderList)
            {
                XmlNodeList folders = folderNodes.ChildNodes;
                foreach (XmlNode node in folders)
                {
                    switch (node.Name)
                    {
                        case CommonXMLConstants.NodeName:
                            folderList.Add(node.InnerText);
                            break;
                    }
                }
            }

            return folderList;
        }

        // Based on the list of XMLCompareObjects and FileInfo objects, remove any similar files by comparing
        // against each other by their names.
        private void RemoveSimilarFiles(List<XMLCompareObject> xmlObjList, FileInfo[] fileList)
        {
            if (xmlObjList.Count == 0)
                return;

            for (int i = 0; i < fileList.Length; i++)
            {
                for (int j = 0; j < xmlObjList.Count; j++)
                {
                    FileInfo fileInfo = fileList[i];
                    string name = xmlObjList[j].Name;

                    if (name.Equals(fileInfo.Name))
                        xmlObjList.RemoveAt(j);
                }
            }
        }

        // Based on the list of folder names and DirectoryInfo objects, remove any similar folders by comparing
        // against each other by their names.
        private void RemoveSimilarFolders(List<string> folderNameList, DirectoryInfo[] dirList)
        {
            if (folderNameList.Count == 0)
                return;

            for (int i = 0; i < dirList.Length; i++)
            {
                for (int j = 0; j < folderNameList.Count; j++)
                {
                    DirectoryInfo dirInfo = dirList[i];
                    string folderName = folderNameList[j];

                    if (dirInfo.Name.Equals(folderName))
                        folderNameList.RemoveAt(j);
                }
            }
        }

        // For any XMLCompareObjects in the list , create them as a node and append them to the folder node
        private void AddFileToChild(List<XMLCompareObject> xmlFileList, FolderCompareObject folder, int counter, int length)
        {
            for (int i = 0; i < xmlFileList.Count; i++)
            {
                BaseCompareObject o = folder.GetChild(xmlFileList[i].Name);
                FileCompareObject fco;

                if (o == null)
                    fco = new FileCompareObject(xmlFileList[i].Name, length, folder);
                else
                    fco = (FileCompareObject)o;

                fco.MetaCreationTimeUtc[counter] = xmlFileList[i].CreatedTimeUtc;
                fco.MetaHash[counter] = xmlFileList[i].Hash;
                fco.MetaLastWriteTimeUtc[counter] = xmlFileList[i].LastModifiedTimeUtc;
                fco.MetaLength[counter] = xmlFileList[i].Size;
                fco.MetaUpdated[counter] = xmlFileList[i].LastUpdatedTimeUtc;
                fco.MetaExists[counter] = true;

                if (o == null)
                    folder.AddChild(fco);
            }
        }

        // For any folder name in the list , create them as a node and append them to the folder node
        private void AddFolderToChild(List<string> folderName, FolderCompareObject folder, int counter, int length)
        {
            for (int i = 0; i < folderName.Count; i++)
            {
                BaseCompareObject o = folder.GetChild(folderName[i]);
                FolderCompareObject fco;

                if (o == null)
                    fco = new FolderCompareObject(folderName[i], length, folder);
                else
                    fco = (FolderCompareObject)o;

                fco.MetaExists[counter] = true;

                if (o == null)
                    folder.AddChild(fco);
            }
        }

        // For each XMLCompareObjects in the list , create a node and append them to the root node
        private void AddFileToRoot(List<XMLCompareObject> xmlFileList, RootCompareObject root, int counter, int length)
        {
            if (xmlFileList.Count == 0)
                return;

            for (int i = 0; i < xmlFileList.Count; i++)
            {
                BaseCompareObject o = root.GetChild(xmlFileList[i].Name);
                FileCompareObject fco;

                if (o == null)
                    fco = new FileCompareObject(xmlFileList[i].Name, length, root);
                else
                    fco = (FileCompareObject)o;

                fco.MetaCreationTimeUtc[counter] = xmlFileList[i].CreatedTimeUtc;
                fco.MetaHash[counter] = xmlFileList[i].Hash;
                fco.MetaLastWriteTimeUtc[counter] = xmlFileList[i].LastModifiedTimeUtc;
                fco.MetaLength[counter] = xmlFileList[i].Size;
                fco.MetaUpdated[counter] = xmlFileList[i].LastUpdatedTimeUtc;
                fco.MetaExists[counter] = true;

                if (o == null)
                    root.AddChild(fco);
            }
        }

        // For each list of folder names , create a node and append them to the root node
        private void AddFolderToRoot(List<string> folderName, RootCompareObject root, int counter, int length)
        {
            if (folderName.Count == 0)
                return;

            for (int i = 0; i < folderName.Count; i++)
            {
                BaseCompareObject o = root.GetChild(folderName[i]);
                FolderCompareObject fco;

                if (o == null)
                    fco = new FolderCompareObject(folderName[i], length, root);
                else
                    fco = (FolderCompareObject)o;

                fco.MetaExists[counter] = true;

                if (o == null)
                    root.AddChild(fco);
            }
        }

        #endregion

        #region Folders

        // Populate the folder's meta name if it exists
        private void PopulateFolderMetaName(FolderCompareObject folder, int numOfPaths)
        {
            for (int i = 0; i < numOfPaths; i++)
            {
                string currMetaData = Path.Combine(Path.Combine(folder.GetSmartParentPath(i), folder.Name), CommonXMLConstants.MetadataPath);

                if (File.Exists(currMetaData))
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    CommonMethods.LoadXML(ref xmlDoc, currMetaData);
                    folder.MetaName = xmlDoc.SelectSingleNode(CommonXMLConstants.XPathExpr + "/name").InnerText;
                }
            }
        }

        // Checks the meta data by the folder name , if it exists , set it MetaExists to true.
        // If not , look up the last known state and populate the values from that document
        private FolderCompareObject PopulateFolderWithMetaData(XmlDocument xmlDoc, FolderCompareObject folder, int counter)
        {
            XmlNode node = xmlDoc.SelectSingleNode(CommonXMLConstants.XPathExpr + CommonXMLConstants.XPathFolder + "[name=" + CommonMethods.ParseXPathString(folder.Name) + "]");
            if (node != null)
            {
                folder.MetaExists[counter] = true;
            }
            else
            {
                string path = Path.Combine(folder.GetSmartParentPath(counter), CommonXMLConstants.LastKnownStatePath);

                if (File.Exists(path))
                {
                    XmlDocument lastKnownXmlDoc = new XmlDocument();
                    CommonMethods.LoadXML(ref lastKnownXmlDoc, path);
                    XmlNode folderNode = lastKnownXmlDoc.SelectSingleNode(CommonXMLConstants.XPathLastKnownState + CommonXMLConstants.XPathFolder + "[name=" + CommonMethods.ParseXPathString(folder.Name) + "]");

                    if (folderNode != null)
                    {
                        XmlNodeList nodeList = folderNode.ChildNodes;
                        for (int i = 0; i < nodeList.Count; i++)
                        {
                            XmlNode childNode = nodeList[i];

                            switch (childNode.Name)
                            {
                                case CommonXMLConstants.NodeAction:
                                    string action = childNode.InnerText;
                                    if (action.Equals("deleted"))
                                        folder.LastKnownState[counter] = LastKnownState.Deleted;
                                    else
                                        folder.LastKnownState[counter] = LastKnownState.Renamed;
                                    break;
                                case CommonXMLConstants.NodeLastUpdatedUtc:
                                    folder.MetaUpdated[counter] = long.Parse(childNode.InnerText);
                                    break;
                            }
                        }
                    }
                }
            }

            return folder;
        }

        #endregion
    }
}