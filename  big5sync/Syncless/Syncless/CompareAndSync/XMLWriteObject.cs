using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.CompareAndSync
{
    public class XMLWriteObject
    {
        private string _origin, _from, _to, _newHash;
        private long _creationTime, _lastWriteTime, _length;

        //Files: Used for delete        
        private XMLWriteObject(FileChangeType changeType, string from)
        {
        }
        
        //Files: Used for rename
        private XMLWriteObject(FileChangeType changeType, string from, string to)
        {
        }

        //Files: Used for create/update
        private XMLWriteObject(FileChangeType changeType, string from, string to, string newHash, long creationTime, long lastWriteTime, long length)
        {
            _newHash = newHash;
            _creationTime = creationTime;
            _lastWriteTime = lastWriteTime;
            _length = length;
        }
    }
}
