using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace Syncless.Filters
{
    public abstract class FileFilter:Filter
    {
        public FileFilter(FilterMode mode):base(mode)
        {
        }
        protected override List<string> Exclude(List<string> patterns)
        {
            List<string> outputList = new List<string>();
            foreach (string pattern in patterns)
            {
                if (Directory.Exists(pattern))
                {
                    //is directory
                    outputList.Add(pattern);
                    continue;
                }
                FileInfo file = new FileInfo(pattern);
                
                if (!Match(file.Name))
                {
                    outputList.Add(pattern);
                }
            }

            return outputList;
        }
        protected override List<string> Include(List<string> patterns)
        {
            List<string> outputList = new List<string>();
            foreach (string pattern in patterns)
            {
                if (Directory.Exists(pattern))
                {
                    //is directory
                    outputList.Add(pattern);
                    continue;
                }
                FileInfo file = new FileInfo(pattern);
                
                if (!Match(file.Name))
                {
                    outputList.Add(pattern);
                }
            }
            return outputList;
        }
    }
}
