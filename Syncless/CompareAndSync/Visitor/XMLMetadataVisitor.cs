using System.Collections.Generic;
using Syncless.CompareAndSync.CompareObject;
using System.IO;
using System.Xml;
using Syncless.CompareAndSync.Enum;

namespace Syncless.CompareAndSync.Visitor
{
    public class XMLMetadataVisitor : IVisitor
    {
        private const string MetaDir = ".syncless";
        private const string XmlName = @"\syncless.xml";
        private const string Metadatapath = MetaDir + @"\syncless.xml";
        private const string Todopath = @".syncless\todo.xml";
        private const string XpathExpr = "/meta-data";
        private const string NodeName = "name";
        private const string NodeSize = "size";
        private const string NodeHash = "hash";
        private const string NodeLastModified = "last_modified";
        private const string NodeLastCreated = "last_created";
        private const string NodeLastUpdated = "last_updated";
        private const string FILE = "file";
        private const string FOLDER = "folder";
        private const string Action = "action";
        private const string LastUpdated = "last_updated";
        private const string LastKnownState = "last_known_state";
        private static readonly object SyncLock = new object();

        #region IVisitor Members

        public void Visit(FileCompareObject file, int numOfPaths)
        {
            XmlDocument xmlDoc = new XmlDocument();
            for (int i = 0; i < numOfPaths; i++)
            {
                //if (currentPaths[i].Contains(MetaDir))
                //    continue;
                string path = Path.Combine(file.GetSmartParentPath(i), Metadatapath);
                if (!File.Exists(path))
                    continue;

                lock (SyncLock)
                {
                    CommonMethods.LoadXML(ref xmlDoc, path);
                }

                file = PopulateFileWithMetaData(xmlDoc, file, i);
                //xmlDoc.Save(path);                
            }
            xmlDoc = null;
            ProcessFileMetaData(file, numOfPaths);
            ProcessToDo(file, numOfPaths);
        }

        public void Visit(FolderCompareObject folder, int numOfPaths)
        {
            XmlDocument xmlDoc = new XmlDocument();

            PopulateFolderMetaName(folder, numOfPaths);

            for (int i = 0; i < numOfPaths; i++)
            {

                string path = Path.Combine(folder.GetSmartParentPath(i), Metadatapath);
                if (!File.Exists(path))
                    continue;

                lock (SyncLock)
                {
                    CommonMethods.LoadXML(ref xmlDoc, path);
                }

                folder = PopulateFolderWithMetaData(xmlDoc, folder, i);
            }
            ProcessFolderMetaData(folder, numOfPaths);
            ProcessToDo(folder, numOfPaths);

            DirectoryInfo dirInfo = null;
            List<XMLCompareObject> xmlObjList = new List<XMLCompareObject>();

            for (int i = 0; i < numOfPaths; i++)
            {
                string path = Path.Combine(folder.GetSmartParentPath(i), folder.Name);


                if (Directory.Exists(path))
                {
                    dirInfo = new DirectoryInfo(path);
                    FileInfo[] fileList = dirInfo.GetFiles();
                    DirectoryInfo[] dirInfoList = dirInfo.GetDirectories();
                    string xmlPath = Path.Combine(path, Metadatapath);
                    if (!File.Exists(xmlPath))
                        continue;

                    lock (SyncLock)
                    {
                        CommonMethods.LoadXML(ref xmlDoc, xmlPath);
                    }

                    xmlObjList = GetAllFilesInXML(xmlDoc);
                    List<string> xmlFolderList = GetAllFoldersInXML(xmlDoc);
                    RemoveSimilarFiles(xmlObjList, fileList);
                    RemoveSimilarFolders(xmlFolderList, dirInfoList);
                }


                AddFileToChild(xmlObjList, folder, i, numOfPaths);

                xmlObjList.Clear();
            }
        }

        public void Visit(RootCompareObject root)
        {
            XmlDocument xmlDoc = new XmlDocument();
            string[] listOfPaths = root.Paths;
            DirectoryInfo di = null;
            List<XMLCompareObject> xmlObjList = null;

            for (int i = 0; i < listOfPaths.Length; i++)
            {
                string xmlPath = Path.Combine(listOfPaths[i], Metadatapath);
                if (!File.Exists(xmlPath))
                    continue;

                lock (SyncLock)
                {
                    CommonMethods.LoadXML(ref xmlDoc, xmlPath);
                }

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
            }

        }

