using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Core.View;

namespace Syncless.Core.View
{
    public class TagView
    {
        protected string _tagName;

        public string TagName
        {
            get { return _tagName; }
            set { this._tagName = value; }
        }
        
        private List<PathGroupView> _groupList;

        public List<PathGroupView> GroupList
        {
            get { return _groupList; }
            set { _groupList = value; }
        } 
        public List<string> PathStringList
        {
            get
            {
                List<string> pathList = new List<string>();
                foreach (PathGroupView grp in _groupList)
                {
                    pathList.AddRange(grp.PathListString);
                }
                return pathList;
            }
        }

        protected long _lastupdated;

        public long LastUpdated
        {
            get { return _lastupdated; }
            set { _lastupdated = value; }
        }

        protected long _created;

        public long Created
        {
            get { return _created; }
            set { _created = value; }
        }

        private TagState _tagState;

        public bool IsSeamless
        {
            get { return TagState == TagState.Seamless; }
        }

        private bool isQueued;

        public bool IsQueued
        {
            get { return isQueued; }
            set { isQueued = value; }
        }
        private bool isSyncing;

        public bool IsSyncing
        {
            get { return isSyncing; }
            set { isSyncing = value; }
        }       

        public bool IsLocked
        {
            get { return (isQueued || isSyncing); }
        }
        
        
        
        public TagState TagState
        {
            get { return _tagState; }
            set { _tagState = value; }
        }

        public TagView(string tagname, long created)
        {
            this._tagName = tagname;
            this._created = created;
            this._lastupdated = created;
            this._tagState = TagState.Seamless;
            this._groupList = new List<PathGroupView>();
        }
        
    }
}
