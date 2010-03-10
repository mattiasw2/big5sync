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
        #region IVisitor Members

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

        public void Visit(FileCompareObject file, string[] currentPath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            for (int i = 0; i < currentPath.Length; i++)
            {
                if (currentPath[i].Contains(META_DIR))
                    continue;
                string path = Path.Combine(currentPath[i], METADATAPATH);
                if (!File.Exists(path))
                    continue;
                xmlDoc.Load(path);
                file = PopulateFileWithMetaData(xmlDoc, file,i);
                xmlDoc.Save(path);
            }
        }

        public void Visit(FolderCompareObject folder, string[] currentPath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            for (int i = 0; i < currentPath.Length; i++)
            {
                if (currentPath[i].Contains(META_DIR))
                    continue;
                string path = Path.Combine(currentPath[i], METADATAPATH);
                if (!File.Exists(path))
                    continue;
                xmlDoc.Load(path);
                folder = PopulateFolderWithMetaData(xmlDoc, folder, i);
                xmlDoc.Save(path);
            }
        }

        public void Visit(RootCompareObject root)
        {
           //
        }

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

        private FolderCompareObject PopulateFolderWithMetaData(XmlDocument xmlDoc, FolderCompareObject folder, int counter)
        {
            XmlNode node = xmlDoc.SelectSingleNode(XPATH_EXPR + "/folder" + "[name='" + folder.Name + "']");
            if (node == null)
                return folder;

            folder.MetaExists[counter] = true;
            return folder;
        }

        #endregion
    }
}
