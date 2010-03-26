using Syncless.CompareAndSync.Enum;

namespace Syncless.CompareAndSync.XMLWriteObject
{
    public class XMLWriteFolderObject : BaseXMLWriteObject
    {
        //Create
        public XMLWriteFolderObject(string name, string fullPath, long creationTime, MetaChangeType changeType, long metaUpdated)
            : base(name, fullPath, creationTime, changeType, metaUpdated)
        {
        }

        //Rename
        public XMLWriteFolderObject(string name, string newName, string fullPath, long creationTime, MetaChangeType changeType, long metaUpdated)
            : base(name, newName, fullPath, creationTime, changeType, metaUpdated)
        {
        }

        //Delete
        public XMLWriteFolderObject(string name, string fullPath, MetaChangeType changeType, long metaUpdated)
            : base(name, fullPath, changeType, metaUpdated)
        {
        }


    }
}
