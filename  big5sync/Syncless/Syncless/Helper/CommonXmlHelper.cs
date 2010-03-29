using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Threading;
namespace Syncless.Helper
{
    public static class CommonXmlHelper
    {
        public static void SaveXml(XmlDocument xml, string path)
        {
            XmlTextWriter textWriter = null;
            FileStream fs = null;

            FileInfo fileInfo = new FileInfo(path);
            if (fileInfo.Directory!=null && !fileInfo.Directory.Exists)
            {
                DirectoryInfo info = Directory.CreateDirectory(fileInfo.Directory.FullName);
                info.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
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