        #endregion

        #region Files

        private FileCompareObject PopulateFileWithMetaData(XmlDocument xmlDoc, FileCompareObject file, int counter)
        {
            XmlNode node = xmlDoc.SelectSingleNode(XpathExpr + "/" + FILE + "[name=" + CommonMethods.ParseXpathString(file.Name) + "]");
            if (node != null)
            {

                XmlNodeList childNodeList = node.ChildNodes;
                for (int i = 0; i < childNodeList.Count; i++)
                {
                    XmlNode childNode = childNodeList[i];
                    if (childNode.Name.Equals(NodeSize))
                    {
                        file.MetaLength[counter] = long.Parse(childNode.InnerText);
                    }
                    else if (childNode.Name.Equals(NodeHash))
                    {
                        file.MetaHash[counter] = childNode.InnerText;
                    }
                    else if (childNode.Name.Equals(NodeLastModified))
                    {
                        file.MetaLastWriteTime[counter] = long.Parse(childNode.InnerText);
                    }
                    else if (childNode.Name.Equals(NodeLastCreated))
                    {
                        file.MetaCreationTime[counter] = long.Parse(childNode.InnerText);
                    }
                    else if (childNode.Name.Equals(NodeLastUpdated))
                    {
                        file.MetaUpdated[counter] = long.Parse(childNode.InnerText);
                    }
                    
                }

                file.MetaExists[counter] = true;

            }else
            {
                string path = Path.Combine(file.GetSmartParentPath(counter),Todopath);
                if (File.Exists(path))
                {
                    XmlDocument todoXMLDoc = new XmlDocument();
                    CommonMethods.LoadXML(ref todoXMLDoc, path);
                    XmlNode todoNode = todoXMLDoc.SelectSingleNode("/" + LastKnownState + "/" + FILE + "[name=" + CommonMethods.ParseXpathString(file.Name) + "]");
                    if (todoNode != null)
                    {
                        XmlNodeList nodeList = todoNode.ChildNodes;
                        for (int i = 0; i < nodeList.Count; i++)
                        {
                            XmlNode childNode = nodeList[i];
                            switch (childNode.Name)
                            {
                                case Action:
                                    string action = childNode.InnerText;
                                    if (action.Equals("Deleted"))
                                    {
                                        file.ToDoAction[counter] = Enum.LastKnownState.Deleted;
                                    }
                                    else
                                    {
                                        file.ToDoAction[counter] = Enum.LastKnownState.Renamed;
                                    }
                                    break;
                                case NodeLastModified:
                                    file.MetaLastWriteTime[counter] = long.Parse(childNode.InnerText);
                                    break;
                                case NodeHash:
                                    file.MetaHash[counter] = childNode.InnerText;
                                    break;
                                case NodeLastUpdated:
                                    file.MetaUpdated[counter] = long.Parse(childNode.InnerText);
                                    break;
                            }
                        }
                    }
                }
            }

            return file;
        }

        private List<XMLCompareObject> GetAllFilesInXML(XmlDocument xmlDoc)
        {
            string hash = "";
            string name = "";
            long size = 0;
            long createdTime = 0;
            long modifiedTime = 0;
            long updatedTime = 0;

            List<XMLCompareObject> objectList = new List<XMLCompareObject>();
            XmlNodeList xmlNodeList = xmlDoc.SelectNodes(XpathExpr + "/" + FILE);
            if (xmlNodeList == null)
                return objectList;

            foreach (XmlNode nodes in xmlNodeList)
            {
                XmlNodeList list = nodes.ChildNodes;
                foreach (XmlNode node in list)
                {
                    switch (node.Name)
                    {
                        case NodeName:
                            name = node.InnerText;
                            break;
                        case NodeSize:
                            size = long.Parse(node.InnerText);
                            break;
                        case NodeHash:
                            hash = node.InnerText;
                            break;
                        case NodeLastCreated:
                            createdTime = long.Parse(node.InnerText);
                            break;
                        case NodeLastModified:
                            modifiedTime = long.Parse(node.InnerText);
                            break;
                        case NodeLastUpdated:
                            updatedTime = long.Parse(node.InnerText);
                            break;
                    }
                }
                objectList.Add(new XMLCompareObject(name, hash, size, createdTime, modifiedTime , updatedTime));
            }

            return objectList;
        }

