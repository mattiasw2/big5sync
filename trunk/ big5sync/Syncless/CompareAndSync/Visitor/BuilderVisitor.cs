using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;
using CompareAndSync.CompareObject;

namespace CompareAndSync.Visitor
{
    public class BuilderVisitor : IVisitor
    {
        #region IVisitor Members

        public void Visit(FileCompareObject file, int level, string[] currentPaths)
        {
            //Do nothing
        }

        public void Visit(FolderCompareObject folder, int level, string[] currentPaths)
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
                            fco = new FolderCompareObject(info.Name, currentPaths.Length);
                        else
                            fco = (FolderCompareObject)o;

                        fco.CreationTime[index] = info.CreationTime.Ticks;
                        fco.ExistsArray[index] = true;

                        if (o == null)
                            folder.AddChild(fco);
                    }

                    FileInfo[] fileInfos = f.GetFiles();
                    foreach (FileInfo info in fileInfos)
                    {
                        BaseCompareObject o = folder.GetChild(info.Name);
                        FileCompareObject fco = null;

                        if (o == null)
                            fco = new FileCompareObject(info.Name, currentPaths.Length);
                        else
                            fco = (FileCompareObject)o;

                        fco.CreationTime[index] = info.CreationTime.Ticks;
                        fco.Hash[index] = CalculateMD5Hash(info);
                        fco.LastWriteTime[index] = info.LastWriteTime.Ticks;
                        fco.Length[index] = info.Length;
                        fco.ExistsArray[index] = true;

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
            string[] rootPaths = root.Paths;
            for (int index = 0; index < rootPaths.Length; index++)
            {
                string path = rootPaths[index];
                DirectoryInfo folder = new DirectoryInfo(path);

                if (folder.Exists)
                {
                    DirectoryInfo[] infos = folder.GetDirectories();
                    foreach (DirectoryInfo info in infos)
                    {
                        BaseCompareObject o = root.GetChild(info.Name);
                        FolderCompareObject fco = null;

                        if (o == null)
                            fco = new FolderCompareObject(info.Name, rootPaths.Length);
                        else
                            fco = (FolderCompareObject)o;

                        fco.CreationTime[index] = info.CreationTime.Ticks;
                        fco.ExistsArray[index] = true;

                        if (o == null)
                            root.AddChild(fco);
                    }

                    FileInfo[] fileInfos = folder.GetFiles();
                    foreach (FileInfo info in fileInfos)
                    {
                        BaseCompareObject o = root.GetChild(info.Name);
                        FileCompareObject fco = null;

                        if (o == null)
                            fco = new FileCompareObject(info.Name, rootPaths.Length);
                        else
                            fco = (FileCompareObject)o;

                        fco.CreationTime[index] = info.CreationTime.Ticks;
                        fco.Hash[index] = CalculateMD5Hash(info);
                        fco.LastWriteTime[index] = info.LastWriteTime.Ticks;
                        fco.Length[index] = info.Length;
                        fco.ExistsArray[index] = true;

                        if (o == null)
                            root.AddChild(fco);
                    }
                }
                else
                {
                    //Do nothing
                }
            }
        }

        #endregion

        private static string CalculateMD5Hash(FileInfo fileInput)
        {
            Debug.Assert(fileInput.Exists);
            //Debug.Assert(fileInput.Directory.Name != XMLHelper.METADATADIR);
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
