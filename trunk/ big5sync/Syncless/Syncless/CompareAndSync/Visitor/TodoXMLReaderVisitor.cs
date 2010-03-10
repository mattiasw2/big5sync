using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using Syncless.CompareAndSync.CompareObject;
using Syncless.CompareAndSync.Enum;

namespace Syncless.CompareAndSync.Visitor
{
    class TodoXMLReaderVisitor
    {
        private const string META_DIR = ".syncless";
        private const string XML_NAME = @"\todo.xml";
        private const string METADATAPATH = META_DIR + @"\syncless.xml";
        private const string XPATH_EXPR = "/meta-data";
        private const string NODE_NAME = "name";
        private const string NODE_CHANGE_TYPE = "change_type";
        private const string DELETE = "delete";
        private const string RENAME = "rename";

        public void Visit(FileCompareObject file, string[] currentPath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            for (int i = 0; i < currentPath.Length; i++)
            {
                if(currentPath[i].Contains(META_DIR))
                    continue;
                string xmlPath = Path.Combine(currentPath[i], METADATAPATH);
                if (!File.Exists(xmlPath))
                    continue;
                xmlDoc.Load(xmlPath);
                PopulateFileWithTodo(xmlDoc, file, i);
                xmlDoc.Save(xmlPath);
            }
        }

        public void Visit(FolderCompareObject folder, string[] currentPath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            for (int i = 0; i < currentPath.Length; i++)
            {
                if (currentPath[i].Contains(META_DIR))
                    continue;
                string xmlPath = Path.Combine(currentPath[i], METADATAPATH);
                if (!File.Exists(xmlPath))
                    continue;
                xmlDoc.Load(xmlPath);
                PopulateFolderWithTodo(xmlDoc, folder, i);
                xmlDoc.Save(xmlPath);
            }
        }

        public void Visit(RootCompareObject root)
        {
            //
        }

        private FileCompareObject PopulateFileWithTodo(XmlDocument xmlDoc, FileCompareObject file, int counter)
        {
            XmlNode node = xmlDoc.SelectSingleNode(XPATH_EXPR + "/todo" + "[name='" + file.Name + "']");
            if (node == null)
                return file;

            XmlNodeList childNodeList = node.ChildNodes;
            for (int i = 0; i < childNodeList.Count; i++)
            {
                XmlNode childNode = childNodeList[i];
                if (childNode.Name.Equals(NODE_CHANGE_TYPE))
                {
                    if (node.InnerText.Equals(DELETE))
                        file.ToDoAction = ToDo.Delete;
                    else if (node.InnerText.Equals(RENAME))
                        file.ToDoAction = ToDo.Rename;
                }
            }

            return file;
        }

        private FolderCompareObject PopulateFolderWithTodo(XmlDocument xmlDoc, FolderCompareObject folder, int counter)
        {
            XmlNode node = xmlDoc.SelectSingleNode(XPATH_EXPR + "/todo" + "[name='" + folder.Name + "']");
            if (node == null)
                return folder;

            XmlNodeList childNodeList = node.ChildNodes;
            for (int i = 0; i < childNodeList.Count; i++)
            {
                XmlNode childNode = childNodeList[i];
                if (childNode.Name.Equals(NODE_CHANGE_TYPE))
                {
                    if (node.InnerText.Equals(DELETE))
                        folder.ToDoAction = ToDo.Delete;
                    else if (node.InnerText.Equals(RENAME))
                        folder.ToDoAction = ToDo.Rename;
                }
            }

            return folder;
        }



    }
}
