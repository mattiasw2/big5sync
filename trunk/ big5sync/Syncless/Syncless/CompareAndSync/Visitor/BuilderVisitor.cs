using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;
using Syncless.CompareAndSync.CompareObject;

namespace Syncless.CompareAndSync.Visitor
{
    public class BuilderVisitor : IVisitor
    {
        #region IVisitor Members

        List<string> _filter;

        public BuilderVisitor()
        {
        }

        public BuilderVisitor(List<string> filter)
        {
            _filter = filter;
        }

        public void Visit(FileCompareObject file, string[] currentPaths)
        {
            //Do nothing
        }

        public void Visit(FolderCompareObject folder, string[] currentPaths)
        {
            for (int index = 0; index < currentPaths.Length; index++)
            {
                string path = currentPaths[index] + @"\" + folder.Name;
                DirectoryInfo f = new DirectoryInfo(path);

                if (f.Exists)
                {
                    DirectoryInfo[] infos = f.GetDirectories();
                    foreach (DirectoryInfo info in infos)
                    {
                        BaseCompareObject o = folder.GetChild(info.Name);
                        FolderCompareObject fco = null;

                        if (o == null)
                            fco = new FolderCompareObject(info.Name, currentPaths.Length, folder);
                        else
                            fco = (FolderCompareObject)o;

                        fco.CreationTime[index] = info.CreationTime.Ticks;
                        fco.Exists[index] = true;

                        if (o == null)
                            folder.AddChild(fco);
                    }

                    FileInfo[] fileInfos = f.GetFiles();
                    foreach (FileInfo info in fileInfos)
                    {
                        BaseCompareObject o = folder.GetChild(info.Name);
                        FileCompareObject fco = null;

                        if (o == null)
                            fco = new FileCompareObject(info.Name, currentPaths.Length, folder);
                        else
                            fco = (FileCompareObject)o;

                        fco.CreationTime[index] = info.CreationTime.Ticks;
                        fco.Hash[index] = CalculateMD5Hash(info);
                        fco.LastWriteTime[index] = info.LastWriteTime.Ticks;
                        fco.Length[index] = info.Length;
                        fco.Exists[index] = true;

                        if (o == null)
                            folder.AddChild(fco);
                    }
                }
                else
                {
                    //Do nothing
                }
            }
        }

        public void Visit(RootCompareObject root)
        {
            Visit(root, root.Paths);
        }

        #endregion

        private static string CalculateMD5Hash(FileInfo fileInput)
        {
            Debug.Assert(fileInput.Exists);
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
    }
}
