using System.ComponentModel;
using System.Net;

namespace DaveyM69.Components.SNTP
{
    /// <summary>
    /// A class that holds the information needed to connect to a remote NTP/SNTP server.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class RemoteSNTPServer
    {
		#region Fields 

        private string _HostNameOrAddress;
        private int _Port;
        /// <summary>
        /// African RemoteSNTPServer.
        /// </summary>
        public static readonly RemoteSNTPServer Africa = new RemoteSNTPServer("africa.pool.ntp.org");
        /// <summary>
        /// Apple Europe.
        /// </summary>
        public static readonly RemoteSNTPServer AppleEurope = new RemoteSNTPServer("time1.euro.apple.com");
        /// <summary>
        /// Apple Europe (2).
        /// </summary>
        public static readonly RemoteSNTPServer AppleEurope2 = new RemoteSNTPServer("time.euro.apple.com");
        /// <summary>
        /// Asian RemoteSNTPServer.
        /// </summary>
        public static readonly RemoteSNTPServer Asia = new RemoteSNTPServer("asia.pool.ntp.org");
        /// <summary>
        /// An array of Asian RemoteSNTPServers.
        /// See http://www.pool.ntp.org
        /// </summary>
        public static readonly RemoteSNTPServer[] AsiaServers = new RemoteSNTPServer[]{
            new RemoteSNTPServer("0.asia.pool.ntp.org"),
            new RemoteSNTPServer("1.asia.pool.ntp.org"),
            new RemoteSNTPServer("2.asia.pool.ntp.org"),
            new RemoteSNTPServer("3.asia.pool.ntp.org"),
        };
        /// <summary>
        /// Australian RemoteSNTPServer.
        /// </summary>
        public static readonly RemoteSNTPServer Australia = new RemoteSNTPServer("au.pool.ntp.org");
        /// <summary>
        /// An array of Australian RemoteSNTPServers.
        /// See http://www.pool.ntp.org
        /// </summary>
        public static readonly RemoteSNTPServer[] AustraliaServers = new RemoteSNTPServer[]{
            new RemoteSNTPServer("0.au.pool.ntp.org"),
            new RemoteSNTPServer("1.au.pool.ntp.org"),
            new RemoteSNTPServer("2.au.pool.ntp.org"),
            new RemoteSNTPServer("3.au.pool.ntp.org"),
        };
        /// <summary>
        /// Blue Yonder UK (Virgin Media).
        /// [When I checked 12th July 2009, it was using USNavalObservatory3]
        /// </summary>
        public static readonly RemoteSNTPServer BlueYonder = new RemoteSNTPServer("ntp.blueyonder.co.uk");
        /// <summary>
        /// Canadian RemoteSNTPServer.
        /// </summary>
        public static readonly RemoteSNTPServer Canada = new RemoteSNTPServer("ca.pool.ntp.org");
        /// <summary>
        /// An array of Canadian RemoteSNTPServers.
        /// See http://www.pool.ntp.org
        /// </summary>
        public static readonly RemoteSNTPServer[] CanadaServers = new RemoteSNTPServer[]{
            new RemoteSNTPServer("0.ca.pool.ntp.org"),
            new RemoteSNTPServer("1.ca.pool.ntp.org"),
            new RemoteSNTPServer("2.ca.pool.ntp.org"),
            new RemoteSNTPServer("3.ca.pool.ntp.org"),
        };
        /// <summary>
        /// A remote NTP/SNTP server configured with the default values.
        /// </summary>
        public static readonly RemoteSNTPServer Default = new RemoteSNTPServer();
        /// <summary>
        /// The default server host name.
        /// </summary>
        public const string DefaultHostName = "time.nist.gov";
        /// <summary>
        /// The dafault port number for a NTP/SNTP server.
        /// </summary>
        public const int DefaultPort = 123;
        /// <summary>
        /// European RemoteSNTPServer.
        /// </summary>
        public static readonly RemoteSNTPServer Europe = new RemoteSNTPServer("europe.pool.ntp.org");
        /// <summary>
        /// An array of European RemoteSNTPServers.
        /// See http://www.pool.ntp.org
        /// </summary>
        public static readonly RemoteSNTPServer[] EuropeServers = new RemoteSNTPServer[]{
            new RemoteSNTPServer("0.europe.pool.ntp.org"),
            new RemoteSNTPServer("1.europe.pool.ntp.org"),
            new RemoteSNTPServer("2.europe.pool.ntp.org"),
            new RemoteSNTPServer("3.europe.pool.ntp.org"),
        };
        /// <summary>
        /// The Microsoft (Redmond, Washington) RemoteSNTPServer (time-nw.nist.gov).
        /// </summary>
        public static readonly RemoteSNTPServer Microsoft = new RemoteSNTPServer("time-nw.nist.gov");
        /// <summary>
        /// North American RemoteSNTPServer.
        /// </summary>
        public static readonly RemoteSNTPServer NorthAmerica = new RemoteSNTPServer("north-america.pool.ntp.org");
        /// <summary>
        /// An array of North American RemoteSNTPServers.
        /// See http://www.pool.ntp.org
        /// </summary>
        public static readonly RemoteSNTPServer[] NorthAmericaServers = new RemoteSNTPServer[]{
            new RemoteSNTPServer("0.north-america.pool.ntp.org"),
            new RemoteSNTPServer("1.north-america.pool.ntp.org"),
            new RemoteSNTPServer("2.north-america.pool.ntp.org"),
            new RemoteSNTPServer("3.north-america.pool.ntp.org"),
        };
        /// <summary>
        /// NTL UK (Virgin Media).
        /// </summary>
        public static readonly RemoteSNTPServer NTL = new RemoteSNTPServer("time.cableol.net");
        /// <summary>
        /// Pacific RemoteSNTPServer.
        /// </summary>
        public static readonly RemoteSNTPServer Oceania = new RemoteSNTPServer("oceania.pool.ntp.org");
        /// <summary>
        /// An array of Pacific RemoteSNTPServers.
        /// See http://www.pool.ntp.org
        /// </summary>
        public static readonly RemoteSNTPServer[] OceaniaServers = new RemoteSNTPServer[]{
            new RemoteSNTPServer("0.oceania.pool.ntp.org"),
            new RemoteSNTPServer("1.oceania.pool.ntp.org"),
            new RemoteSNTPServer("2.oceania.pool.ntp.org"),
            new RemoteSNTPServer("3.oceania.pool.ntp.org"),
        };
        /// <summary>
        /// General RemoteSNTPServer.
        /// </summary>
        public static readonly RemoteSNTPServer Pool = new RemoteSNTPServer("pool.ntp.org");
        /// <summary>
        /// An array of general RemoteSNTPServers.
        /// See http://www.pool.ntp.org
        /// </summary>
        public static readonly RemoteSNTPServer[] PoolServers = new RemoteSNTPServer[]{
            new RemoteSNTPServer("0.pool.ntp.org"),
            new RemoteSNTPServer("1.pool.ntp.org"),
            new RemoteSNTPServer("2.pool.ntp.org"),
        };
        /// <summary>
        /// South American RemoteSNTPServer.
        /// </summary>
        public static readonly RemoteSNTPServer SouthAmerica = new RemoteSNTPServer("south-america.pool.ntp.org");
        /// <summary>
        /// An array of South American RemoteSNTPServers.
        /// See http://www.pool.ntp.org
        /// </summary>
        public static readonly RemoteSNTPServer[] SouthAmericaServers = new RemoteSNTPServer[]{
            new RemoteSNTPServer("0.south-america.pool.ntp.org"),
            new RemoteSNTPServer("1.south-america.pool.ntp.org"),
            new RemoteSNTPServer("2.south-america.pool.ntp.org"),
            new RemoteSNTPServer("3.south-america.pool.ntp.org"),
        };
        /// <summary>
        /// A United Kindom GPS Primary server.
        /// </summary>
        public static readonly RemoteSNTPServer UKJanetGPS = new RemoteSNTPServer("ntp1.ja.net");
        /// <summary>
        /// United Kingdom RemoteSNTPServer.
        /// </summary>
        public static readonly RemoteSNTPServer UnitedKingdom = new RemoteSNTPServer("uk.pool.ntp.org");
        /// <summary>
        /// An array of UK RemoteSNTPServers.
        /// See http://www.pool.ntp.org
        /// </summary>
        public static readonly RemoteSNTPServer[] UnitedKingdomServers = new RemoteSNTPServer[]{
            new RemoteSNTPServer("0.uk.pool.ntp.org"),
            new RemoteSNTPServer("1.uk.pool.ntp.org"),
            new RemoteSNTPServer("2.uk.pool.ntp.org"),
            new RemoteSNTPServer("3.uk.pool.ntp.org"),
        };
        /// <summary>
        /// US RemoteSNTPServer.
        /// </summary>
        public static readonly RemoteSNTPServer UnitedStates = new RemoteSNTPServer("us.pool.ntp.org");
        /// <summary>
        /// An array of US RemoteSNTPServers.
        /// See http://www.pool.ntp.org
        /// </summary>
        public static readonly RemoteSNTPServer[] UnitedStatesServers = new RemoteSNTPServer[]{
            new RemoteSNTPServer("0.us.pool.ntp.org"),
            new RemoteSNTPServer("1.us.pool.ntp.org"),
            new RemoteSNTPServer("2.us.pool.ntp.org"),
            new RemoteSNTPServer("3.us.pool.ntp.org"),
        };
        /// <summary>
        /// U.S. Naval Observatory.
        /// </summary>
        public static readonly RemoteSNTPServer USNavalObservatory = new RemoteSNTPServer("tock.usno.navy.mil");
        /// <summary>
        /// U.S. Naval Observatory (2).
        /// </summary>
        public static readonly RemoteSNTPServer USNavalObservatory2 = new RemoteSNTPServer("tick.usno.navy.mil");
        /// <summary>
        /// U.S. Naval Observatory (3).
        /// </summary>
        public static readonly RemoteSNTPServer USNavalObservatory3 = new RemoteSNTPServer("ntp1.usno.navy.mil");
        /// <summary>
        /// A United States GPS Primary server.
        /// </summary>
        public static readonly RemoteSNTPServer USXMissionGPS = new RemoteSNTPServer("clock.xmission.com");
        /// <summary>
        /// The Microsoft Windows RemoteSNTPServer (time.windows.com).
        /// </summary>
        public static readonly RemoteSNTPServer Windows = new RemoteSNTPServer("time.windows.com");

		#endregion Fields 

		#region Constructors 

        /// <summary>
        /// Creates a new instance of a remote NTP/SNTP server.
        /// </summary>
        /// <param name="hostNameOrAddress">The host name or address of the server.</param>
        /// <param name="port">The port to use (normally 123).</param>
        public RemoteSNTPServer(string hostNameOrAddress, int port)
        {
            HostNameOrAddress = hostNameOrAddress;
            Port = port;
        }

        /// <summary>
        /// Creates a new instance of a remote NTP/SNTP server.
        /// </summary>
        /// <param name="hostNameOrAddress">The host name or address of the server.</param>
        public RemoteSNTPServer(string hostNameOrAddress)
            : this(hostNameOrAddress, DefaultPort)
        { }

        /// <summary>
        /// Creates a new instance of a remote NTP/SNTP server.
        /// </summary>
        public RemoteSNTPServer()
            : this(DefaultHostName, DefaultPort)
        { }

		#endregion Constructors 

		#region Properties 

        /// <summary>
        /// Gets or sets the host name or address of the server.
        /// </summary>
        [Description("The host name or address of the server."),
        DefaultValue(DefaultHostName),
        NotifyParentProperty(true)]
        public string HostNameOrAddress
        {
            get { return _HostNameOrAddress; }
            set
            {
                value = value.Trim();
                if (string.IsNullOrEmpty(value))
                    value = DefaultHostName;
                _HostNameOrAddress = value;
            }
        }

        /// <summary>
        /// Gets or sets the port number that this server uses.
        /// </summary>
        [Description("The port number that this server uses."),
        DefaultValue(DefaultPort),
        NotifyParentProperty(true)]
        public int Port
        {
            get { return _Port; }
            set
            {
                if (value >= 0 && value <= 65535)
                    _Port = value;
                else
                    _Port = DefaultPort;
            }
        }

		#endregion Properties 

		#region Methods

        /// <summary>
        /// Attempts to get the System.Net.IEPEndPoint of this server.
        /// </summary>
        /// <returns>The System.Net.IEPEndPoint of this server.</returns>
        public IPEndPoint GetIPEndPoint()
        {
            return new IPEndPoint(Dns.GetHostAddresses(HostNameOrAddress)[0], Port);
        }

        /// <summary>
        /// Returns the host name, IP address and port number of this server.
        /// </summary>
        /// <returns>The host name, IP address and port number of this server.</returns>
        public override string ToString()
        {
            return string.Format("{0}:{1}",
                HostNameOrAddress, Port);
        }

		#endregion Methods 
    }
}
