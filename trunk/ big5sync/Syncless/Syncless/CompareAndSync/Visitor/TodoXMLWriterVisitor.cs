using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.CompareAndSync.CompareObject;
using Syncless.CompareAndSync.Enum;
using System.Xml;
using System.IO;

namespace Syncless.CompareAndSync.Visitor
{
    class TodoXMLWriterVisitor
    {

        private const string META_DIR = ".syncless";
        private const string XML_NAME = "todo.xml";
        private const string METADATAPATH = META_DIR + "\\" + XML_NAME;
        private const string TODO = "todo";
        private const string NAME = "name";
        private const string CHANGETYPE = "change_type";
        private const string DELETE = "delete";
        private const string RENAME = "rename";
        private const string XPATH_EXPR = "/meta-data";
        private const string PATHS = "paths";
        private const string COMPID = "comp_id";
        private static readonly object syncLock = new object();
        private static string[] listOfPath = null;

        public TodoXMLWriterVisitor(string[] pathList)
        {
            listOfPath = pathList;
        }

        public void Visit(FileCompareObject file, string[] currentPath)
        {
            for (int i = 0; i < currentPath.Length; i++)
            {
                ProcessFileFinalState(currentPath[i], file, i);
            }
        }

        public void Visit(FolderCompareObject folder, string[] currentPath)
        {
            for (int i = 0; i < currentPath.Length; i++)
            {
                ProcessFolderFinalState(currentPath[i], folder, i);
            }
        }

        public void Visit(RootCompareObject root)
        {
            //
        }

        private void ProcessFileFinalState(string currentPath , FileCompareObject file , int counter)
        {
            string xmlPath = Path.Combine(currentPath, METADATAPATH);
            CreateFileIfNotExist(currentPath);
            FinalState? [] finalStateList = file.FinalState;
            FinalState? state = finalStateList[counter];
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);

            switch (state)
            {
                case FinalState.Deleted:
                    WriteToDoFileObject(xmlDoc, file, DELETE);
                    break;
                case FinalState.Renamed:
                    WriteToDoFileObject(xmlDoc, file, RENAME);
                    break;
            }

            xmlDoc.Save(xmlPath);
        }

        private void CreateFileIfNotExist(string path)
        {
            string xmlPath = Path.Combine(path, METADATAPATH);
            if (File.Exists(xmlPath))
                return;
            lock (syncLock)
            {
                Directory.CreateDirectory(Path.Combine(path, META_DIR));
                XmlTextWriter writer = new XmlTextWriter(xmlPath, null);
                writer.Formatting = Formatting.Indented;
                writer.WriteStartDocument();
                writer.WriteStartElement("meta-data");
                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Flush();
                writer.Close();
            }

        }

        private void WriteToDoFileObject(XmlDocument xmlDoc , FileCompareObject file , string type)
        {
            XmlElement todoElement = xmlDoc.CreateElement(TODO);
            XmlElement nameElement = xmlDoc.CreateElement(NAME);
            XmlElement changeTypeElement = xmlDoc.CreateElement(CHANGETYPE);

            XmlText nameText = xmlDoc.CreateTextNode(file.Name);
            XmlText changeTypeText = xmlDoc.CreateTextNode(type);

            nameElement.AppendChild(nameText);
            changeTypeElement.AppendChild(changeTypeText);
            
            todoElement.AppendChild(nameElement);
            todoElement.AppendChild(changeTypeElement);

            XmlNode node = xmlDoc.SelectSingleNode(XPATH_EXPR);
            node.AppendChild(todoElement);
            WritePaths(xmlDoc);
        }

        private void WritePaths(XmlDocument xmlDoc)
        {
            if (listOfPath == null || listOfPath.Length == 0)
                return;
            CheckForPathNode(xmlDoc);
            XmlNode node = null;
            XmlNode pathNode = null;
            for (int i = 0; i < listOfPath.Length; i++)
            {
                node = xmlDoc.SelectSingleNode(XPATH_EXPR + "/paths=['" + listOfPath[i] + "']");
                if (node == null)
                {
                    XmlText compIDValue = xmlDoc.CreateTextNode(listOfPath[i]);
                    XmlElement compIDElement = xmlDoc.CreateElement(COMPID);
                    compIDElement.AppendChild(compIDValue);
                    pathNode = xmlDoc.SelectSingleNode(XPATH_EXPR + "/path");
                    pathNode.AppendChild(compIDElement);
                }
            }
        }

        private void CheckForPathNode(XmlDocument xmlDoc)
        {
            XmlNode node = xmlDoc.SelectSingleNode(XPATH_EXPR + "/path");
            if (node != null)
                return;
            XmlNode pathNode = xmlDoc.CreateElement(PATHS);
            XmlNode root = xmlDoc.SelectSingleNode(XPATH_EXPR);
            root.AppendChild(pathNode);
        }

        private void ProcessFolderFinalState(string currentPath, FolderCompareObject folder, int counter)
        {
            string xmlPath = Path.Combine(currentPath, METADATAPATH);
            CreateFileIfNotExist(currentPath);
            FinalState?[] finalStateList = folder.FinalState;
            FinalState? state = finalStateList[counter];
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);

            switch (state)
            {
                case FinalState.Deleted:
                    WriteToDoFolderObject(xmlDoc, folder, DELETE);
                    break;
                case FinalState.Renamed:
                    WriteToDoFolderObject(xmlDoc, folder, RENAME);
                    break;
            }

            xmlDoc.Save(xmlPath);
        }

        private void WriteToDoFolderObject(XmlDocument xmlDoc, FolderCompareObject folder, string type)
        {
            XmlElement todoElement = xmlDoc.CreateElement(TODO);
            XmlElement nameElement = xmlDoc.CreateElement(NAME);
            XmlElement changeTypeElement = xmlDoc.CreateElement(CHANGETYPE);

            XmlText nameText = xmlDoc.CreateTextNode(folder.Name);
            XmlText changeTypeText = xmlDoc.CreateTextNode(type);

            nameElement.AppendChild(nameText);
            changeTypeElement.AppendChild(changeTypeText);

            todoElement.AppendChild(nameElement);
            todoElement.AppendChild(changeTypeElement);

            XmlNode node = xmlDoc.SelectSingleNode(XPATH_EXPR);
            node.AppendChild(todoElement);
            WritePaths(xmlDoc);
        }



    }
}
