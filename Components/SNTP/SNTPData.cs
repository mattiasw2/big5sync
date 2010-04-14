using System;
using System.Collections.Generic;

namespace DaveyM69.Components.SNTP
{
    /// <summary>
    /// A class that represents a SNTP packet.
    /// See http://www.faqs.org/rfcs/rfc2030.html for full details of protocol.
    /// </summary>
    public class SNTPData
    {
		#region Fields

        private byte[] data;
        private static readonly DateTime Epoch = new DateTime(1900, 1, 1);
        private const int LeapIndicatorLength = 4;
        private const byte LeapIndicatorMask = 0xC0;
        private const byte LeapIndicatorOffset = 6;
        /// <summary>
        /// The maximum number of bytes in a SNTP packet.
        /// </summary>
        public const int MaximumLength = 68;
        /// <summary>
        /// The minimum number of bytes in a SNTP packet.
        /// </summary>
        public const int MinimumLength = 48;
        private const byte ModeComplementMask = 0xF8;
        private const int ModeLength = 8;
        private const byte ModeMask = 0x07;
        private const int originateIndex = 24;
        private const int receiveIndex = 32;
        private const int referenceIdentifierOffset = 12;
        private const int referenceIndex = 16;
        private const int StratumLength = 16;
        /// <summary>
        /// Represents the number of ticks in 1 second.
        /// </summary>
        public const long TicksPerSecond = TimeSpan.TicksPerSecond;
        private const int transmitIndex = 40;
        private const byte VersionNumberComplementMask = 0xC7;
        private const int VersionNumberLength = 8;
        private const byte VersionNumberMask = 0x38;
        private const byte VersionNumberOffset = 3;

		#endregion Fields 

		#region Constructors 

        internal SNTPData(byte[] bytearray)
        {
            if (bytearray.Length >= MinimumLength && bytearray.Length <= MaximumLength)
                data = bytearray;
            else
                throw new ArgumentOutOfRangeException(
                    "Byte Array",
                    string.Format(
                    "Byte array must have a length between {0} and {1}.",
                    MinimumLength, MaximumLength));
        }

        internal SNTPData()
            : this(new byte[48])
        { }

		#endregion Constructors 

		#region Properties

        /// <summary>
        /// Gets the DateTime (UTC) when the data arrived from the server.
        /// </summary>
        public DateTime DestinationDateTime
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets a warning of an impending leap second to be inserted/deleted in the last minute of the current day.
        /// </summary>
        public LeapIndicator LeapIndicator
        {
            get { return (LeapIndicator)LeapIndicatorValue; }
        }

        /// <summary>
        /// Gets a textual representation of the LeapIndicator property.
        /// </summary>
        public string LeapIndicatorText
        {
            get
            {
                string result;
                LeapIndicatorDictionary.TryGetValue(LeapIndicator, out result);
                return result;
            }
        }

        private byte LeapIndicatorValue
        {
            get { return (byte)((data[0] & LeapIndicatorMask) >> LeapIndicatorOffset); }
        }

        /// <summary>
        /// Gets the number of bytes in the packet.
        /// </summary>
        public int Length
        {
            get { return data.Length; }
        }

        /// <summary>
        /// Gets the difference in seconds between the local time and the time retrieved from the server.
        /// </summary>
        public double LocalClockOffset
        {
            get
            {
                return ((double)((ReceiveDateTime.Ticks - OriginateDateTime.Ticks) + 
                    (TransmitDateTime.Ticks - DestinationDateTime.Ticks)) / 2) / TicksPerSecond;
            }
        }

        /// <summary>
        /// Gets the operating mode of whatever last altered the packet.
        /// </summary>
        public Mode Mode
        {
            get { return (Mode)ModeValue; }
            private set { ModeValue = (byte)value; }
        }

        /// <summary>
        /// Gets a textual representation of the Mode property.
        /// </summary>
        public string ModeText
        {
            get
            {
                string result;
                ModeDictionary.TryGetValue(Mode, out result);
                return result;
            }
        }

        private byte ModeValue
        {
            get { return (byte)(data[0] & ModeMask); }
            set { data[0] = (byte)((data[0] & ModeComplementMask) | value); }
        }

        /// <summary>
        /// Gets the DateTime (UTC) at which the request departed the client for the server.
        /// </summary>
        public DateTime OriginateDateTime
        {
            get { return TimestampToDateTime(originateIndex); }
        }

        /// <summary>
        /// Gets the maximum interval between successive messages, in seconds.
        /// </summary>
        public double PollInterval
        {
            get { return Math.Pow(2, (sbyte)data[2]); }
        }

        /// <summary>
        /// Gets the precision of the clock, in seconds.
        /// </summary>
        public double Precision
        {
            get { return Math.Pow(2, (sbyte)data[3]); }
        }

