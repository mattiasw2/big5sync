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
        public static List<CompareInfoObject> GetCompareInfoObjects(string tagName, string folderPath)
        {

            string path = "";
            string namePath = folderPath + "\\" + "_syncless" + "\\" + tagName + ".xml";
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
        public static void GenerateXMLFile(string tagName, string folderPath)
        {
            Debug.Assert(!(tagName.Equals("") || tagName == null));
            Debug.Assert(!(folderPath.Equals("") || folderPath == null));
            string writeTo = folderPath + "\\" + "_syncless" + "\\" + tagName + ".xml";
            string directoryPath = folderPath + "\\_syncless";

            DirectoryInfo dirInfo = new DirectoryInfo(directoryPath);
            if (!dirInfo.Exists)
                Directory.CreateDirectory(directoryPath);


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

    }
}