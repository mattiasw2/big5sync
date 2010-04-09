using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using DaveyM69.Components.Interop;
using DaveyM69.Components.SNTP;

namespace DaveyM69.Components
{
    /// <summary>
    /// A class for retrieving data from a NTP/SNTP server.
    /// See http://www.faqs.org/rfcs/rfc2030.html for full details of protocol.
    /// </summary>
    [DefaultEvent("QueryServerCompleted"),
    DefaultProperty("RemoteSNTPServer")]
    public class SNTPClient : Component
    {
        #region Fields

        private int _Timeout;
        private AsyncOperation asyncOperation = null;
        /// <summary>
        /// The server that is used by default.
        /// </summary>
        public static readonly RemoteSNTPServer DefaultServer = RemoteSNTPServer.Default;
        /// <summary>
        /// The default number of milliseconds used for send and receive.
        /// </summary>
        public const int DefaultTimeout = 5000;
        /// <summary>
        /// The default NTP/SNTP version number.
        /// </summary>
        public const VersionNumber DefaultVersionNumber = VersionNumber.Version3;
        private readonly SendOrPostCallback operationCompleted;
        private readonly WorkerThreadStartDelegate threadStart;

		#endregion Fields 

		#region Constructors

        /// <summary>
        /// Creates a new instance of SNTPClient.
        /// </summary>
        public SNTPClient()
        {
            Initialize();
            threadStart = new WorkerThreadStartDelegate(WorkerThreadStart);
            operationCompleted = new SendOrPostCallback(AsyncOperationCompleted);
            Timeout = DefaultTimeout;
            VersionNumber = DefaultVersionNumber;
            UpdateLocalDateTime = true;
        }

		#endregion Constructors 

		#region Properties 

        /// <summary>
        /// Gets whether the SNTPClient is busy.
        /// </summary>
        [Browsable(false)]
        public bool IsBusy
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the real local date and time using the default server and a total timeout of 1 second.
        /// If there is an error or exception, DateTime.MinValue is returned.
        /// (NB: This property getter is blocking)
        /// </summary>
        public static DateTime Now
        {
            get { return GetNow(); }
        }

