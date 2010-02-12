using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.CompareAndSync
{
    public class FileCompareLayer
    {
        private static FileCompareLayer _instance;
        public static FileCompareLayer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FileCompareLayer();
                }
                return _instance;
            }
        }
        private FileCompareLayer()
        {

        }
    }
}
