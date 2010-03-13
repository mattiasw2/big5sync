using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.CompareAndSync.Enum;

namespace Syncless.CompareAndSync.XMLWriteObject
{
    public class XMLWriteFolderObject : BaseXMLWriteObject
    {
        //Create
        public XMLWriteFolderObject(string name, string fullPath, long creationTime, MetaChangeType changeType)
            : base(name, fullPath, creationTime, changeType)
        {
        }

        //Rename
        public XMLWriteFolderObject(string name, string newName, string fullPath, long creationTime, MetaChangeType changeType)
            : base(name, fullPath, creationTime, changeType)
        {
        }

        //Delete
        public XMLWriteFolderObject(string name, string fullPath, MetaChangeType changeType)
            : base(name, fullPath, changeType)
        {
        }


    }
}