        /// <summary>
        /// Gets or sets the server to use.
        /// </summary>
        [Description("The server to use."),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        Category("Connection")]
        public RemoteSNTPServer RemoteSNTPServer
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the timeout in milliseconds used for sending and receiving.
        /// </summary>
        [Description("The timeout in milliseconds used for sending and receiving.")]
        [DefaultValue(DefaultTimeout),
        Category("Connection")]
        public int Timeout
        {
            get{return _Timeout;}
            set
            {
                if (value < -1)
                    value = DefaultTimeout;
                _Timeout = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to update the local date and time to the date and time calculated by querying the server.
        /// </summary>
        [Description("Whether to update the local date and time to the date and time calculated by querying the server."),
        DefaultValue(true),
        Category("Actions")]
        public bool UpdateLocalDateTime
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the NTP/SNTP version to use.
        /// </summary>
        [Description("The NTP/SNTP version to use.")]
        [DefaultValue(DefaultVersionNumber),
        Category("Connection")]
        public VersionNumber VersionNumber
        {
            get;
            set;
        }

		#endregion Properties 

		#region Delegates and Events 

		// Delegates

        private delegate void WorkerThreadStartDelegate();
		// Events 

        /// <summary>
        /// Raised when a query to the server completes successfully.
        /// </summary>
        [Description("Raised when a query to the server completes successfully."),
        Category("Success")]
        public event EventHandler<QueryServerCompletedEventArgs> QueryServerCompleted;

		#endregion Delegates and Events 

		#region Methods 

		// Public Methods

        /// <summary>
        /// Calculates the current local time zone offset from UTC.
        /// </summary>
        /// <returns>A TimeSpan that is the current local time zone offset from UTC.</returns>
        public static TimeSpan GetCurrentLocalTimeZoneOffset()
        {
            return TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
        }

        /// <summary>
        /// Gets the real local date and time using the default server and a total timeout of 1 second.
        /// If there is an error or exception, DateTime.MinValue is returned.
        /// </summary>
        /// <returns>The real local date and time.</returns>
        public static DateTime GetNow()
        {
            return GetNow(RemoteSNTPServer.Default, 500);
        }

        /// <summary>
        /// Gets the real local date and time using the specified server and a total timeout of 1 second.
        /// If there is an error or exception, DateTime.MinValue is returned.
        /// </summary>
        /// <param name="remoteSNTPServer">The server to use.</param>
        /// <returns>The real local date and time.</returns>
        public static DateTime GetNow(RemoteSNTPServer remoteSNTPServer)
        {
            return GetNow(remoteSNTPServer, 500);
        }

        /// <summary>
        /// Gets the real local date and time using the default server and the specified timeout.
        /// If there is an error or exception, DateTime.MinValue is returned.
        /// </summary>
        /// <param name="timeout">The timeout in milliseconds used for sending and receiving.</param>
        /// <returns>The real local date and time.</returns>
        public static DateTime GetNow(int timeout)
        {
            return GetNow(RemoteSNTPServer.Default, timeout);
        }

        /// <summary>
        /// Gets the real local date and time using the default server and the specified timeout.
        /// If there is an error or exception, DateTime.MinValue is returned.
        /// </summary>
        /// <param name="remoteSNTPServer">The server to use.</param>
        /// <param name="timeout">The timeout in milliseconds used for sending and receiving.</param>
        /// <returns>The real local date and time.</returns>
        public static DateTime GetNow(RemoteSNTPServer remoteSNTPServer, int timeout)
        {
            SNTPClient sntpClient = new SNTPClient();
            sntpClient.UpdateLocalDateTime = false;
            sntpClient.RemoteSNTPServer = remoteSNTPServer;
            sntpClient.Timeout = timeout;
            QueryServerCompletedEventArgs args = sntpClient.QueryServer();
            if (args.Succeeded)
                return DateTime.Now.AddSeconds(args.Data.LocalClockOffset);
            else
                return DateTime.MinValue;
        }

        /// <summary>
        /// Queries the specified server on a separate thread.
        /// </summary>
        /// <returns>true if the SNTPClient wasn't busy, otherwise false.</returns>
        public bool QueryServerAsync()
        {
            bool result = false;
            if (!IsBusy)
            {
                IsBusy = true;
                asyncOperation = AsyncOperationManager.CreateOperation(null);
                threadStart.BeginInvoke(null, null);
                result = true;
            }
            return result;
        }

		// Protected Methods 

        /// <summary>
        /// Raises the QueryServerCompleted event.
        /// </summary>
        /// <param name="e">A QueryServerCompletedEventArgs instance.</param>
        protected virtual void OnQueryServerCompleted(QueryServerCompletedEventArgs e)
        {
            EventHandler<QueryServerCompletedEventArgs> eh = QueryServerCompleted;
            if (eh != null)
                eh(this, e);
        }

		// Private Methods 

        private void AsyncOperationCompleted(object arg)
        {
            IsBusy = false;
            OnQueryServerCompleted((QueryServerCompletedEventArgs)arg);
        }

        private void Initialize()
        {
            if (RemoteSNTPServer == null)
                RemoteSNTPServer = DefaultServer;
        }
        
        /// <summary>
        /// This is the 'nuts and bolts' method that queries the server.
        /// </summary>
        /// <returns>A QueryServerResults instance that holds the results of the query.</returns>
        private QueryServerCompletedEventArgs QueryServer()
        {
            QueryServerCompletedEventArgs result = new QueryServerCompletedEventArgs();
            Initialize();
            UdpClient client = null;
            try
            {
                // Configure and connect the socket.
                client = new UdpClient();
                IPEndPoint ipEndPoint = RemoteSNTPServer.GetIPEndPoint();
                client.Client.SendTimeout = Timeout;
                client.Client.ReceiveTimeout = Timeout;
                client.Connect(ipEndPoint);

                // Send and receive the data, and save the completion DateTime.
                SNTPData request = SNTPData.GetClientRequestPacket(VersionNumber);
                client.Send(request, request.Length);
                result.Data = client.Receive(ref ipEndPoint);
                result.Data.DestinationDateTime = DateTime.Now.ToUniversalTime();

                // Check the data
                if (result.Data.Mode == Mode.Server)
                {
                    result.Succeeded = true;

                    // Call other method(s) if needed
                    if (UpdateLocalDateTime)
                    {
                        UpdateTime(result.Data.LocalClockOffset);
                        result.LocalDateTimeUpdated = true;
                    }
                }
                else
                {
                    result.ErrorData = new ErrorData("The response from the server was invalid.");
                }
                return result;
            }
            catch (Exception ex)
            {
                result.ErrorData = new ErrorData(ex);
                return result;
            }
            finally
            {
                // Close the socket
                if (client != null)
                    client.Close();
            }
        }

        private void UpdateTime(double localClockOffset)
        {
            SYSTEMTIME systemTime;
            DateTime newDateTime = DateTime.Now.AddSeconds(localClockOffset);
            systemTime.wYear = (UInt16)newDateTime.Year;
            systemTime.wMonth = (UInt16)newDateTime.Month;
            systemTime.wDayOfWeek = (UInt16)newDateTime.DayOfWeek;
            systemTime.wDay = (UInt16)newDateTime.Day;
            systemTime.wHour = (UInt16)newDateTime.Hour;
            systemTime.wMinute = (UInt16)newDateTime.Minute;
            systemTime.wSecond = (UInt16)newDateTime.Second;
            systemTime.wMilliseconds = (UInt16)newDateTime.Millisecond;
            if (!Functions.SetLocalTime(ref systemTime))
                throw new Win32Exception();
        }

        private void WorkerThreadStart()
        {
            lock (this)
            {
                QueryServerCompletedEventArgs e = null;
                try
                {
                    e = QueryServer();
                }
                catch (Exception exception)
                {
                    throw exception;
                }
                asyncOperation.PostOperationCompleted(operationCompleted, e);
            }
        }

		#endregion Methods 
    }
}
