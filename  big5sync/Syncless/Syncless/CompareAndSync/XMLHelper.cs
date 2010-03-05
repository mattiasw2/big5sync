using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Diagnostics;
using System.Security.Cryptography;

namespace Syncless.CompareAndSync
{
    class XMLHelper
    {
        private const string NODE_NAME = "name";
        private const string NODE_SIZE = "size";
        private const string NODE_HASH = "hash";
        private const string NODE_LAST_MODIFIED = "last_modified";
        private const string NODE_LAST_CREATED = "last_created";
        private const string FILES = "files";
        private const string FOLDER = "folder";
        private const string NAME_OF_FOLDER = "name_of_folder";
        public const string METADATADIR = "_syncless";
        public const string METADATAPATH = METADATADIR + "\\syncless.xml";
        public const string METADATATODO = METADATADIR + "\\todo.xml";
            

        private static readonly object syncLock = new object(); 
        /// <summary>
        /// Given a dirpath , breaks it up into a folder and returns an Xpath Expression
        /// If it is an empty string , return the root of the xml
        /// </summary>
        /// <param name="dirPath"></param>
        /// <returns> the final Xpath expression</returns>
        private static string ConcatPath(string dirPath)
        {
            Debug.Assert(!(dirPath == null));
            string finalExpr = "/meta-data";

            if (dirPath.Equals(""))
                return finalExpr;

            string[] splitWords = dirPath.Split('\\');


            for (int i = 0; i < splitWords.Length; i++)
            {
                if (!splitWords[i].Equals(""))
                    finalExpr = finalExpr + "/folder" + "[name_of_folder='" + splitWords[i] + "']";
            }
            return finalExpr;
        }

        /// <summary>
        /// Takes in an XMLDocument that has already been loaded with the file , and 2 other strings.
        /// First string is a path which is a valid folder of the xml structure.
        /// E.g. Java is a folder in C:\...\...\3212. So it is being represented by \Java in this case
        /// Second string is the entire full path of which the xml represents. 
        /// E.g. C:\...\...\3212
        /// </summary>
        /// <param name="path"> Truncated folder path . \Java </param>
        /// <param name="xmlDoc"> XMLDocument object that already been loaded with the xml file</param>
        /// <param name="fullPath"> C:\...\...\</param>
        /// <returns> The list of paths in string that can be found given a folder path</returns>
        public static List<CompareInfoObject> GetCompareInfoObjects(string folderPath)
        {

            string path = "";
            string namePath = Path.Combine(folderPath, METADATAPATH);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(namePath);
            FileInfo fileInfo = new FileInfo(namePath);
            string directory = fileInfo.Directory.Parent.FullName;

            try
            {

                CompareInfoObject infoObject = null;
                List<CompareInfoObject> pathList = new List<CompareInfoObject>();
                Stack<string> stack = new Stack<string>();
                stack.Push(path);


                while (stack.Count > 0)
                {
                    string dirPath = stack.Pop();
                    string temp = dirPath;

                    XmlNodeList nodeList = xmlDoc.SelectNodes(ConcatPath(dirPath));

                    foreach (XmlNode node in nodeList)
                    {
                        XmlNodeList childNodes = node.ChildNodes;
                        for (int i = 1; i < childNodes.Count; i++)
                        {
                            if (childNodes[i].FirstChild.Name.Equals("name_of_folder"))
                            {
                                stack.Push(dirPath + "\\" + childNodes[i].FirstChild.InnerText);
                                //Console.WriteLine(dirPath + "\\" + childNodes[i].FirstChild.InnerText);
                            }
                            else
                            {
                                if (!path.Equals(""))
                                    temp = dirPath.Replace(path, "");

                                string entirePath = directory + temp + "\\" + childNodes[i].FirstChild.InnerText;
                                infoObject = GenerateInfo(childNodes[i], directory, entirePath);
                                pathList.Add(infoObject);
                            }
                        }
                    }
                }
                return pathList;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return null;
            }
        }

