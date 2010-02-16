using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Diagnostics;

namespace Syncless.CompareAndSync
{
    class XMLHelper
    {
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
                    finalExpr = finalExpr + "/folder" + "[name_of_folder='" + splitWords[i] +"']";
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
        public static List<CompareInfoObject> GetCompareInfoObjects(string path, XmlDocument xmlDoc, string fullPath,string directory)
        {
            Debug.Assert(path != null);
            Debug.Assert(xmlDoc != null);
            Debug.Assert(!(fullPath.Equals("") || fullPath == null));

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
                            }
                            else
                            {                               
                                if (!path.Equals(""))
                                    temp = dirPath.Replace(path, "");

                                string entirePath = fullPath + temp + "\\" + childNodes[i].FirstChild.InnerText;
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

        private static CompareInfoObject GenerateInfo(XmlNode node , string origin , string fullPath)
        {
            XmlNodeList nodeList = node.ChildNodes;
            int counter = 0 ;

            string name = "";
            string size = "";
            string hash = "";
            string lastModified = "";
            
            foreach (XmlNode childNode in nodeList)
            {
                if (counter == 0)
                    name = childNode.InnerText;
                else if (counter == 1)
                    size = childNode.InnerText;
                else if (counter == 2)
                    hash = childNode.InnerText;
                else
                    lastModified = childNode.InnerText;

                counter++;
            }

            return new CompareInfoObject(origin, fullPath, name, long.Parse(lastModified) ,
                long.Parse(size) , hash);
        }
    }
}