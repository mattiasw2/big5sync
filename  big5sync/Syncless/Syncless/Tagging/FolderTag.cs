using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Filters;

namespace Syncless.Tagging
{
    public class FolderTag : Tag
    {
        private List<Filter> _filters;

        public List<Filter> Filters
        {
            get { return _filters; }
        }
        
        public FolderTag(string tagname, long lastupdated)
            : base(tagname, lastupdated)
        {
            _filters = new List<Filter>();
        }

        public string FindMatchedParentDirectory(string path, bool isFolder)
        {
            string[] pathTokens = TaggingHelper.TrimEnd(path.Split('\\'));
            string logicalid = TaggingHelper.GetLogicalID(path);
            foreach (TaggedPath p in _pathList)
            {
                if (path.StartsWith(p.Path))
                {
                    if (!path.Equals(p.Path))
                    {
                        string[] pTokens = TaggingHelper.TrimEnd(p.Path.Split('\\'));
                        int trailingIndex = TaggingHelper.Match(pathTokens, pTokens);
                        if (trailingIndex > 0)
                        {
                            return TaggingHelper.CreatePath(trailingIndex, pathTokens, isFolder);
                        }
                    }
                }
            }
            return null;
        }

        public void AddFilter(Filter filter)
        {
            if (!_filters.Contains(filter))
            {
                _filters.Add(filter);
            }
        }

        public Filter RemoveFilter(Filter filter)
        {
            if (_filters.Contains(filter))
            {
                _filters.Remove(filter);
                return filter;
            }
            else
            {
                return null;
            }
        }
    }
}