        private List<string> GetAllFoldersInXML(XmlDocument xmlDoc)
        {
            List<string> folderList = new List<string>();

            XmlNodeList xmlFolderList = xmlDoc.SelectNodes(XpathExpr + "/" + FOLDER);
            if (xmlFolderList == null)
                return folderList;

            foreach (XmlNode folderNodes in xmlFolderList)
            {
                XmlNodeList folders = folderNodes.ChildNodes;
                foreach (XmlNode node in folders)
                {
                    switch (node.Name)
                    {
                        case NodeName:
                            folderList.Add(node.InnerText);
                            break;
                    }
                }
            }

            return folderList;
        }

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


        private void AddFileToChild(List<XMLCompareObject> xmlFileList, FolderCompareObject folder, int counter, int length)
        {
            for (int i = 0; i < xmlFileList.Count; i++)
            {
                BaseCompareObject o = folder.GetChild(xmlFileList[i].Name);
                FileCompareObject fco = null;

                if (o == null)
                    fco = new FileCompareObject(xmlFileList[i].Name, length, folder);
                else
                    fco = (FileCompareObject)o;

                fco.MetaCreationTime[counter] = xmlFileList[i].CreatedTime;
                fco.MetaHash[counter] = xmlFileList[i].Hash;
                fco.MetaLastWriteTime[counter] = xmlFileList[i].LastModifiedTime;
                fco.MetaLength[counter] = xmlFileList[i].Size;
                fco.MetaUpdated[counter] = xmlFileList[i].LastUpdatedTime;
                fco.MetaExists[counter] = true;

                if (o == null)
                    folder.AddChild(fco);
            }
        }

        private void AddFolderToChild(List<string> folderName, FolderCompareObject folder, int counter, int length)
        {
            for (int i = 0; i < folderName.Count; i++)
            {
                BaseCompareObject o = folder.GetChild(folderName[i]);
                FileCompareObject fco = null;

                if (o == null)
                    fco = new FileCompareObject(folderName[i], length, folder);
                else
                    fco = (FileCompareObject)o;

                fco.MetaExists[counter] = true;

                if (o == null)
                    folder.AddChild(fco);
            }
        }


        private void AddFileToRoot(List<XMLCompareObject> xmlFileList, RootCompareObject root, int counter, int length)
        {
            if (xmlFileList.Count == 0)
                return;

            for (int i = 0; i < xmlFileList.Count; i++)
            {
                BaseCompareObject o = root.GetChild(xmlFileList[i].Name);
                FileCompareObject fco = null;

                if (o == null)
                    fco = new FileCompareObject(xmlFileList[i].Name, length, root);
                else
                    fco = (FileCompareObject)o;

                fco.MetaCreationTime[counter] = xmlFileList[i].CreatedTime;
                fco.MetaHash[counter] = xmlFileList[i].Hash;
                fco.MetaLastWriteTime[counter] = xmlFileList[i].LastModifiedTime;
                fco.MetaLength[counter] = xmlFileList[i].Size;
                fco.MetaUpdated[counter] = xmlFileList[i].LastUpdatedTime;
                fco.MetaExists[counter] = true;

                if (o == null)
                    root.AddChild(fco);
            }
        }

        private void AddFolderToRoot(List<string> folderName, RootCompareObject root, int counter, int length)
        {
            if (folderName.Count == 0)
                return;

            for (int i = 0; i < folderName.Count; i++)
            {
                BaseCompareObject o = root.GetChild(folderName[i]);
                FolderCompareObject fco = null;

                if (o == null)
                    fco = new FolderCompareObject(folderName[i], length, root);
                else
                    fco = (FolderCompareObject)o;

                fco.MetaExists[counter] = true;

                if (o == null)
                    root.AddChild(fco);
            }
        }

