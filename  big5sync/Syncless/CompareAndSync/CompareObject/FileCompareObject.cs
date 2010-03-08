using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompareAndSync.CompareObject
{
    public class FileCompareObject : BaseCompareObject
    {
        private string _hash;
        private long _length, _lastWriteTime;

        public FileCompareObject(string name, string hash, long length, long creationTime, long lastWriteTime, int numOfPaths)
            : base (name, creationTime, numOfPaths)
        {
            _hash = hash;
            _length = length;
            _lastWriteTime = lastWriteTime;
        }

        public string Hash
        {
            get { return _hash; }
        }

        public long Length
        {
            get { return _length; }
        }

        public long LastWriteTime
        {
            get { return _lastWriteTime; }
        }
    }
}
