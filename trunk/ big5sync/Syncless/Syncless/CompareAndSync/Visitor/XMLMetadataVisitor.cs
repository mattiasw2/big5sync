using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.CompareAndSync.CompareObject;
using System.IO;
using System.Xml;
using Syncless.CompareAndSync.Enum;
using System.Diagnostics;

namespace Syncless.CompareAndSync.Visitor
{
    public class XMLMetadataVisitor : IVisitor
    {
        private const string META_DIR = ".syncless";
        private const string XML_NAME = @"\syncless.xml";
        private const string METADATAPATH = META_DIR + @"\syncless.xml";
        private const string XPATH_EXPR = "/meta-data";
        private const string NODE_NAME = "name";
        private const string NODE_SIZE = "size";
        private const string NODE_HASH = "hash";
        private const string NODE_LAST_MODIFIED = "last_modified";
        private const string NODE_LAST_CREATED = "last_created";
        private const string FILE = "file";
        private const string FOLDER = "folder";
        private const string NODE_TODO = "todo";
        private const string NODE_DATE = "date";
        private const string NODE_ACTION = "action";
        private const string NODE_NEWNAME = "new_name";
        private static readonly object syncLock = new object();

        #region IVisitor Members

        public void Visit(FileCompareObject file, int numOfPaths)
        {
            XmlDocument xmlDoc = new XmlDocument();
            for (int i = 0; i < numOfPaths; i++)
            {
                //if (currentPaths[i].Contains(META_DIR))
                //    continue;
                string path = Path.Combine(file.GetSmartParentPath(i), METADATAPATH);
                if (!File.Exists(path))
                    continue;

                lock (syncLock)
                {
                    CommonMethods.LoadXML(ref xmlDoc, path);
                }

                file = PopulateFileWithMetaData(xmlDoc, file, i);
                //xmlDoc.Save(path);                
            }
            xmlDoc = null;
            ProcessFileMetaData(file,numOfPaths);
        }

