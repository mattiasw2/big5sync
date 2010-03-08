using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestNewComparer.CompareAndSync
{
    public class CompareObjectHelper
    {
        public static void PreTraverseFolder(FolderCompareObject folder, int level,string[] currentPath, IVisitor visitor)
        {
            visitor.Visit(folder,level, currentPath);
            List<CompareObject> objectList = folder.ObjectList;
            foreach (CompareObject o in objectList)
            {
                string[] newCurrentPath = new string[currentPath.Length];
                for (int i = 0; i < currentPath.Length; i++)
                {
                    newCurrentPath[i] = currentPath[i] + o.Name;
                }
                if(o is FolderCompareObject){
                    
                    PreTraverseFolder((FolderCompareObject)o,level+1,newCurrentPath,visitor);
                }else{
                    visitor.Visit((FileCompareObject)o,level+1, newCurrentPath);

                }
            }
            
        }
        public static void PreTraverseFolder(RootCompareObject root, IVisitor visitor)
        {
            visitor.Visit(root);
            List<CompareObject> objectList = root.ObjectList;
            foreach (CompareObject o in objectList)
            {
                string[] newCurrentPath = root.Paths;
                
                if (o is FolderCompareObject)
                {

                    PreTraverseFolder((FolderCompareObject)o,1, newCurrentPath, visitor);
                }
                else
                {
                    visitor.Visit((FileCompareObject)o,1, newCurrentPath);

                }
            }
        }

    }
}