        private static CompareInfoObject GenerateInfo(XmlNode node, string origin, string fullPath)
        {
            XmlNodeList nodeList = node.ChildNodes;
            string name = "";
            string size = "";
            string hash = "";
            string lastModified = "";
            string lastCreated = "";

            foreach (XmlNode childNode in nodeList)
            {
                switch (childNode.Name)
                {
                    case NODE_NAME:
                        name = childNode.InnerText;
                        break;
                    case NODE_SIZE: 
                        size = childNode.InnerText;
                        break;
                    case NODE_HASH: 
                        hash = childNode.InnerText;
                        break;
                    case NODE_LAST_MODIFIED: 
                        lastModified = childNode.InnerText;
                        break;
                    case NODE_LAST_CREATED: 
                        lastCreated = childNode.InnerText;
                        break;
                }
            }

            return new CompareInfoObject(origin, fullPath, name,long.Parse(lastCreated),long.Parse(lastModified),
                long.Parse(size), hash);
        }

        /// <summary>
        /// Creates an xml writer and reads the path given by the user and generate all folders
        /// and files according to the directory structure
        /// </summary>
        /// <param name="nameOfFile"> Name of the xml file to write to</param>
        /// <param name="readFrom"> Read from a given folder path </param>
        public static void GenerateXMLFile(string folderPath)
        {
            Debug.Assert(!(folderPath.Equals("") || folderPath == null));
            string writeTo = Path.Combine(folderPath, METADATAPATH);

            FileInfo metaFile = new FileInfo(writeTo);
            if (!metaFile.Exists)
            {
                if (!Directory.Exists(metaFile.DirectoryName))
                {
                    Directory.CreateDirectory(metaFile.DirectoryName);
                }
            }

            lock (syncLock)
            {
                Debug.Assert(!writeTo.Equals(""));
                XmlTextWriter writer = new XmlTextWriter(writeTo, null);
                writer.Formatting = Formatting.Indented;
                writer.WriteStartDocument();
                writer.WriteStartElement("meta-data");
                writer.WriteElementString("last_modified", (DateTime.Now.Ticks).ToString());
                HandleSubDirectories(writer, new DirectoryInfo(folderPath));
                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Flush();
                writer.Close();
        
            }

        }

        /// <summary>
        /// Recursively writes files in the current folders and sub directories through the XMLWriter
        /// </summary>
        /// <param name="writer"> XmltextWriter that is currently connected to a xml file stream</param>
        /// <param name="dirInfo"> directory info </param>
        public static void HandleSubDirectories(XmlTextWriter writer, DirectoryInfo dirInfo)
        {
            Debug.Assert(writer != null);
            Debug.Assert(dirInfo != null);

            foreach (DirectoryInfo subDirInfo in dirInfo.GetDirectories())
            {
                if (subDirInfo.Name != "_syncless")
                {
                    writer.WriteStartElement("folder");
                    writer.WriteElementString("name_of_folder", subDirInfo.Name);
                    HandleSubDirectories(writer, subDirInfo);
                    writer.WriteEndElement();
                }
            }

            foreach (FileInfo fileInfo in dirInfo.GetFiles())
            {
                FileStream fileStream = fileInfo.OpenRead();
                byte[] fileHash = MD5.Create().ComputeHash(fileStream);
                fileStream.Close();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < fileHash.Length; i++)
                {
                    sb.Append(fileHash[i].ToString("X2"));
                }

                writer.WriteStartElement("files");
                writer.WriteElementString("name", fileInfo.Name);
                writer.WriteElementString("size", fileInfo.Length.ToString());
                writer.WriteElementString("hash", sb.ToString());
                writer.WriteElementString("last_modified", fileInfo.LastWriteTime.Ticks.ToString());
                writer.WriteElementString("last_created", fileInfo.CreationTime.Ticks.ToString());
                writer.WriteEndElement();

            }
        }

