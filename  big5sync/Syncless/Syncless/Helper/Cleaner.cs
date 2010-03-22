
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Syncless.Helper
{
    public static class Cleaner
    {
        public static int CleanSynclessMeta(DirectoryInfo info)
        {
            DirectoryInfo[] infos = info.GetDirectories("*", SearchOption.AllDirectories);
            int count = 0;
            foreach (DirectoryInfo i in infos)
            {
                if (i.Name.ToLower().Equals(".syncless"))
                {
                    try
                    {
                        i.Delete(true);
                        count++;
                    }
                    catch (IOException e)
                    {
                    }
                }
            }
            return count;
        }
    }
}
