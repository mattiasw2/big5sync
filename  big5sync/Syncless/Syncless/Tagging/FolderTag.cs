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
            string[] pathTokens = TrimEnd(path.Split('\\'));
            string logicalid = pathTokens[0].TrimEnd(':');
            string trailingPath = null;
            foreach (TaggedPath p in _pathList)
            {
                if (p.LogicalDriveId.Equals(logicalid))
                {
                    string[] pTokens = TrimEnd(p.Path.Split('\\'));
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
    }
}
