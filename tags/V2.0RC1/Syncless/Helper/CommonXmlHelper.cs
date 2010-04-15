using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.IO;
using System.Threading;

namespace Syncless.Helper
{
    /// <summary>
    /// CommonXmlHelper class provides common Xml-related operations, such as loading and saving Xml files,
    /// which can be called by other classes in the Syncless.Tagging namespace.
    /// </summary>
    public static class CommonXmlHelper
    {
        /// <summary>
        /// Saves a XmlDocument object to a Xml file at a location specified by the path that is passed 
        /// as parameter
        /// </summary>
        /// <param name="xml">The XmlDocument object that is to be saved to a Xml file</param>
        /// <param name="path">The path where the Xml file is to be saved to</param>
        /// <remarks>This method can only be executed by one thread at any one time.</remarks>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void SaveXml(XmlDocument xml, string path)
        {
            XmlTextWriter textWriter = null;
            FileStream fs = null;
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception)
            {
                
            }
            FileInfo fileInfo = new FileInfo(path);
            if (fileInfo.Directory!=null && !fileInfo.Directory.Exists)
            {
                DirectoryInfo info = Directory.CreateDirectory(fileInfo.Directory.FullName);
                info.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }
            try
            {
                
                int tries = 0;
                while (tries < 5 && fs == null)
                {
                    try
                    {
                        fs = new FileStream(path, FileMode.OpenOrCreate);
                        break;
                    }
                    catch (Exception)
                    {
                        tries++;
                        Thread.Sleep(1000);
                    }
                }
                if (fs == null)
                {
                    throw new IOException();
                }
                textWriter = new XmlTextWriter(fs, Encoding.UTF8);
                textWriter.Formatting = Formatting.Indented;
                xml.WriteContentTo(textWriter);
            }
            catch (IOException io)
            {
                //TODO : Error Log
                //TODO : Throw to SLL
                Console.WriteLine(io.StackTrace);
            }
            finally
            {
                if (textWriter != null)
                {
                    try
                    {
                        textWriter.Close();
                    }
                    catch (Exception)
                    {
                    }
                }

            }

        }

        /// <summary>
        /// Loads a XmlDocument object from a Xml file that is saved at a location specified by the path 
        /// that is passed as parameter
        /// </summary>
        /// <param name="path">The path where the Xml file is to be loaded from</param>
        /// <returns>the XmlDocument object that is saved as a Xml file at the location specified</returns>
        /// <remarks>This method can only be executed by one thread at any one time.</remarks>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static XmlDocument LoadXml(string path)
        {
            FileStream fs = null;
            try
            {
                FileInfo info = new FileInfo(path);
                if (!info.Exists)
                {
                    return null;
                }
                fs = info.Open(FileMode.Open);
                XmlDocument xml = new XmlDocument();
                xml.Load(fs);
                return xml;
            }
            catch (XmlException xml)
            {
                Core.ServiceLocator.GetLogger(Core.ServiceLocator.DEBUG_LOG).Write(xml);
                return null;
            }
            catch (IOException io)
            {
                Core.ServiceLocator.GetLogger(Core.ServiceLocator.DEBUG_LOG).Write(io);
                return null;
            }
            finally
            {
                if (fs != null)
                {
                    try { fs.Close(); }
                    catch (Exception) { }
                }
            }

        }
    }
}