        /// <summary>
        /// Gets the DateTime (UTC) at which the request arrived at the server.
        /// </summary>
        public DateTime ReceiveDateTime
        {
            get { return TimestampToDateTime(receiveIndex); }
        }

        /// <summary>
        /// Gets the DateTime (UTC) at which the clock was last set or corrected.
        /// </summary>
        public DateTime ReferenceDateTime
        {
            get { return TimestampToDateTime(referenceIndex); }
        }

        /// <summary>
        /// Gets the identifier of the reference source.
        /// </summary>
        public string ReferenceIdentifier
        {
            get
            {
                string result = null;
                switch (Stratum)
                {
                    case Stratum.Unspecified:
                    case Stratum.Primary:
                        UInt32 id = 0;
                        for (int i = 0; i <= 3; i++)
                            id = (id << 8) | data[referenceIdentifierOffset + i];
                        if (!RefererenceIdentifierDictionary.TryGetValue(((ReferenceIdentifier)id), out result))
                        {
                            result = string.Format("{0}{1}{2}{3}",
                                (char)data[referenceIdentifierOffset],
                                (char)data[referenceIdentifierOffset + 1],
                                (char)data[referenceIdentifierOffset + 2],
                                (char)data[referenceIdentifierOffset + 3]);
                        }
                        break;
                    case Stratum.Secondary:
                    case Stratum.Secondary3:
                    case Stratum.Secondary4:
                    case Stratum.Secondary5:
                    case Stratum.Secondary6:
                    case Stratum.Secondary7:
                    case Stratum.Secondary8:
                    case Stratum.Secondary9:
                    case Stratum.Secondary10:
                    case Stratum.Secondary11:
                    case Stratum.Secondary12:
                    case Stratum.Secondary13:
                    case Stratum.Secondary14:
                    case Stratum.Secondary15:
                        switch (VersionNumber)
                        {
                            case VersionNumber.Version3:
                                result = string.Format("{0}.{1}.{2}.{3}",
                                    data[referenceIdentifierOffset],
                                    data[referenceIdentifierOffset + 1],
                                    data[referenceIdentifierOffset + 2],
                                    data[referenceIdentifierOffset + 3]);
                                break;
                            // The code below works with the Version 4 spec, but many servers respond as v4 but fill this as v3.
                            case VersionNumber.Version4:
                                // result = Timestamp32ToDateTime(referenceIdentifierOffset).ToString();
                                break;
                            default:
                                if (VersionNumber < VersionNumber.Version3)
                                {
                                    result = string.Format("{0}.{1}.{2}.{3}",
                                    data[referenceIdentifierOffset],
                                    data[referenceIdentifierOffset + 1],
                                    data[referenceIdentifierOffset + 2],
                                    data[referenceIdentifierOffset + 3]);
                                }
                                else
                                {
                                    // For future
                                }
                                break;
                        }
                        break;
                    default:
                        break;
                }
                return result;
            }
        }

        /// <summary>
        /// Gets the total delay to the primary reference source, in seconds.
        /// </summary>
        public double RootDelay
        {
            get { return SecondsStampToSeconds(4); }
        }

        /// <summary>
        /// Gets the nominal error relative to the primary reference source, in seconds.
        /// </summary>
        public double RootDispersion
        {
            get { return SecondsStampToSeconds(8); }
        }

        /// <summary>
        /// Gets the total roundtrip delay, in seconds.
        /// </summary>
        public double RoundTripDelay
        {
            get
            {
                return (double)((DestinationDateTime.Ticks - OriginateDateTime.Ticks)
                    - (ReceiveDateTime.Ticks - TransmitDateTime.Ticks)) / TicksPerSecond;
            }
        }

        /// <summary>
        /// Gets the stratum level of the clock.
        /// </summary>
        public Stratum Stratum
        {
            get { return (Stratum)StratumValue; }
        }

        /// <summary>
        /// Gets a textual representation of the Stratum property.
        /// </summary>
        public string StratumText
        {
            get
            {
                string result;
                if (!StratumDictionary.TryGetValue(Stratum, out result))
                    result = "Reserved";
                return result;
            }
        }

        private byte StratumValue
        {
            get { return data[1]; }
        }

        /// <summary>
        /// Gets the DateTime (UTC) at which the reply departed the server for the client.
        /// </summary>
        public DateTime TransmitDateTime
        {
            get { return TimestampToDateTime(transmitIndex); }
            private set { DateTimeToTimestamp(value, transmitIndex); }
        }

        /// <summary>
        /// Gets the NTP/SNTP version number.
        /// </summary>
        public VersionNumber VersionNumber
        {
            get { return (VersionNumber)VersionNumberValue; }
            private set { VersionNumberValue = (byte)value; }
        }

