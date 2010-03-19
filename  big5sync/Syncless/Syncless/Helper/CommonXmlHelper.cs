using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
namespace Syncless.Helper
{
    public static class CommonXmlHelper
    {
        public static void SaveXml(XmlDocument xml, string path)
        {
            XmlTextWriter textWriter = null;
            FileStream fs = null;

            FileInfo fileInfo = new FileInfo(path);
            if (!fileInfo.Directory.Exists)
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
                    }
                }
                textWriter = new XmlTextWriter(fs, Encoding.UTF8);
                textWriter.Formatting = Formatting.Indented;
                xml.WriteContentTo(textWriter);
            }
            catch (IOException io)
            {
                //TODO : Error Log
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
                Core.ExceptionHandler.Handle(xml);
                return null;
            }
            catch (IOException io)
            {
                Core.ExceptionHandler.Handle(io);
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
