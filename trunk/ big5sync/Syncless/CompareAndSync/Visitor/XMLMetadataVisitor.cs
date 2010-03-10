using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CompareAndSync.CompareObject;
using System.IO;
using System.Xml;

namespace CompareAndSync.Visitor
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
        private const string NODE_NAME_OF_FOLDER = "name_of_folder";

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
                xmlDoc = null;
                //xmlDoc.Save(path);
                ProcessFileMetaData(file, currentPaths);
            }
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
                xmlDoc = null;
                //xmlDoc.Save(path);
                ProcessFolderMetaData(folder, currentPaths);
            }
        }

        public void Visit(RootCompareObject root)
        {
            //
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
