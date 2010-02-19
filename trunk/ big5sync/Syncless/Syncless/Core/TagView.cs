using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Core
{
    public abstract class TagView
    {
        protected string _tagName;

        public string TagName
        {
            get { return _tagName; }
            set { this._tagName = value; }
        }
        private List<string> _pathStringList;
        public List<string> PathStringList
        {
            get { return _pathStringList; }
            set { this._pathStringList = value; }
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

        protected bool _isSeamless;

        public bool IsSeamless
        {
            get { return _isSeamless; }
            set { _isSeamless = value; }
        }

        public TagView(string tagname, long created)
        {
            this._tagName = tagname;
            this._created = created;
            this._lastupdated = created;
            this._isSeamless = false;
            this._pathStringList = new List<string> ();
        }
    }
}
