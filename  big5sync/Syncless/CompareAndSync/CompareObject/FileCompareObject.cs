using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompareAndSync.CompareObject
{
    public class FileCompareObject : BaseCompareObject
    {
        private string[] _hash;
        private long[] _length, _lastWriteTime;

        public FileCompareObject(string name, int numOfPaths)
            : base (name, numOfPaths)
        {
            _hash = new string[numOfPaths];
            _length = new long[numOfPaths];
            _lastWriteTime = new long[numOfPaths];
        }

        public string[] Hash
        {
            get { return _hash; }
            set { _hash = value; }
        }

        public long[] Length
        {
            get { return _length; }
            set { _length = value; }
        }

        public long[] LastWriteTime
        {
            get { return _lastWriteTime; }
            set { _lastWriteTime = value; }
        }
    }
}
