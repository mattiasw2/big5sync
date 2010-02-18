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
            string[] tempPathTokens = path.Split('\\');
            string[] pathTokens = TrimEnd(tempPathTokens);
            string logicalid = pathTokens[0].TrimEnd(':');
            string trailingPath = null;
            foreach (TaggedPath p in _pathList)
            {
                if (p.LogicalDriveId.Equals(logicalid))
                {
                    string[] tempPTokens = p.Path.Split('\\');
                    string[] pTokens = TrimEnd(tempPTokens);
                    if (pathTokens.Length > pTokens.Length)
                    {
                        if (CheckMatch(pathTokens, pTokens))
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
            }
            return null;
        }

        private string[] TrimEnd(string[] tempPathTokens)
        {
            string[] pathTokens = new string[tempPathTokens.Length-1];
            if (tempPathTokens[tempPathTokens.Length - 1].Equals(""))
            {
                for (int i = 0; i < pathTokens.Length; i++)
                {
                    pathTokens[i] = tempPathTokens[i];
                }
                return pathTokens;
            }
            return tempPathTokens;
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

        private bool CheckMatch(string[] pathTokens, string[] pTokens)
        {
            bool allMatched = true;
            for (int i = 0; i < pTokens.Length; i++)
            {
                if (!pTokens[i].Equals(pathTokens[i]))
                {
                    allMatched = false;
                    break;
                }
            }
            return allMatched;
        }
    }
}