        // STUB
        // PATHS TO BE CONFIRMED
        public static void GenerateTodoXml(List<string> paths , string path)
        {
            XmlTextWriter writer = new XmlTextWriter(path,null);
            writer.Formatting = Formatting.Indented;
            writer.WriteStartDocument();
            writer.WriteStartElement("todo");
            writer.WriteStartAttribute("lastUpdated");
            writer.WriteAttributeString("lastUpdated", DateTime.Now.Ticks.ToString());
            writer.WriteEndAttribute();

            writer.WriteStartElement("deleted");
            writer.WriteStartAttribute("lastUpdated");
            writer.WriteAttributeString("lastUpdated", DateTime.Now.Ticks.ToString());
            writer.WriteEndAttribute();

            writer.WriteElementString("file", "YOU NEED TO FILL IN HERE");
            foreach(string logicalDrive in paths)
            {
                writer.WriteElementString("paths",logicalDrive);
            }

            writer.WriteEndElement();

            writer.WriteStartElement("updated");
            writer.WriteStartAttribute("lastUpdated");
            writer.WriteAttributeString("lastUpdated", DateTime.Now.Ticks.ToString());
            writer.WriteEndAttribute();

            foreach (string logicalDrive in paths)
            {
                writer.WriteStartElement("files");
                writer.WriteElementString("to", "YOU NEED TO FILL IN ");
                writer.WriteElementString("from", "YOU NEED TO FILL IN ");
                writer.WriteEndElement();
            }

            foreach (string logicalDrives in paths)
            {
                writer.WriteElementString("paths" , "PATHS HERE");
            }

            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Flush();
            writer.Close();
        }

        // new methods for edit xml
        public static string getFileExpr(string type, string filePath)
        {
            Debug.Assert(!filePath.Equals(""));
            string[] splitWords = filePath.Split('\\');

            string finalExpr = "/meta-data";

            if (splitWords.Length <= 1 && type.Equals("files"))
                return finalExpr + "/files" + "[name='" + filePath + "']";
            else if (splitWords.Length <= 1 && type.Equals("folder"))
                return finalExpr;

            for (int i = 0; i < splitWords.Length; i++)
            {
                if (splitWords[i].Equals(""))
                    continue;

                if (type.Equals("files"))
                {
                    if (i == splitWords.Length - 1)
                    {
                        finalExpr = finalExpr + "/files" + "[name='" + splitWords[i] + "']";
                    }
                    else
                    {
                        finalExpr = finalExpr + "/folder" + "[name_of_folder='" + splitWords[i] + "']";
                    }
                }
                else
                {
                    if (i == splitWords.Length - 1)
                        break;
                    finalExpr = finalExpr + "/folder" + "[name_of_folder='" + splitWords[i] + "']";
                }

            }

            return finalExpr;
        }

        private static string getEntireFolder(string filePath) // FOR FOLDER AS XMLWRITEOBJ
        {
            string[] splitWords = filePath.Split('\\');
            string finalExpr = "/meta-data"; ;
            for (int i = 0; i < splitWords.Length; i++)
            {
                if (splitWords[i].Equals(""))
                    continue;
                finalExpr = finalExpr + "/folder" + "[name_of_folder='" + splitWords[i] + "']";
            }
            return finalExpr;
        }

        //get C:/asd/asd e.g
        private static string getFolderString(string filePath)
        {
            string[] splitWords = filePath.Split('\\');
            string folderPath = "";
            for (int i = 0; i < splitWords.Length; i++)
            {
                if (i == splitWords.Length - 1)
                    break;
                folderPath = folderPath + splitWords[i];
            }

            return folderPath;
        }

        //get 1.txt
        private static string getFileString(string path)
        {
            Debug.Assert(!path.Equals(""));
            string[] splitWords = path.Split('\\');
            return splitWords[splitWords.Length - 1];
        }

