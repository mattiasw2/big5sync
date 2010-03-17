using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.CompareAndSync.Enum
{
    public enum MetaChangeType
    {
        //NoMeta,
        Delete, //File, Folder
        New, //File, Folder
        NoChange, //File, Folder
        Rename, //File, Folder
        Update
    }
}