        /// <summary>
        /// Gets a textual representation of the VersionNumber property.
        /// </summary>
        public string VersionNumberText
        {
            get
            {
                string result;
                if (!VersionNumberDictionary.TryGetValue(VersionNumber, out result))
                    result = "Unknown";
                return result;
            }
        }

        private byte VersionNumberValue
        {
            get { return (byte)((data[0] & VersionNumberMask) >> VersionNumberOffset); }
            set { data[0] = (byte)((data[0] & VersionNumberComplementMask) | (value << VersionNumberOffset)); }
        }

		#endregion Properties 

		#region Methods

		// Private Methods 

        /// <summary>
        /// Converts a DateTime into a byte array and stores it starting at the position specifed.
        /// </summary>
        /// <param name="dateTime">The DateTime to convert.</param>
        /// <param name="startIndex">The index in the data at which to start.</param>
        private void DateTimeToTimestamp(DateTime dateTime, int startIndex)
        {
            UInt64 ticks = (UInt64)(dateTime - Epoch).Ticks;
            UInt64 seconds = ticks / TicksPerSecond;
            UInt64 fractions = ((ticks % TicksPerSecond) * 0x100000000L) / TicksPerSecond;
            for (int i = 3; i >= 0; i--)
            {
                data[startIndex + i] = (byte)seconds;
                seconds = seconds >> 8;
            }
            for (int i = 7; i >= 4; i--)
            {
                data[startIndex + i] = (byte)fractions;
                fractions = fractions >> 8;
            }
        }

        /// <summary>
        /// Converts a 32bit seconds (16 integer part, 16 fractional part) into a double that represents the value in seconds.
        /// </summary>
        /// <param name="startIndex">The index in the data at which to start.</param>
        /// <returns>A double that represents the value in seconds</returns>
        private double SecondsStampToSeconds(int startIndex)
        {
            UInt64 seconds = 0;
            for (int i = 0; i <= 1; i++)
                seconds = (seconds << 8) | data[startIndex + i];
            UInt64 fractions = 0;
            for (int i = 2; i <= 3; i++)
                fractions = (fractions << 8) | data[startIndex + i];
            UInt64 ticks = (seconds * TicksPerSecond) + ((fractions * TicksPerSecond) / 0x10000L);
            return (double)ticks / TicksPerSecond;
        }

        private DateTime Timestamp32ToDateTime(int startIndex)
        {
            UInt64 seconds = 0;
            for (int i = 0; i <= 3; i++)
                seconds = (seconds << 8) | data[startIndex + i];
            UInt64 ticks = (seconds * TicksPerSecond);
            return Epoch + TimeSpan.FromTicks((Int64)ticks);
        }

        /// <summary>
        /// Converts a byte array starting at the position specified into a DateTime.
        /// </summary>
        /// <param name="startIndex">The index in the data at which to start.</param>
        /// <returns>A DateTime converted from a byte array starting at the position specified.</returns>
        private DateTime TimestampToDateTime(int startIndex)
        {
            UInt64 seconds = 0;
            for (int i = 0; i <= 3; i++)
                seconds = (seconds << 8) | data[startIndex + i];
            UInt64 fractions = 0;
            for (int i = 4; i <= 7; i++)
                fractions = (fractions << 8) | data[startIndex + i];
            UInt64 ticks = (seconds * TicksPerSecond) + ((fractions * TicksPerSecond) / 0x100000000L);
            return Epoch + TimeSpan.FromTicks((Int64)ticks);
        }
		// Internal Methods 

        /// <summary>
        /// A SNTPData that is used by a client to send to a server to request data.
        /// </summary>
        internal static SNTPData GetClientRequestPacket(VersionNumber versionNumber)
        {
            SNTPData packet = new SNTPData();
            packet.Mode = Mode.Client;
            packet.VersionNumber = versionNumber;
            packet.TransmitDateTime = DateTime.Now.ToUniversalTime();
            return packet;
        }

		#endregion Methods 

        #region Conversion Operators

        public static implicit operator SNTPData(byte[] byteArray)
        {
            return new SNTPData(byteArray);
        }

        public static implicit operator byte[](SNTPData sntpPacket)
        {
            return sntpPacket.data;
        }

        #endregion Conversion Operators

        #region Dictionaries

        private static readonly Dictionary<LeapIndicator, string> LeapIndicatorDictionary = new Dictionary<LeapIndicator, string>()
        {
            {LeapIndicator.NoWarning, "No warning"},
            {LeapIndicator.LastMinute61Seconds, "Last minute has 61 seconds"},
            {LeapIndicator.LastMinute59Seconds, "Last minute has 59 seconds"},
            {LeapIndicator.Alarm, "Alarm condition (clock not synchronized)"},
        };

