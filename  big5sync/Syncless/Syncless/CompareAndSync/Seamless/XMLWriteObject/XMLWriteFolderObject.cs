using Syncless.CompareAndSync.Enum;

namespace Syncless.CompareAndSync.Seamless.XMLWriteObject
{
    public class XMLWriteFolderObject : BaseXMLWriteObject
    {
        //Create
        public XMLWriteFolderObject(string name, string parent, long creationTimeUtc, MetaChangeType changeType, long metaUpdatedUtc)
            : base(name, parent, creationTimeUtc, changeType, metaUpdatedUtc)
        {
        }

        //Rename
        public XMLWriteFolderObject(string name, string newName, string parent, MetaChangeType changeType, long metaUpdatedUtc)
            : base(name, newName, parent, changeType, metaUpdatedUtc)
        {
        }

        //Delete
        public XMLWriteFolderObject(string name, string parent, MetaChangeType changeType, long metaUpdatedUtc)
            : base(name, parent, changeType, metaUpdatedUtc)
        {
        }

    }
}