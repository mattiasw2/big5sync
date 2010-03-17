
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Syncless.Helper
{
    public static class Cleaner
    {
        public static void CleanSynclessMeta(DirectoryInfo info)
        {
            DirectoryInfo[] infos = info.GetDirectories("*", SearchOption.AllDirectories);
            foreach (DirectoryInfo i in infos)
            {
                if (i.Name.ToLower().Equals(".syncless"))
                {
                    i.Delete();
                }
            }
        }
    }
}
