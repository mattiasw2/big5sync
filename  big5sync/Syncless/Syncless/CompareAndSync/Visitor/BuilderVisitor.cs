using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using Syncless.CompareAndSync.CompareObject;
using Syncless.Filters;

namespace Syncless.CompareAndSync.Visitor
{
    public class BuilderVisitor : IVisitor
    {
        #region IVisitor Members

        private List<Filter> _filter;
        private FilterChain _filterChain;

        public BuilderVisitor(List<Filter> filter)
        {
            _filter = filter;
            _filterChain = new FilterChain();
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

                if (f.Exists && _filterChain.ApplyFilter(_filter, currentPaths[index]))
                {
                    DirectoryInfo[] infos = f.GetDirectories();

                    foreach (DirectoryInfo info in infos)
                    {
                        if (_filterChain.ApplyFilter(_filter, info.FullName))
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
                    }

                    FileInfo[] fileInfos = f.GetFiles();
                    foreach (FileInfo info in fileInfos)
                    {
                        if (_filterChain.ApplyFilter(_filter, info.FullName))
                        {
                            BaseCompareObject o = folder.GetChild(info.Name);
                            FileCompareObject fco = null;

                            if (o == null)
                                fco = new FileCompareObject(info.Name, currentPaths.Length, folder);
                            else
                                fco = (FileCompareObject)o;

                            fco.CreationTime[index] = info.CreationTime.Ticks;
                            fco.Hash[index] = CommonMethods.CalculateMD5Hash(info);
                            fco.LastWriteTime[index] = info.LastWriteTime.Ticks;
                            fco.Length[index] = info.Length;
                            fco.Exists[index] = true;

                            if (o == null)
                                folder.AddChild(fco);
                        }
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
    }
}