        private void ProcessFileMetaData(FileCompareObject file, int numOfPaths)
        {
            for (int i = 0; i < numOfPaths; i++)
            {
                if (file.Exists[i] && !file.MetaExists[i])
                    file.ChangeType[i] = MetaChangeType.New; //Possible rename/move
                else if (!file.Exists[i] && file.MetaExists[i])
                    file.ChangeType[i] = MetaChangeType.Delete; //Possible rename/move
                else if (file.Exists[i] && file.MetaExists[i])
                {
                    if (file.Length[i] != file.MetaLength[i] || file.Hash[i] != file.MetaHash[i])
                        file.ChangeType[i] = MetaChangeType.Update;
                    else
                        file.ChangeType[i] = MetaChangeType.NoChange;
                }
                else
                    file.ChangeType[i] = null;
            }
        }


        private void ProcessToDo(BaseCompareObject bco, int numOfPaths)
        {
            for (int i = 0; i < numOfPaths; i++)
            {
                if (bco.ChangeType[i] == null && bco.ToDoAction[i].HasValue)
                {
                    if (bco.ToDoAction[i] == Enum.LastKnownState.Deleted)
                        bco.ChangeType[i] = MetaChangeType.Delete;
                }
            }
        }

        #endregion

        #region Folders

        private void PopulateFolderMetaName(FolderCompareObject folder, int numOfPaths)
        {
            for (int i = 0; i < numOfPaths; i++)
            {
                string currMetaData = Path.Combine(Path.Combine(folder.GetSmartParentPath(i), folder.Name), Metadatapath);
                if (File.Exists(currMetaData))
                {
                    XmlDocument xmlDoc = new XmlDocument();

                    lock (SyncLock)
                    {
                        CommonMethods.LoadXML(ref xmlDoc, currMetaData);
                    }

                    folder.MetaName = xmlDoc.SelectSingleNode(XpathExpr + "/name").InnerText;
                }
            }
        }

        private FolderCompareObject PopulateFolderWithMetaData(XmlDocument xmlDoc, FolderCompareObject folder, int counter)
        {
            XmlNode node = xmlDoc.SelectSingleNode(XpathExpr + "/"+ FOLDER + "[name=" + CommonMethods.ParseXpathString(folder.Name) + "]");
            if (node != null)
            {
                folder.MetaExists[counter] = true;
            }
            else
            {
                string path = Path.Combine(folder.GetSmartParentPath(counter), Todopath);
                if (File.Exists(path))
                {
                    XmlDocument todoXmlDoc = new XmlDocument();
                    CommonMethods.LoadXML(ref todoXmlDoc, path);
                    XmlNode todoNode = todoXmlDoc.SelectSingleNode("/" + LastKnownState + "/" + FOLDER + "[name=" + CommonMethods.ParseXpathString(folder.Name) + "]");
                    if (todoNode != null)
                    {
                        XmlNodeList nodeList = todoNode.ChildNodes;
                        for (int i = 0; i < nodeList.Count; i++)
                        {
                            XmlNode childNode = nodeList[i];
                            switch (childNode.Name)
                            {
                                case Action:
                                    string action = childNode.InnerText;
                                    if (action.Equals("Deleted"))
                                    {
                                        folder.ToDoAction[counter] = Enum.LastKnownState.Deleted;
                                    }
                                    else
                                    {
                                        folder.ToDoAction[counter] = Enum.LastKnownState.Renamed;
                                    }
                                    break;
                                case NodeLastUpdated:
                                    folder.MetaUpdated[counter] = long.Parse(childNode.InnerText);
                                    break;
                            }
                        }
                    }
                }
            }
            
            return folder;
        }

        private void ProcessFolderMetaData(FolderCompareObject folder, int numOfPaths)
        {
            for (int i = 0; i < numOfPaths; i++)
            {
                if (folder.Exists[i] && !folder.MetaExists[i])
                    folder.ChangeType[i] = MetaChangeType.New; //Possible rename/move
                else if (!folder.Exists[i] && folder.MetaExists[i])
                    folder.ChangeType[i] = MetaChangeType.Delete; //Possible rename/move
                else if (folder.Exists[i] && folder.MetaExists[i])
                    folder.ChangeType[i] = MetaChangeType.NoChange;
                else
                    folder.ChangeType[i] = null;
            }
        }

        #endregion

    }
}
