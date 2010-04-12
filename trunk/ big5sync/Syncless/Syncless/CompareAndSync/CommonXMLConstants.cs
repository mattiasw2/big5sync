namespace Syncless.CompareAndSync
{
    internal static class CommonXMLConstants
    {
        //XML Paths
        internal const string MetaDir = ".syncless";
        internal const string XMLName = "syncless.xml";
        internal const string LastKnownStateName = "lastknownstate.xml";
        internal const string MetadataPath = MetaDir + "\\" + XMLName;
        internal const string LastKnownStatePath = MetaDir + "\\" + LastKnownStateName;

        //XML Metadata (syncless.xml)
        internal const string NodeMetaData = "meta-data";
        internal const string NodeName = "name";
        internal const string NodeSize = "size";
        internal const string NodeHash = "hash";
        internal const string NodeLastCreatedUtc = "last_created_utc";

        //Last Known State Metadata (lastknownstate.xml)
        internal const string NodeLastKnownState = "last_known_state";
        internal const string NodeAction = "action";
        internal const string ActionDeleted = "deleted";

        //Common Elements in Metadatas
        internal const string NodeFolder = "folder";
        internal const string NodeFile = "file";
        internal const string NodeLastModifiedUtc = "last_modified_utc";
        internal const string NodeLastUpdatedUtc = "last_updated_utc";
        
        //XML Metadata XPath (syncless.xml)
        internal const string XPathExpr = "/meta-data";
        internal const string XPathName = "/" + NodeName;
        internal const string XPathSize = "/" + NodeSize;
        internal const string XPathHash = "/" + NodeHash;
        internal const string XPathLastCreated = "/" + NodeLastCreatedUtc;

        //Last Known State Metadata XPath
        internal const string XPathLastKnownState = "/" + NodeLastKnownState;
        internal const string XPathAction = "/" + NodeAction;

        //Common XPath
        internal const string XPathFolder = "/" + NodeFolder;
        internal const string XPathFile = "/" + NodeFile;
        internal const string XPathLastModified = "/" + NodeLastModifiedUtc;
        internal const string XPathLastUpdated = "/" + NodeLastUpdatedUtc;

    }

}