        private static string getRelativePath(string fullPath, string truncatedPath)
        {
            return fullPath.Replace(truncatedPath, "");
        }

        /* YC to Gordon: I need you to write this such that I will pass you the following
         * Origin path and CompareResult.
         * 1. Take the origin path, append the metadatapath to it, and check if it exists
         * 2. If it exists, then just edit, if it doesn't create it using the original method (that scans the entire directory and creates the xml)
         * 3. If we created a new XML, then we can just end here since the method has already iterated thru everything
         * 4. If we edit, then based on the type of filechange, the info needed is different
         * 5. For update and create, you need the filename (full path), hash, length, modified date, creation date (basically all the elements)
         * 6. For delete, you need the filename (full path), and then remove the element completely
         * 7. For rename, you need the old path and new path, and simply change the old name to new name
         *  Load into dom object then process the thing from there.
         *  Downcast compare result into FileCompareResult
         */
        public static void EditXML(string origin, List<XMLWriteObject> xmlObjList)
        {
            string xmlPath = Path.Combine(origin, METADATAPATH);
            if (File.Exists(xmlPath))
                ModifyExistingXML(origin , xmlPath, xmlObjList);
            else
                CreateNewXML(origin,xmlPath, xmlObjList);
        }

        private static void CreateNewXML(string origin , string xmlPath, List<XMLWriteObject> xmlObjList)
        {
            lock (syncLock) // create a new file 
            {
                Debug.Assert(!xmlPath.Equals(""));
                XmlTextWriter writer = new XmlTextWriter(xmlPath, null);
                writer.Formatting = Formatting.Indented;
                writer.WriteStartDocument();
                writer.WriteStartElement("meta-data");
                writer.WriteElementString("last_modified", (DateTime.Now.Ticks).ToString());
                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Flush();
                writer.Close();
            }

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);
            foreach (XMLWriteObject xmlWriteObj in xmlObjList)
            {
                if (xmlWriteObj.ChangeType == FileChangeType.Delete)
                    continue;
                string relativePath = xmlWriteObj.To.Replace(origin,"");
                string xpathExpr = getFileExpr(FOLDER, relativePath);
                XmlNode node = xmlDoc.SelectSingleNode(xpathExpr);
                if (node == null)
                    node = CreationOfNode(relativePath, xmlDoc);
                XmlNode fileElement = AddNode(relativePath , xmlDoc, xmlWriteObj);
                node.AppendChild(fileElement);
            }
            xmlDoc.Save(xmlPath);
        }

        

        private static void ModifyExistingXML(string origin , string xmlPath, List<XMLWriteObject> xmlObjList)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);

            foreach(XMLWriteObject xmlWriteObj in xmlObjList)
            {
                if (Directory.Exists(xmlWriteObj.To))
                {
                    HandleFolderChangeType(origin, xmlDoc, xmlWriteObj);
                    continue;
                }

                switch (xmlWriteObj.ChangeType)
                {
                    case FileChangeType.Create:
                        CreateFileInExistingXML(origin , xmlDoc , xmlWriteObj);
                        break;
                    case FileChangeType.Rename:
                        RenameFileInExistingXML(origin , xmlDoc , xmlWriteObj);
                        break;
                    case FileChangeType.Update:
                        UpdateFileInExistingXML(origin , xmlDoc , xmlWriteObj);
                        break;
                    case FileChangeType.Delete:
                        DeleteFileInExistingXML(origin , xmlDoc , xmlWriteObj);
                        break;
                }
            }
            xmlDoc.Save(xmlPath);
        }
        // FOR CREATE CASE IF PARENT NODE DOES NOT EXIST
        private static XmlNode CreationOfNode(string relativePath ,  XmlDocument xmlDoc)
        {
            string finalExpr = "/meta-data";
            string[] splitWords = relativePath.Split('\\');
            XmlNode currentNode = null;
            for (int i = 0; i < splitWords.Length - 1 ; i++)
            {
                if (splitWords[i].Equals(""))
                    continue;

                finalExpr = finalExpr + "/folder" + "[name_of_folder='" + splitWords[i] + "']";

                if (xmlDoc.SelectSingleNode(finalExpr) == null) // expression is invalid because no path exists
                {
                    XmlElement folder = xmlDoc.CreateElement(FOLDER);
                    XmlText nameOfFolderText = xmlDoc.CreateTextNode(NAME_OF_FOLDER);
                    folder.AppendChild(nameOfFolderText);

                    if (currentNode == null) // then get the root
                        currentNode = xmlDoc.SelectSingleNode("/meta-data");

                    currentNode.AppendChild(folder);
                }
                else
                {
                    currentNode = xmlDoc.SelectSingleNode(finalExpr);
                }

            }

            return currentNode;

        }

        private static void CreateFileInExistingXML(string origin , XmlDocument xmlDoc , XMLWriteObject xmlWriteObj)
        {
            string relativePath = xmlWriteObj.To.Replace(origin, "");
            string xpathExpr = getFileExpr(FOLDER, relativePath);
            XmlNode node = xmlDoc.SelectSingleNode(xpathExpr);

            if (node == null)
                node = CreationOfNode(relativePath,xmlDoc);

            XmlNode fileElement = AddNode(relativePath , xmlDoc, xmlWriteObj);

            node.AppendChild(fileElement);
        }

        private static void RenameFileInExistingXML(string origin , XmlDocument xmlDoc , XMLWriteObject xmlWriteObj)
        {
            string relativePath = xmlWriteObj.To.Replace(origin, "");
            string xpathExpr = getFileExpr(FOLDER,relativePath);
            XmlNode node = xmlDoc.SelectSingleNode(xpathExpr + "/files[" + "hash ='" +
                xmlWriteObj.NewHash + "' and last_created='" + xmlWriteObj.LastWriteTime + "']");

            if (node == null) //Maybe node does not exist in xml at all
                return;

            node.FirstChild.InnerText = getFileString(relativePath);
        }

        private static void UpdateFileInExistingXML(string origin, XmlDocument xmlDoc, XMLWriteObject xmlWriteObj)
        {
            string relativePath = xmlWriteObj.To.Replace(origin, "");
            string hash = xmlWriteObj.NewHash;
            string size = xmlWriteObj.Length.ToString();
            string lastModified = xmlWriteObj.LastWriteTime.ToString();
            string lastCreated = xmlWriteObj.CreationTime.ToString();

            string xpathExpr = getFileExpr(FILES, relativePath);
            XmlNode node = xmlDoc.SelectSingleNode(xpathExpr);

            if (node == null) //Maybe node does not exist in xml at all
                return;

            XmlNodeList childNodeList = node.ChildNodes;
            for (int i = 0; i < childNodeList.Count; i++)
            {
                XmlNode nodes = childNodeList[i];
                if (nodes.Name.Equals(NODE_SIZE))
                {
                    nodes.InnerText = size;
                }
                else if (nodes.Name.Equals(NODE_HASH))
                {
                    nodes.InnerText = hash;
                }
                else if (nodes.Name.Equals(NODE_LAST_MODIFIED))
                {
                    nodes.InnerText = lastModified;
                }
                else if (nodes.Name.Equals(NODE_LAST_CREATED))
                {
                    nodes.InnerText = lastCreated;
                }
            }
        }

        private static void DeleteFileInExistingXML(string origin, XmlDocument xmlDoc, XMLWriteObject xmlWriteObj)
        {
            string relativePath = xmlWriteObj.To.Replace(origin, "");
            string xpathExpr = getFileExpr(FILES, relativePath);
            XmlNode node = xmlDoc.SelectSingleNode(xpathExpr);

            if (node == null) //Maybe node does not exist in xml at all
                return;

            node.ParentNode.RemoveChild(node);
        }

        // ASSUME THAT FOLDER HAS NO UPDATE
        private static void HandleFolderChangeType(string origin, XmlDocument xmlDoc, XMLWriteObject xmlWriteObj)
        {
            string relativePath = xmlWriteObj.From.Replace(origin, "");
            string xpathExpr = getEntireFolder(relativePath);
            XmlNode node = null;
            switch (xmlWriteObj.ChangeType)
            {
                case FileChangeType.Create:
                    relativePath = xmlWriteObj.To.Replace(origin,"");
                    xpathExpr = getEntireFolder(relativePath);
                    node = xmlDoc.SelectSingleNode(xpathExpr);

                    if(node == null)
                        node = CreationOfNode(relativePath,xmlDoc);
                    XmlElement childFolderElement = xmlDoc.CreateElement(FOLDER); // PLS RMB TO CHANGE
                    XmlText textChild = xmlDoc.CreateTextNode(getFileString(relativePath));
                    childFolderElement.AppendChild(textChild);
                    node.AppendChild(childFolderElement);
                    break;
                case FileChangeType.Rename:
                    node = xmlDoc.SelectSingleNode(xpathExpr);
                    node.FirstChild.InnerText = getFileString(xmlWriteObj.To);
                    break;
                case FileChangeType.Delete:
                    node = xmlDoc.SelectSingleNode(xpathExpr);
                    node.ParentNode.RemoveChild(node);
                    break;
            }
        }

        private static XmlNode AddNode(string relativePath , XmlDocument xmlDoc, XMLWriteObject xmlWriteObj)
        {
            XmlText hashNode = xmlDoc.CreateTextNode(xmlWriteObj.NewHash);
            XmlText nameNode = xmlDoc.CreateTextNode(getFileString(xmlWriteObj.To));
            XmlText sizeNode = xmlDoc.CreateTextNode(xmlWriteObj.Length.ToString());
            XmlText lastModifiedNode = xmlDoc.CreateTextNode(xmlWriteObj.LastWriteTime.ToString());
            XmlText lastCreatedNode = xmlDoc.CreateTextNode(xmlWriteObj.CreationTime.ToString());
            XmlElement fileElement = xmlDoc.CreateElement(FILES);
            XmlElement nameElement = xmlDoc.CreateElement(NODE_NAME);
            XmlElement sizeElement = xmlDoc.CreateElement(NODE_SIZE);
            XmlElement hashElement = xmlDoc.CreateElement(NODE_HASH);
            XmlElement lastModifiedElement = xmlDoc.CreateElement(NODE_LAST_MODIFIED);
            XmlElement lastCreatedElement = xmlDoc.CreateElement(NODE_LAST_CREATED);

            string xpathExpr = getFileExpr(FOLDER, relativePath);
            XmlNode node = xmlDoc.SelectSingleNode(xpathExpr);

            if (node == null)
                node = CreationOfNode(relativePath, xmlDoc);

            nameElement.AppendChild(nameNode);
            sizeElement.AppendChild(sizeNode);
            hashElement.AppendChild(hashNode);
            lastModifiedElement.AppendChild(lastModifiedNode);
            lastCreatedElement.AppendChild(lastCreatedNode);

            fileElement.AppendChild(nameElement);
            fileElement.AppendChild(sizeElement);
            fileElement.AppendChild(hashElement);
            fileElement.AppendChild(lastModifiedElement);
            fileElement.AppendChild(lastCreatedElement);
            return fileElement;
        }




        /*
        public static void EditXml(string xmlpath, FileChangeType type, string filePath)
        {
            Debug.Assert(File.Exists(xmlpath));
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlpath);
            XmlNode node = null;
            string xpathExpr = "";

            if (type.Equals(FileChangeType.Delete))
            {
                xpathExpr = getFileExpr("files", filePath);
                node = xmlDoc.SelectSingleNode(xpathExpr);

                if (node == null) //Maybe node does not exist in xml at all
                    return;

                node.ParentNode.RemoveChild(node);
                xmlDoc.Save(xmlpath);
            }
            else if (type.Equals(FileChangeType.Rename))
            {
                xpathExpr = getFileExpr("folder", filePath);
                node = xmlDoc.SelectSingleNode(xpathExpr + "/files[" + "hash ='" +
                    "ENTER HASH CODE HERE" + "' and last_created='" + "ENTER LAST CREATED HERE" + "']");

                if (node == null) //Maybe node does not exist in xml at all
                    return;

                node.FirstChild.InnerText = getFileString(filePath);
                xmlDoc.Save(xmlpath);
            }
            else if (type.Equals(FileChangeType.Create))
            {
                xpathExpr = getFileExpr("files", filePath);
                node = xmlDoc.SelectSingleNode(xpathExpr);
                if (node != null) // Trying to create a node that existed 
                    return;

                XmlText hashNode = xmlDoc.CreateTextNode("ENTER HASH CODE HERE");
                XmlText nameNode = xmlDoc.CreateTextNode(getFileString(filePath));
                XmlText sizeNode = xmlDoc.CreateTextNode("ENTER SIZE HERE");
                XmlText lastModifiedNode = xmlDoc.CreateTextNode("ENTER LAST MODIFIED HERE");
                XmlText lastCreatedNode = xmlDoc.CreateTextNode("ENTER LAST CREATED HERE");
                XmlElement fileElement = xmlDoc.CreateElement("files");
                XmlElement nameElement = xmlDoc.CreateElement("name");
                XmlElement sizeElement = xmlDoc.CreateElement("size");
                XmlElement hashElement = xmlDoc.CreateElement("hash");
                XmlElement lastModifiedElement = xmlDoc.CreateElement("last_modified");
                XmlElement lastCreatedElement = xmlDoc.CreateElement("last_created");

                xpathExpr = getFileExpr("folder", filePath);
                node = xmlDoc.SelectSingleNode(xpathExpr);
                nameElement.AppendChild(nameNode);
                sizeElement.AppendChild(sizeNode);
                hashElement.AppendChild(hashNode);
                lastModifiedElement.AppendChild(lastModifiedNode);
                lastCreatedElement.AppendChild(lastCreatedNode);

                fileElement.AppendChild(nameElement);
                fileElement.AppendChild(sizeElement);
                fileElement.AppendChild(hashElement);
                fileElement.AppendChild(lastModifiedElement);
                fileElement.AppendChild(lastCreatedElement);

                node.AppendChild(fileElement);
                xmlDoc.Save(xmlpath);
            }
            else if (type.Equals(FileChangeType.Update))
            {
                string hash = "ENTER HASH HERE";
                string size = "ENTER SIZE HERE";
                string lastModified = "ENTER LAST MODIFIED HERE";
                string lastCreated = "ENTER LAST CREATED HERE";

                xpathExpr = getFileExpr("files", filePath);
                node = xmlDoc.SelectSingleNode(xpathExpr);

                if (node == null) //Maybe node does not exist in xml at all
                    return;

                XmlNodeList childNodeList = node.ChildNodes;
                for (int i = 0; i < childNodeList.Count; i++)
                {
                    XmlNode nodes = childNodeList[i];
                    if (nodes.Name.Equals("size"))
                    {
                        nodes.InnerText = size;
                    }
                    else if (nodes.Name.Equals("hash"))
                    {
                        nodes.InnerText = hash;
                    }
                    else if (nodes.Name.Equals("last_modified"))
                    {
                        nodes.InnerText = lastModified;
                    }
                    else if (nodes.Name.Equals("last_created"))
                    {
                        nodes.InnerText = lastCreated;
                    }
                }

                xmlDoc.Save(xmlpath);

            }
        }*/


    }
}