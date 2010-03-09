using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Core
{
    //Implemented by Any UI.
    public interface IUIInterface
    {
        string getAppPath();
        //I need this method if you can

        //void DriveChanged(); <--- inform you of a drive that is changed
        //void TagChanged(); <--- inform you of a change in one of the tag


    }
}
