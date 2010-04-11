using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Net;
using System.IO;

// Source: http://themech.net/2008/05/adding-check-for-update-option-in-csharp/
namespace SynclessUpdateChecker
{
    public class UpdateChecker
    {
        public readonly string XmlUrl;
        public readonly string NodeRootElement;
        public const string NodeLatestVersion = "version";
        public const string NodeUrl = "url";

        private string _updateUrl;

        public UpdateChecker()
        {
            XmlUrl = "http://big5sync.googlecode.com/files/LatestVersion.xml";
            NodeRootElement = "syncless";
        }

        public UpdateChecker(string xmlUrl, string rootElement)
        {
            XmlUrl = xmlUrl;
            NodeRootElement = rootElement;
        }

        /// <summary>
        /// Retrieves and compares the version stored on the server against the current running version.
        /// </summary>
        /// <returns>1 if new updates are found, 0 if no newer version is found. -1 is thrown if there is an error.</returns>
        public int GetNewVersion()
        {
            WebRequest request = WebRequest.Create(XmlUrl);
            request.Timeout = 3000;
            WebResponse response = request.GetResponse();
            Stream responseStream = response.GetResponseStream();

            Version newVersion = null;
            XmlTextReader reader = null;

            try
            {
                reader = new XmlTextReader(responseStream);
                reader.MoveToContent();
                string elementName = string.Empty;

                if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == NodeRootElement))
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                            elementName = reader.Name;
                        else
                        {
                            if ((reader.NodeType == XmlNodeType.Text) && (reader.HasValue))
                            {
                                switch (elementName)
                                {
                                    case NodeLatestVersion:
                                        newVersion = new Version(reader.Value);
                                        break;
                                    case NodeUrl:
                                        _updateUrl = reader.Value;
                                        break;
                                }
                            }
                        }
                    }
                }

                if (newVersion == null || string.IsNullOrEmpty(_updateUrl))
                    return -1;
            }
            catch (Exception e)
            {
                return -1;
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }

            Version curVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            if (curVersion.CompareTo(newVersion) < 0)
                return 1; // Newer version found
            else
                return 0; // Unable to find any updates
        }

        public string GetUpdateUrl
        {
            get { return _updateUrl; }
        }

    }
}
