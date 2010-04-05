using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Core.View;

namespace Syncless.Core
{
    //Implemented by Any UI.
    public interface IUIInterface
    {
        string getAppPath();
        
        void DriveChanged(); // <--- inform you of a drive that is changed

        void TagChanged(string tagName); // <--- inform you of a change in the TagView

        void TagsChanged(); // <-- inform you of changes in the tags changed

        void PathChanged();
    }
}
