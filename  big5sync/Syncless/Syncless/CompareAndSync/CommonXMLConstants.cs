using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.CompareAndSync
{
    public static class CommonXMLConstants
    {
        //XML Paths
        public const string MetaDir = ".syncless";
        public const string XMLName = "syncless.xml";
        public const string TodoName = "todo.xml";
        public const string MetadataPath = MetaDir + "\\" + XMLName;
        public const string TodoPath = MetaDir + "\\" + TodoName;

        //XML Metadata (syncless.xml)
        public const string NodeName = "name";
        public const string NodeSize = "size";
        public const string NodeHash = "hash";
        public const string NodeLastCreated = "last_created";

        //Todo Metadata (todo.xml)
        public const string NodeLastKnownState = "last_known_state";
        public const string NodeAction = "action";
        public const string ActionDeleted = "deleted";

        //Common Elements in Metadatas
        public const string NodeFolder = "folder";
        public const string NodeFile = "file";
        public const string NodeLastModified = "last_modified";
        public const string NodeLastUpdated = "last_updated";
        
        //XML Metadata XPath (syncless.xml)
        public const string XPathExpr = "/meta-data";
        public const string XPathName = "/" + NodeName;
        public const string XPathSize = "/" + NodeSize;
        public const string XPathHash = "/" + NodeHash;
        public const string XPathLastCreated = "/" + NodeLastCreated;

        //Todo Metadata XPath
        public const string XPathLastKnownState = "/" + NodeLastKnownState;
        public const string XPathAction = "/" + NodeAction;

        //Common XPath
        public const string XPathFolder = "/" + NodeFolder;
        public const string XPathFile = "/" + NodeFile;
        public const string XPathLastModified = "/" + NodeLastModified;
        public const string XPathLastUpdated = "/" + NodeLastUpdated;


    }

}
