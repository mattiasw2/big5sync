using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Diagnostics;

namespace ExtractingXMLData
{
    class ExtractXML
    {

        /// <summary>
        /// XMLDocuments load the xml file
        /// 
        /// There are 2 strings that we need. The folder path where the meta-data explains its stuff
        /// as well as the path that we want to search in the meta-data.
        /// relative path = path where the user wants to search - folder path
        /// If the user wants the entire folder , it will pass in an empty string
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            string pathToSearch = @"C:\Documents and Settings\Nil\Desktop\3212\haskell";
            string directoryWhereItResides = @"C:\Documents and Settings\Nil\Desktop\3212";

            string relativePath = "";

            if (!pathToSearch.Equals(directoryWhereItResides))
                relativePath = pathToSearch.Replace(directoryWhereItResides, "");

            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.Load("C:\\Documents and Settings\\Nil\\Desktop\\meta-data.xml");

            List<string> list = printNodes(relativePath, xmlDoc, pathToSearch);

            foreach (string path in list)
            {
                Console.WriteLine(path);
            }

            Console.ReadKey();
        }

        /// <summary>
        /// Given a dirpath , breaks it up into a folder and returns an Xpath Expression
        /// If it is an empty string , return the root of the xml
        /// </summary>
        /// <param name="dirPath"></param>
        /// <returns> the final Xpath expression</returns>
        private static string concatPath(string dirPath)
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
        private static List<string> printNodes(string path, XmlDocument xmlDoc, string fullPath)
        {
            Debug.Assert(path != null);
            Debug.Assert(xmlDoc != null);
            Debug.Assert(!(fullPath.Equals("") || fullPath == null));

            try
            {
                List<string> pathList = new List<string>();
                Stack<string> stack = new Stack<string>();
                stack.Push(path);


                while (stack.Count > 0)
                {
                    string dirPath = stack.Pop();
                    string temp = dirPath;

                    XmlNodeList nodeList = xmlDoc.SelectNodes(concatPath(dirPath));

                    foreach (XmlNode node in nodeList)
                    {
                        XmlNodeList childNodes = node.ChildNodes;
                        for (int i = 1; i < childNodes.Count; i++)
                        {
                            if (childNodes[i].FirstChild.Name.Equals("name_of_folder"))
                            {
                                stack.Push(dirPath + "\\" + childNodes[i].FirstChild.InnerText);
                            }
                            else
                            {

                                if (!path.Equals(""))
                                    temp = dirPath.Replace(path, "");

                                pathList.Add(fullPath + temp + "\\" + childNodes[i].FirstChild.InnerText);
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
    }
}