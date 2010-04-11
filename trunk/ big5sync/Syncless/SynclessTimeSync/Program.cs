// Source: http://www.codeproject.com/KB/datetime/SNTPClient.aspx
// Icon source: http://www.iconfinder.net/icondetails/10407/24/
// Icon source: http://www.webdesignerdepot.com/2009/03/200-free-exclusive-icons-siena/

using DaveyM69.Components;
using System.Threading;

namespace SynclessTimeSync
{
    public static class Program
    {
        private static int? _returnValue;
        private static bool _canWork;
        private static EventWaitHandle _wh = new ManualResetEvent(false);

        public static int Main()
        {
            SNTPClient sntpClient = new SNTPClient();
            sntpClient.UpdateLocalDateTime = true;
            sntpClient.Timeout = 3000;
            sntpClient.QueryServerCompleted += sntpClient_QueryServerCompleted;
            _canWork = sntpClient.QueryServerAsync();

            if (_canWork)
                _wh.WaitOne();
            else
                return -1;

            return (int)_returnValue;
        }

        public static void sntpClient_QueryServerCompleted(object sender, DaveyM69.Components.SNTP.QueryServerCompletedEventArgs e)
        {
            _returnValue = e.Succeeded && e.LocalDateTimeUpdated ? 0 : -1;
            if (_canWork)
                _wh.Set();
        }
    }
}
