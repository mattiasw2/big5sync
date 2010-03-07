using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace Redesign
{
    public class FileCompareObject : CompareObject
    {
        private string _hash;
        private long _length, _lastWriteTime;

        public FileCompareObject(string origin, FileInfo file)
            : base(origin)
        {
            base._fullName = file.FullName;
            base._name = file.Name;
            _hash = CalculateMD5Hash(file);
            _length = file.Length;
            base._creationTime = file.CreationTime.Ticks;
            _lastWriteTime = file.LastWriteTime.Ticks;
        }

        public FileCompareObject(string origin, string fullName, string name, string hash, long length, long creationTime, long lastWriteTime) :
            base(origin, fullName, name, creationTime)
        {
            _hash = hash;
            _length = length;
            _lastWriteTime = lastWriteTime;            
        }

        public string Hash
        {
            get { return _hash; }
        }

        public long LastWriteTime
        {
            get { return _lastWriteTime; }
        }

        public long Length
        {
            get { return _length; }
        }

        private static string CalculateMD5Hash(FileInfo fileInput)
        {
            FileStream fileStream = null;
            try
            {
                fileStream = fileInput.OpenRead();
            }
            catch (IOException)
            {
                fileInput.Refresh();
                fileStream = fileInput.OpenRead();
            }

            byte[] fileHash = MD5.Create().ComputeHash(fileStream);
            fileStream.Close();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < fileHash.Length; i++)
            {
                sb.Append(fileHash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        public override string ToString()
        {
            string s = "File: " + base._fullName;
            return s;
        }
    }
}