        private static readonly Dictionary<VersionNumber, string> VersionNumberDictionary = new Dictionary<VersionNumber, string>()
        {
            //{VersionNumber.Version0, "Version 0 (obselete)"},
            //{VersionNumber.Version1, "Version 1 (obselete)"},
            //{VersionNumber.Version2, "Version 2 (obselete)"},
            {VersionNumber.Version3, "Version 3 (IPv4 only)"},
            {VersionNumber.Version4, "Version 4 (IPv4, IPv6 and OSI)"}
        };

        private static readonly Dictionary<Mode, string> ModeDictionary = new Dictionary<Mode, string>()
        {
            {Mode.Reserved, "Reserved"},
            {Mode.SymmetricActive, "Symmetric active"},
            {Mode.SymmetricPassive, "Symmetric passive"},
            {Mode.Client, "Client"},
            {Mode.Server, "Server"},
            {Mode.Broadcast, "Broadcast"},
            {Mode.ReservedNTPControl, "Reserved for NTP control message"},
            {Mode.ReservedPrivate, "Reserved for private use"},
        };

        private static readonly Dictionary<Stratum, string> StratumDictionary = new Dictionary<Stratum, string>()
        {
            {Stratum.Primary, "1, Primary reference (e.g. radio clock)"},
            {Stratum.Secondary, "2, Secondary reference (via NTP or SNTP)"},
            {Stratum.Secondary3, "3, Secondary reference (via NTP or SNTP)"},
            {Stratum.Secondary4, "4, Secondary reference (via NTP or SNTP)"},
            {Stratum.Secondary5, "5, Secondary reference (via NTP or SNTP)"},
            {Stratum.Secondary6, "6, Secondary reference (via NTP or SNTP)"},
            {Stratum.Secondary7, "7, Secondary reference (via NTP or SNTP)"},
            {Stratum.Secondary8, "8, Secondary reference (via NTP or SNTP)"},
            {Stratum.Secondary9, "9, Secondary reference (via NTP or SNTP)"},
            {Stratum.Secondary10, "10, Secondary reference (via NTP or SNTP)"},
            {Stratum.Secondary11, "11, Secondary reference (via NTP or SNTP)"},
            {Stratum.Secondary12, "12, Secondary reference (via NTP or SNTP)"},
            {Stratum.Secondary13, "13, Secondary reference (via NTP or SNTP)"},
            {Stratum.Secondary14, "14, Secondary reference (via NTP or SNTP)"},
            {Stratum.Secondary15, "15, Secondary reference (via NTP or SNTP)"},
            {Stratum.Unspecified, "Unspecified or unavailable"}
        };

        private static readonly Dictionary<ReferenceIdentifier, string> RefererenceIdentifierDictionary = new Dictionary<ReferenceIdentifier, string>()
        {
            {SNTP.ReferenceIdentifier.ACTS, "NIST dialup modem service"},
            {SNTP.ReferenceIdentifier.CHU, "Ottawa (Canada) Radio 3330, 7335, 14670 kHz"},
            {SNTP.ReferenceIdentifier.DCF, "Mainflingen (Germany) Radio 77.5 kHz"},
            {SNTP.ReferenceIdentifier.GOES, "Geostationary Orbit Environment Satellite"},
            {SNTP.ReferenceIdentifier.GPS, "Global Positioning Service"},
            {SNTP.ReferenceIdentifier.LOCL, "Uncalibrated local clock used as a primary reference for a subnet without external means of synchronization"},
            {SNTP.ReferenceIdentifier.LORC, "LORAN-C radionavigation system"},
            {SNTP.ReferenceIdentifier.MSF, "Rugby (UK) Radio 60 kHz"},
            {SNTP.ReferenceIdentifier.OMEG, "OMEGA radionavigation system"},
            {SNTP.ReferenceIdentifier.PPS, "Atomic clock or other pulse-per-second source individually calibrated to national standards"},
            {SNTP.ReferenceIdentifier.PTB, "PTB (Germany) modem service"},
            {SNTP.ReferenceIdentifier.TDF, "Allouis (France) Radio 164 kHz"},
            {SNTP.ReferenceIdentifier.USNO, "U.S. Naval Observatory modem service"},
            {SNTP.ReferenceIdentifier.WWV, "Ft. Collins (US) Radio 2.5, 5, 10, 15, 20 MHz"},
            {SNTP.ReferenceIdentifier.WWVB, "Boulder (US) Radio 60 kHz"},
            {SNTP.ReferenceIdentifier.WWVH, "Kaui Hawaii (US) Radio 2.5, 5, 10, 15 MHz"},
        };

        #endregion Dictionaries
    }
}
