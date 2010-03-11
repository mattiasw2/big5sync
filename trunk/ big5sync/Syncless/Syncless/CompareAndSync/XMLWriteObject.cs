using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.CompareAndSync
{
    class XMLWriteObject
    {

        private string name ;
        private long size ;
        private string hash ;
        private long lastModified ;
        private long lastCreated ;
        private bool isFolder = false ;
        private string fullPath = "" ;
        private FileChangeType type;

        public XMLWriteObject(string name , long size , string hash , long lastModified , long lastCreated)
        {
            this.name = name;
            this.hash = hash;
            this.size = size;
            this.lastModified = lastModified;
            this.lastCreated = lastCreated;
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Hash
        {
            get { return hash; }
            set { hash = value; }
        }

        public long Size
        {
            get { return size; }
            set { size = value; }
        }

        public long LastModifiedTime
        {
            get { return lastModified; }
            set { lastModified = value; }
        }

        public long CreatedTime
        {
            get { return lastCreated; }
            set { lastCreated = value; }
        }

        public bool IsFolder
        {
            get { return isFolder; }
            set { isFolder = value; }
        }

        public string FullPath
        {
            get { return fullPath; }
            set { fullPath = value; }
        }

        public FileChangeType ChangeType
        {
            get { return type; }
            set { type = value; }
        }

    }
}
