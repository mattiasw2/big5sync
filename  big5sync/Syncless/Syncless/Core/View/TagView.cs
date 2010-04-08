using System.Collections.Generic;
using Syncless.Tagging;

namespace Syncless.Core.View
{
    /// <summary>
    /// The TagView for <see cref="Tag"/>
    /// </summary>
    public class TagView
    {
        /// <summary>
        /// Get and Set the name of the <see cref="Tag"/>
        /// </summary>
        public string TagName { get; set; }
        /// <summary>
        /// Get the list of Path Group
        /// </summary>
        public List<PathGroupView> GroupList { get; private set; }
        /// <summary>
        /// Get the list of Path in string.
        /// </summary>
        public List<string> PathStringList
        {
            get
            {
                List<string> pathList = new List<string>();
                foreach (PathGroupView grp in GroupList)
                {
                    pathList.AddRange(grp.PathListString);
                }
                return pathList;
            }
        }
        /// <summary>
        /// Get and Set the Last Updated Time
        /// </summary>
        public long LastUpdated { get; set; }
        /// <summary>
        /// Get and Set the Created Time
        /// </summary>
        public long Created { get; set; }
        /// <summary>
        /// Return if the <see cref="Tag"/> is Seamless
        /// </summary>
        public bool IsSeamless
        {
            get { return TagState == TagState.Seamless; }
        }
        /// <summary>
        /// Return true if the <see cref="Tag"/> is Queued
        /// </summary>
        public bool IsQueued { get; set; }
        /// <summary>
        /// Return true if the <see cref="Tag"/> is Synchronzing
        /// </summary>
        public bool IsSyncing { get; set; }
        /// <summary>
        /// Return true if the <see cref="Tag"/> is Queued or Synchronzing
        /// </summary>
        public bool IsLocked
        {
            get { return (IsQueued || IsSyncing); }
        }
        /// <summary>
        /// Return true if the <see cref="Tag"/> is Switching
        /// </summary>
        public bool IsSwitching{
            get
            {
                return (TagState == TagState.ManualToSeamless ||
                        TagState == TagState.SeamlessToManual);
            }
        }
        /// <summary>
        /// Get and Set the State of the Tag.
        /// </summary>
        public TagState TagState { get; set; }
        /// <summary>
        /// Constructor of Tag View
        /// </summary>
        /// <param name="tagname">Name of the tag</param>
        /// <param name="created">The Created date.</param>
        public TagView(string tagname, long created)
        {
            TagName = tagname;
            Created = created;
            LastUpdated = created;
            TagState = TagState.Seamless;
            GroupList = new List<PathGroupView>();
        }
        
    }
}
