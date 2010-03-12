using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.CompareAndSync.CompareObject;
using System.IO;
using System.Xml;
using Syncless.CompareAndSync.Enum;

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
        private const string FILES = "files";

        #region IVisitor Members

        public void Visit(FileCompareObject file, string[] currentPaths)
        {
            XmlDocument xmlDoc = new XmlDocument();
            for (int i = 0; i < currentPaths.Length; i++)
            {
                if (currentPaths[i].Contains(META_DIR))
                    continue;
                string path = Path.Combine(currentPaths[i], METADATAPATH);
                if (!File.Exists(path))
                    continue;
                xmlDoc.Load(path);
                file = PopulateFileWithMetaData(xmlDoc, file, i);
                //xmlDoc.Save(path);                
            }
            xmlDoc = null;
            ProcessFileMetaData(file, currentPaths);
        }

        public void Visit(FolderCompareObject folder, string[] currentPaths)
        {
            XmlDocument xmlDoc = new XmlDocument();
            for (int i = 0; i < currentPaths.Length; i++)
            {
                if (currentPaths[i].Contains(META_DIR))
                    continue;
                string path = Path.Combine(currentPaths[i], METADATAPATH);
                if (!File.Exists(path))
                    continue;
                xmlDoc.Load(path);
                folder = PopulateFolderWithMetaData(xmlDoc, folder, i);
                //xmlDoc.Save(path);               
            }
            ProcessFolderMetaData(folder, currentPaths);

            DirectoryInfo dirInfo = null;
            FileInfo[] fileList = null;
            DirectoryInfo[] dirInfoList = null;
            List<XMLCompareObject> xmlObjList = new List<XMLCompareObject>();
            List<string> xmlFolderList = null;
            string xmlPath = "";

            for (int i = 0; i < currentPaths.Length; i++)
            {
                string path = Path.Combine(currentPaths[i], folder.Name);


                if (Directory.Exists(path))
                {
                    dirInfo = new DirectoryInfo(path);
                    fileList = dirInfo.GetFiles();
                    dirInfoList = dirInfo.GetDirectories();
                    xmlPath = Path.Combine(path, METADATAPATH);
                    if (!File.Exists(xmlPath))
                        continue;

                    xmlDoc.Load(xmlPath);
                    xmlObjList = GetAllFilesInXML(xmlDoc);
                    xmlFolderList = GetAllFoldersInXML(xmlDoc);
                    RemoveSimilarFiles(xmlObjList, fileList);
                    RemoveSimilarFolders(xmlFolderList, dirInfoList);
                }


                AddFileToChild(xmlObjList, folder, i, currentPaths.Length);

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

                xmlDoc.Load(xmlPath);
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
            XmlNode node = xmlDoc.SelectSingleNode(XPATH_EXPR + "/files" + "[name='" + file.Name + "']");
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
            XmlNodeList xmlNodeList = xmlDoc.SelectNodes(XPATH_EXPR + "/files");
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
            string name = "";
            List<string> folderList = new List<string>();

            XmlNodeList xmlFolderList = xmlDoc.SelectNodes(XPATH_EXPR + "/folder");
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



        private void ProcessFileMetaData(FileCompareObject file, string[] currentPaths)
        {
            for (int i = 0; i < currentPaths.Length; i++)
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

        private FolderCompareObject PopulateFolderWithMetaData(XmlDocument xmlDoc, FolderCompareObject folder, int counter)
        {
            XmlNode node = xmlDoc.SelectSingleNode(XPATH_EXPR + "/folder" + "[name='" + folder.Name + "']");
            if (node == null)
                return folder;

            folder.MetaExists[counter] = true;
            return folder;
        }

        private void ProcessFolderMetaData(FolderCompareObject folder, string[] currentPaths)
        {
            for (int i = 0; i < currentPaths.Length; i++)
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