        public void Visit(FolderCompareObject folder, int numOfPaths)
        {
            XmlDocument xmlDoc = new XmlDocument();

            PopulateFolderMetaName(folder, numOfPaths);

            for (int i = 0; i < numOfPaths; i++)
            {
                
                string path = Path.Combine(folder.GetSmartParentPath(i), METADATAPATH);
                if (!File.Exists(path))
                    continue;

                lock (syncLock)
                {
                    CommonMethods.LoadXML(ref xmlDoc, path);
                }

                folder = PopulateFolderWithMetaData(xmlDoc, folder, i);              
            }
            ProcessFolderMetaData(folder,numOfPaths);

            DirectoryInfo dirInfo = null;
            FileInfo[] fileList = null;
            DirectoryInfo[] dirInfoList = null;
            List<XMLCompareObject> xmlObjList = new List<XMLCompareObject>();
            List<string> xmlFolderList = null;
            string xmlPath = "";

            for (int i = 0; i < numOfPaths; i++)
            {
                string path = Path.Combine(folder.GetSmartParentPath(i), folder.Name);


                if (Directory.Exists(path))
                {
                    dirInfo = new DirectoryInfo(path);
                    fileList = dirInfo.GetFiles();
                    dirInfoList = dirInfo.GetDirectories();
                    xmlPath = Path.Combine(path , METADATAPATH);
                    if (!File.Exists(xmlPath))
                        continue;

                    lock (syncLock)
                    {
                        CommonMethods.LoadXML(ref xmlDoc, xmlPath);
                    }

                    xmlObjList = GetAllFilesInXML(xmlDoc);
                    xmlFolderList = GetAllFoldersInXML(xmlDoc);
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
            string xmlPath = "";
            string[] listOfPaths = root.Paths;
            DirectoryInfo di = null;
            List<XMLCompareObject> xmlObjList = null;
            List<string> folderNames = null;

            FileInfo[] fileInfoList = null;
            DirectoryInfo[] dirInfoList = null;

            for (int i = 0; i < listOfPaths.Length; i++)
            {
                xmlPath = Path.Combine(listOfPaths[i], METADATAPATH);
                if (!File.Exists(xmlPath))
                    continue;

                lock (syncLock)
                {
                    CommonMethods.LoadXML(ref xmlDoc, xmlPath);
                }

                di = new DirectoryInfo(listOfPaths[i]);
                fileInfoList = di.GetFiles();
                dirInfoList = di.GetDirectories();

                folderNames = GetAllFoldersInXML(xmlDoc);
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
            XmlNode node = xmlDoc.SelectSingleNode(XPATH_EXPR + "/" + FILE + "[name=" + CommonMethods.ParseXpathString(file.Name) + "]");
            if (node == null)
                return file;

            XmlNodeList childNodeList = node.ChildNodes;
            for (int i = 0; i < childNodeList.Count; i++)
            {
                XmlNode childNode = childNodeList[i];
                if (childNode.Name.Equals(NODE_SIZE))
                {
                    file.MetaLength[counter] = long.Parse(childNode.InnerText);
                }
                else if (childNode.Name.Equals(NODE_HASH))
                {
                    file.MetaHash[counter] = childNode.InnerText;
                }
                else if (childNode.Name.Equals(NODE_LAST_MODIFIED))
                {
                    file.MetaLastWriteTime[counter] = long.Parse(childNode.InnerText);
                }
                else if (childNode.Name.Equals(NODE_LAST_CREATED))
                {
                    file.MetaCreationTime[counter] = long.Parse(childNode.InnerText);
                }
            }

            XmlNode todoNode = xmlDoc.SelectSingleNode(XPATH_EXPR + "/" + FILE + "[name=" + CommonMethods.ParseXpathString(file.Name) + "]/todo");
            if (todoNode != null)
            {
                XmlNodeList nodeList = todoNode.ChildNodes;
                for (int i = 0; i < nodeList.Count; i++)
                {
                    XmlNode childNode = nodeList[i];
                    switch (childNode.Name)
                    {
                        case NODE_ACTION :
                            string action = childNode.InnerText;
                            if (action.Equals("Delete"))
                            {
                                file.Todo[counter] = ToDo.Delete;
                            }
                            else
                            {
                                file.Todo[counter] = ToDo.Rename;
                            }
                            break;

                        case NODE_DATE :
                            long date = long.Parse(childNode.InnerText);
                            file.TodoTimestamp[counter] = date;
                            break;
                        
                        case NODE_NEWNAME :
                            string newName = childNode.InnerText;
                            file.TodoNewName[counter] = newName;
                            break;
                    }
                }
            }
            
            file.MetaExists[counter] = true;
            return file;

        }

        private List<XMLCompareObject> GetAllFilesInXML(XmlDocument xmlDoc)
        {
            string hash = "";
            string name = "";
            long size = 0;
            long createdTime = 0;
            long modifiedTime = 0;

            List<XMLCompareObject> objectList = new List<XMLCompareObject>();
            XmlNodeList xmlNodeList = xmlDoc.SelectNodes(XPATH_EXPR + "/file");
            if (xmlNodeList == null)
                return objectList;

            foreach (XmlNode nodes in xmlNodeList)
            {
                XmlNodeList list = nodes.ChildNodes;
                foreach (XmlNode node in list)
                {
                    switch (node.Name)
                    {
                        case NODE_NAME:
                            name = node.InnerText;
                            break;
                        case NODE_SIZE:
                            size = long.Parse(node.InnerText);
                            break;
                        case NODE_HASH:
                            hash = node.InnerText;
                            break;
                        case NODE_LAST_CREATED:
                            createdTime = long.Parse(node.InnerText);
                            break;
                        case NODE_LAST_MODIFIED:
                            modifiedTime = long.Parse(node.InnerText);
                            break;
                    }
                }
                objectList.Add(new XMLCompareObject(name, hash, size, createdTime, modifiedTime));
            }

            return objectList;
        }

        private List<string> GetAllFoldersInXML(XmlDocument xmlDoc)
        {
            List<string> folderList = new List<string>();

            XmlNodeList xmlFolderList = xmlDoc.SelectNodes(XPATH_EXPR + "/" + FOLDER);
            if (xmlFolderList == null)
                return folderList;

            foreach (XmlNode folderNodes in xmlFolderList)
            {
                XmlNodeList folders = folderNodes.ChildNodes;
                foreach (XmlNode node in folders)
                {
                    switch (node.Name)
                    {
                        case NODE_NAME:
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

        #endregion

        #region Folders

        private void PopulateFolderMetaName(FolderCompareObject folder, int numOfPaths)
        {
            for (int i = 0; i < numOfPaths ; i++)
            {
                string currMetaData = Path.Combine(Path.Combine(folder.GetSmartParentPath(i), folder.Name), METADATAPATH);
                if (File.Exists(currMetaData))
                {
                    XmlDocument xmlDoc = new XmlDocument();

                    lock (syncLock)
                    {
                        CommonMethods.LoadXML(ref xmlDoc, currMetaData);
                    }

                    folder.MetaName = xmlDoc.SelectSingleNode(XPATH_EXPR + "/name").InnerText;
                }
            }
        }

        private FolderCompareObject PopulateFolderWithMetaData(XmlDocument xmlDoc, FolderCompareObject folder, int counter)
        {
            XmlNode node = xmlDoc.SelectSingleNode(XPATH_EXPR + "/" + FOLDER + "[name=" + CommonMethods.ParseXpathString(folder.Name) + "]");
            if (node == null)
                return folder;

            XmlNode todoNode = xmlDoc.SelectSingleNode(XPATH_EXPR + "/" + FOLDER + "[name=" + CommonMethods.ParseXpathString(folder.Name) + "]/todo");
            
            if (todoNode != null)
            {
                XmlNodeList nodeList = todoNode.ChildNodes;
                for (int i = 0; i < nodeList.Count;i++)
                {
                    XmlNode childNode = nodeList[i];
                    switch (childNode.Name)
                    {
                        case NODE_ACTION:
                            string action = childNode.InnerText;
                            if (action.Equals("Delete"))
                            {
                                folder.Todo[counter] = ToDo.Delete;
                            }
                            else
                            {
                                folder.Todo[counter] = ToDo.Rename;
                            }
                            break;

                        case NODE_DATE:
                            long date = long.Parse(childNode.InnerText);
                            folder.TodoTimestamp[counter] = date;
                            break;

                        case NODE_NEWNAME:
                            string newName = childNode.InnerText;
                            folder.TodoNewName[counter] = newName;
                            break;
                    }
                }
            }

            folder.MetaExists[counter] = true;
            return folder;
        }

        private void ProcessFolderMetaData(FolderCompareObject folder, int numOfPaths)
        {
            for (int i = 0; i < numOfPaths ; i++)
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
