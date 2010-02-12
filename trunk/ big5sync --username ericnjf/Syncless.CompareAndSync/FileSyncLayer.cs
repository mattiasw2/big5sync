using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.CompareAndSync
{
    public class FileSyncLayer
    {
        private static FileSyncLayer _instance;
        public static FileSyncLayer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FileSyncLayer();
                }
                return _instance;
            }
        }
        private FileSyncLayer()
        {

        }

        
    }
}
