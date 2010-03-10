using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.CompareAndSync.CompareObject;
using Syncless.CompareAndSync.Request;

namespace Syncless.CompareAndSync
{
    public class CompareAndSyncController
    {
        private CompareAndSyncController _instance;

        public CompareAndSyncController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CompareAndSyncController();
                }
                return _instance;
            }
        }

        private CompareAndSyncController()
        {

        }

        public RootCompareObject Sync(ManualSyncRequest request)
        {
            return null;
        }

        public RootCompareObject Compare(ManualCompareRequest request)
        {
            return null;
        }

        public RootCompareObject Sync(AutoSyncRequest request)
        {
            return null;
        }


    }
}
