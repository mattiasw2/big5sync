using Syncless.CompareAndSync.Enum;

namespace Syncless.CompareAndSync.Seamless.XMLWriteObject
{
    public class XMLWriteFolderObject : BaseXMLWriteObject
    {
        //Create
        public XMLWriteFolderObject(string name, string parent, long creationTime, MetaChangeType changeType, long metaUpdated)
            : base(name, parent, creationTime, changeType, metaUpdated)
        {
        }

        //Rename
        public XMLWriteFolderObject(string name, string newName, string parent, MetaChangeType changeType, long metaUpdated)
            : base(name, newName, parent, changeType, metaUpdated)
        {
        }

        //Delete
        public XMLWriteFolderObject(string name, string parent, MetaChangeType changeType, long metaUpdated)
            : base(name, parent, changeType, metaUpdated)
        {
        }

    }
}