using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Security.Cryptography;
using System.Diagnostics;

namespace XMLGenerator
{
    class GenerateXML
    {
        static public void Main()
        {
            string nameOfFile = "C:\\Documents and Settings\\Nil\\Desktop\\meta-data.xml";
            string readFrom = @"C:\Documents and Settings\Nil\Desktop\3212";
            generateXML(nameOfFile,readFrom);
            Console.ReadKey();
        }

        /// <summary>
        /// Creates an xml writer and reads the path given by the user and generate all folders
        /// and files according to the directory structure
        /// </summary>
        /// <param name="nameOfFile"> Name of the xml file to write to</param>
        /// <param name="readFrom"> Read from a given folder path </param>
        public static void generateXML(string nameOfFile,string readFrom)
        {
            Debug.Assert(!(nameOfFile.Equals("") || nameOfFile == null));
            Debug.Assert(!(readFrom.Equals("") || readFrom == null));
           
            XmlTextWriter writer = new XmlTextWriter(nameOfFile, null);
            writer.Formatting = Formatting.Indented;
            writer.WriteStartDocument();
            writer.WriteStartElement("meta-data");
            writer.WriteElementString("last_modified",(DateTime.Now.Ticks).ToString());
            handleSubDirectories(writer, new DirectoryInfo(readFrom));
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
        public static void handleSubDirectories(XmlTextWriter writer,DirectoryInfo dirInfo)
        {
            Debug.Assert(writer != null);
            Debug.Assert(dirInfo != null);

            foreach (DirectoryInfo subDirInfo in dirInfo.GetDirectories())
            {
                writer.WriteStartElement("folder");
                writer.WriteElementString("name_of_folder", subDirInfo.Name);
                handleSubDirectories(writer, subDirInfo);
                writer.WriteEndElement();
            }

            foreach (FileInfo fileInfo in dirInfo.GetFiles())
            {
                byte [] md5bytes = MD5.Create().ComputeHash(File.ReadAllBytes(fileInfo.FullName));
                string hashedVal = "";
                foreach(byte md5 in md5bytes)
                {
                    hashedVal = hashedVal + md5;
                }

                writer.WriteStartElement("files");
                writer.WriteElementString("name", fileInfo.Name);
                writer.WriteElementString("size", fileInfo.Length.ToString());
                writer.WriteElementString("hash", hashedVal);
                writer.WriteElementString("last_modified",fileInfo.LastWriteTime.Ticks.ToString());
                writer.WriteEndElement();
            }
        }

    }

    
}
