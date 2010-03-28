using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Syncless.Notification.SLLNotification;

namespace Syncless.Core
{
    public class DeletedTaggedPathWatcher
    {
        private Thread _watcher;

        public DeletedTaggedPathWatcher()
        {
        }

        public void Start()
        {
            _watcher = new Thread(Run);
            _watcher.Start();
        }

        public void Stop()
        {
            _watcher.Abort();
        }

        private void Run()
        {
            while (true)
            {
                List<string> deletedPaths = SystemLogicLayer.Instance.FindAllDeletedPaths();
                if (deletedPaths.Count > 0)
                {
                    ServiceLocator.LogicLayerNotificationQueue().Enqueue(new TaggedPathDeletedNotification(deletedPaths));
                }

                Thread.Sleep(3000);
            }
        }
    }
}
