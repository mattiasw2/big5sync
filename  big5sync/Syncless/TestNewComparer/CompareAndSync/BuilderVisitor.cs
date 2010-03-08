using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
namespace TestNewComparer.CompareAndSync
{
    public class BuilderVisitor : IVisitor
    {
        
        #region IVisitor Members

        public void Visit(FileCompareObject file, int level, string[] currentPaths)
        {
            //do nothing
        }

        public void Visit(FolderCompareObject folderCompareObject,int level, string[] currentPaths)
        {
            for (int index = 0; index < currentPaths.Length; index++)
            {
                string path = currentPaths[index] + @"\" + folderCompareObject.Name;
                DirectoryInfo folder = new DirectoryInfo(path);
                if (folder.Exists)
                {
                    DirectoryInfo[] infos = folder.GetDirectories();
                    foreach (DirectoryInfo info in infos)
                    {
                        CompareObject o = folderCompareObject.GetCompareObject(info.Name);
                        if (o == null)
                        {
                            FolderCompareObject fco = new FolderCompareObject(info.Name, currentPaths.Length);
                            fco.State[index] = 1;
                            folderCompareObject.AddChild(fco);
                        }
                        else
                        {
                            o.State[index] = 1;
                        }
                    }
                    FileInfo[] fileInfos = folder.GetFiles();
                    foreach (FileInfo info in fileInfos)
                    {
                        CompareObject o = folderCompareObject.GetCompareObject(info.Name);
                        if (o == null)
                        {
                            FileCompareObject fco = new FileCompareObject(info.Name, currentPaths.Length);
                            fco.State[index] = 1;
                            folderCompareObject.AddChild(fco);
                        }
                        else
                        {
                            o.State[index] = 1;
                        }
                    }
                }
                else
                {
                    //??? possible?
                }
            }
        }

        public void Visit(RootCompareObject root)
        {
            string[] rootPaths = root.Paths;
            for(int index = 0 ; index < rootPaths.Length ; index++)
            {
                string path = rootPaths[index];
                DirectoryInfo folder = new DirectoryInfo(path);
                if (folder.Exists)
                {
                    DirectoryInfo[] infos = folder.GetDirectories();
                    foreach(DirectoryInfo info in infos){
                        CompareObject o = root.GetCompareObject(info.Name);
                        if (o==null)
                        {
                            FolderCompareObject fco = new FolderCompareObject(info.Name, rootPaths.Length);
                            fco.State[index] = 1;
                            root.AddChild(fco);
                        }
                        else
                        {
                            o.State[index] = 1;
                        }
                    }
                    FileInfo[] fileInfos = folder.GetFiles();
                    foreach (FileInfo info in fileInfos)
                    {
                        CompareObject o = root.GetCompareObject(info.Name);
                        if (o == null)
                        {
                            FileCompareObject fco = new FileCompareObject(info.Name, rootPaths.Length);
                            fco.State[index] = 1;
                            root.AddChild(fco);
                        }
                        else
                        {
                            o.State[index] = 1;
                        }
                    }
                }
                else
                {
                    //??? possible?
                }
            }
        }

        #endregion
    }
    
}
