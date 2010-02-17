using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Tagging
{
    public class FolderTag : Tag
    {
        public FolderTag(string tagname, long lastupdated)
            : base(tagname, lastupdated)
        {

        }

        public string FindMatchedParentDirectory(string path)
        {
            string[] pathTokens = path.Split('\\');
            int count1 = pathTokens.Length;
            if (pathTokens[pathTokens.Length-1].Equals(""))
            {
                count1--;
            }
            string logicalid = pathTokens[0].TrimEnd(':');
            string trailingPath = null;
            foreach (TaggedPath p in _pathList)
            {
                if (p.LogicalDriveId.Equals(logicalid))
                {
                    string[] pTokens = p.Path.Split('\\');
                    int count2 = pTokens.Length;
                    if (pTokens[pTokens.Length - 1].Equals(""))
                    {
                        count2--;
                    }
                    if (count1 > count2)
                    {
                        int trailingIndex = Match(pathTokens, pTokens);
                        if (trailingIndex > 0)
                        {
                            for (int i = trailingIndex; i < pathTokens.Length - 1; i++)
                            {
                                trailingPath += (pathTokens[i] + "\\");
                            }
                            trailingPath += pathTokens[pathTokens.Length - 1];
                            return trailingPath;
                        }
                    }
                }
            }
            return null;
        }

        private int Match(string[] pathTokens, string[] pTokens)
        {
            int trailingIndex = 0;
            for (int i = 0; i < pTokens.Length; i++)
            {
                if (pTokens[i].Equals(pathTokens[i]))
                {
                    trailingIndex++;
                }
                else
                {
                    break;
                }
            }
            return trailingIndex;
        }
    }
}
