using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.CompareAndSync
{
    class XMLObject
    {
        private long size;
        private string name;
        private long createdTime;
        private long lastModifiedTime;
        private string hash;

        public XMLObject(string newName, string newHash, long newSize, long newCreatedTime, long newModifiedTime)
        {
            size = newSize;
            name = newName;
            hash = newHash;
            createdTime = newCreatedTime;
            lastModifiedTime = newModifiedTime;
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public long Size
        {
            get { return size; }
            set { size = value;}
        }

        public string Hash
        {
            get { return hash; }
            set { hash = value; }
        }

        public long LastModifiedTime
        {
            get { return lastModifiedTime; }
            set { lastModifiedTime = value; }
        }

        public long CreatedTime
        {
            get { return createdTime; }
            set { createdTime = value; }
        }


    }
}
