namespace AaronLuna.Common.Network
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using Enums;
    using Extensions;
    using Http;
    using Result;

    public static class NetworkUtilities
    {
        public const string CidrPrivateAddressBlockA = "10.0.0.0/8";
        public const string CidrPrivateAddressBlockB = "172.16.0.0/12";
        public const string CidrPrivateAddressBlockC = "192.168.0.0/16";

        public enum AddressType
        {
            Private,
            Public
        }

        public static async Task<Result<IPAddress>> GetPublicIPv4AddressAsync(int maxAttempts)
        {
            var attemptCounter = 1;
            var getPublicIpSucceeded = false;
            var publicIp = IPAddress.None;

            while (!getPublicIpSucceeded && attemptCounter <= maxAttempts)
            {
                var getPublicIpResult = await GetPublicIPv4AddressAsync().ConfigureAwait(false);
                if (getPublicIpResult.Success)
                {
                    publicIp = getPublicIpResult.Value;
                }

                if (publicIp.IsEqualTo(IPAddress.None))
                {
                    attemptCounter++;
                    continue;
                }

                getPublicIpSucceeded = true;
            }

            return getPublicIpSucceeded
                ? Result.Ok(publicIp)
                : Result.Fail<IPAddress>(
                    $"Unable to determine public IP address after {maxAttempts} unsuccessful attempts");
        }

        public static async Task<Result<IPAddress>> GetPublicIPv4AddressAsync()
        {
            Result<string> getUrlResult;

            var getUrlTask = Task.Run(() => HttpHelper.GetUrlContentAsStringAsync("http://ipv4.icanhazip.com/"));
            if (getUrlTask == await Task.WhenAny(getUrlTask, Task.Delay(3000)).ConfigureAwait(false))
            {
                getUrlResult = await getUrlTask;
            }
            else
            {
                getUrlResult = Result.Fail<string>("HTTP Request for public IP address timed out.");
            }

            if (getUrlResult.Failure)
            {
                return Result.Fail<IPAddress>(getUrlResult.Error);
            }

            var parseIpResult = ParseIPv4Addresses(getUrlResult.Value);

            return parseIpResult.Success
                ? Result.Ok(parseIpResult.Value[0])
                : Result.Fail<IPAddress>(parseIpResult.Error);
        }

        public static Result<IPAddress> GetLocalIPv4Address(string localNetworkCidrIp)
        {
            var acquiredLocalIp = GetLocalIPv4AddressFromInternet();
            if (acquiredLocalIp.Success)
            {
                return Result.Ok(acquiredLocalIp.Value);
            }

            var matchedLocalIp = GetLocalIpAddressWithoutInternet(localNetworkCidrIp);

            return matchedLocalIp.Success
                ? Result.Ok(matchedLocalIp.Value)
                : Result.Fail<IPAddress>("Unable to determine local IP address");
        }

        static Result<IPAddress> GetLocalIPv4AddressFromInternet()
        {
            IPAddress localIp;
            try
            {
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    if (!(socket.LocalEndPoint is IPEndPoint endPoint))
                    {
                        return Result.Fail<IPAddress>($"Error occurred casting {socket.LocalEndPoint} to IPEndPoint");
                    }

                    localIp = endPoint.Address;
                }
            }
            catch (SocketException ex)
            {
                return Result.Fail<IPAddress>($"{ex.Message} {ex.GetType()}");
            }

            return Result.Ok(localIp);
        }

        static Result<IPAddress> GetLocalIpAddressWithoutInternet(string localNetworkCidrIp)
        {
            var localIps = GetLocalIPv4AddressList();
            if (localIps.Count == 1)
            {
                return Result.Ok(localIps[0]);
            }

            foreach (var ip in localIps)
            {
                var checkIp = ip.IsInRange(localNetworkCidrIp);
                if (!checkIp.Success) continue;
                if (!checkIp.Value) continue;

                return Result.Ok(ip);
            }

            return Result.Fail<IPAddress>("Unable to determine local IP address");
        }

        static List<IPAddress> GetLocalIPv4AddressList()
        {
            var localIps = new List<IPAddress>();
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                var ips =
                    nic.GetIPProperties().UnicastAddresses
                        .Select(uni => uni.Address)
                        .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToList();

                localIps.AddRange(ips);
            }

            return localIps;
        }

        public static Result<IPAddress> ParseSingleIPv4Address(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return Result.Fail<IPAddress>("Input string cannot be null");
            }

            var conversion = ConvertIPv4StringToBytes(input);

            return conversion.Success
                ? Result.Ok(new IPAddress(conversion.Value))
                : Result.Fail<IPAddress>(conversion.Error);
        }

        public static Result<List<IPAddress>> ParseIPv4Addresses(string input)
        {
            const string ipV4Pattern =
                @"(?:(?:1\d\d|2[0-5][0-5]|2[0-4]\d|0?[1-9]\d|0?0?\d)\.){3}(?:1\d\d|2[0-5][0-5]|2[0-4]\d|0?[1-9]\d|0?0?\d)";

            if (string.IsNullOrEmpty(input))
            {
                return Result.Fail<List<IPAddress>>("Input string cannot be null");
            }

            var ips = new List<IPAddress>();
            try
            {
                var regex = new Regex(ipV4Pattern);
                foreach (Match match in regex.Matches(input))
                {
                    var parse = ParseSingleIPv4Address(match.Value);
                    if (parse.Failure)
                    {
                        return Result.Fail<List<IPAddress>>(parse.Error);
                    }

                    ips.Add(parse.Value);
                }
            }
            catch (RegexMatchTimeoutException ex)
            {
                return Result.Fail<List<IPAddress>>($"{ex.Message} ({ex.GetType()}) raised in method IpAddressHelper.ParseIPv4Addresses");
            }

            return ips.Count > 0
                ? Result.Ok(ips)
                : Result.Fail<List<IPAddress>>("Input string did not contain any valid IPv4 addresses");
        }

        // true if ipAddress falls inside the range defined by cidrIp, example:
        // bool result = IsInCidrRange("192.168.2.3", "192.168.2.0/24"); // result = true
        public static Result<bool> IpAddressIsInRange(string checkIp, string cidrIp)
        {
            if (string.IsNullOrEmpty(checkIp))
            {
                throw new ArgumentException("Input string must not be null", checkIp);
            }

            var parseIp = ParseIPv4Addresses(checkIp);
            if (parseIp.Failure)
            {
                return Result.Fail<bool>($"Unable to parse IP address from input string {checkIp}");
            }

            return IpAddressIsInRange(parseIp.Value[0], cidrIp);
        }

        public static Result<bool> IpAddressIsInRange(IPAddress checkIp, string cidrIp)
        {
            var cidrIpNull = $"CIDR IP address was null or empty string, {cidrIp}";
            var cidrIpParseError = $"Unable to parse IP address from input string {cidrIp}";
            var cidrIpSplitError = $"cidrIp was not in the correct format:\nExpected: a.b.c.d/n\nActual: {cidrIp}";
            var cidrMaskParseError1 = $"Unable to parse netmask bit count from {cidrIp}";
            const string cidrMaskParseError2 = "Netmask bit count value is invalid, must be in range 0-32";

            if (string.IsNullOrEmpty(cidrIp))
            {
                return Result.Fail<bool>(cidrIpNull);
            }

            var parseIp = ParseIPv4Addresses(cidrIp);
            if (parseIp.Failure)
            {
                return Result.Fail<bool>(cidrIpParseError);
            }

            var cidrAddress = parseIp.Value[0];

            var parts = cidrIp.Split('/');
            if (parts.Length != 2)
            {
                return Result.Fail<bool>(cidrIpSplitError);
            }

            if (!Int32.TryParse(parts[1], out var netmaskBitCount))
            {
                return Result.Fail<bool>(cidrMaskParseError1);
            }

            if (0 > netmaskBitCount || netmaskBitCount > 32)
            {
                return Result.Fail<bool>(cidrMaskParseError2);
            }

            var checkIpBytes = BitConverter.ToInt32(checkIp.GetAddressBytes(), 0);
            var cidrIpBytes = BitConverter.ToInt32(cidrAddress.GetAddressBytes(), 0);
            var cidrMaskBytes = IPAddress.HostToNetworkOrder(-1 << (32 - netmaskBitCount));

            var ipIsInRange = (checkIpBytes & cidrMaskBytes) == (cidrIpBytes & cidrMaskBytes);

            return Result.Ok(ipIsInRange);
        }

        public static Result<bool> IpAddressIsInPrivateAddressSpace(string ipAddress)
        {
            var parseIp = ParseIPv4Addresses(ipAddress);
            if (parseIp.Failure)
            {
                return Result.Fail<bool>($"Unable to parse IP address from {ipAddress}");
            }

            var ipIsInPrivateRange = IpAddressIsInPrivateAddressSpace(parseIp.Value[0]);

            return Result.Ok(ipIsInPrivateRange);
        }

        public static bool IpAddressIsInPrivateAddressSpace(IPAddress ipAddress)
        {
            var inPrivateBlockA = IpAddressIsInRange(ipAddress, CidrPrivateAddressBlockA).Value;
            var inPrivateBlockB = IpAddressIsInRange(ipAddress, CidrPrivateAddressBlockB).Value;
            var inPrivateBlockC = IpAddressIsInRange(ipAddress, CidrPrivateAddressBlockC).Value;

            return inPrivateBlockA || inPrivateBlockB || inPrivateBlockC;
        }

        public static AddressType GetAddressType(IPAddress ipAddress)
        {
            return IpAddressIsInPrivateAddressSpace(ipAddress)
                ? AddressType.Private
                : AddressType.Public;
        }

        public static Result<string> GetCidrIp()
        {
            var platform = Environment.OSVersion.Platform.ToString();

            var ipAddressInfoList = platform.Contains("win", StringComparison.OrdinalIgnoreCase)
                ? GetWindowsUnicastAddressInfoList()
                : GetUnixUnicastAddressInfoList();

            if (ipAddressInfoList.Count == 0)
            {
                const string error =
                    "No IPv4 addresses are assocaited with any network adapters " +
                    "on this machine, unable to determine CIDR IP";

                return Result.Fail<string>(error);
            }

            if (ipAddressInfoList.Count > 1)
            {
                return Result.Fail<string>("More than one ethernet adapters found, unable to determine CIDR IP");
            }

            return platform.Contains("win", StringComparison.OrdinalIgnoreCase)
                ? GetCidrIpFromWindowsIpAddressInformation(ipAddressInfoList[0])
                : GetCidrIpFromUnixIpAddressInformation(ipAddressInfoList[0]);
        }

        public static void DisplayLocalIPv4AddressInfo()
        {
            var platform = Environment.OSVersion.Platform.ToString();

            foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                var adapterProperties = adapter.GetIPProperties();

                var uniCast =
                    adapterProperties.UnicastAddresses
                        .Select(uni => uni)
                        .Where(uni => uni.Address.AddressFamily == AddressFamily.InterNetwork).ToList();

                if (uniCast.Count == 0) continue;

                Console.WriteLine($"Adapter Name.....................: {adapter.Name}");
                Console.WriteLine($"Description......................: {adapter.Description}");
                foreach (var uni in uniCast)
                {
                    Console.WriteLine($"  Unicast Address................: {uni.Address}");
                    Console.WriteLine($"    IPv4 Mask....................: {uni.IPv4Mask}");

                    if (!platform.Contains("win", StringComparison.OrdinalIgnoreCase)) continue;

                    Console.WriteLine($"    Prefix Length................: {uni.PrefixLength}");
                    Console.WriteLine($"    Prefix Origin................: {uni.PrefixOrigin}");
                    Console.WriteLine($"    Suffix Origin................: {uni.SuffixOrigin}");
                    Console.WriteLine($"    Duplicate Address Detection..: {uni.DuplicateAddressDetectionState}");
                    Console.WriteLine($"    DNS Eligible.................: {uni.IsDnsEligible}");
                    Console.WriteLine($"    Transient....................: {uni.IsTransient}");
                }
                Console.WriteLine();
            }
        }

        public static void DisplayIPv4GlobalStatistics()
        {
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            IPGlobalStatistics ipstat = properties.GetIPv4GlobalStatistics();

            Console.WriteLine("  Forwarding enabled ...................... : {0}",
                ipstat.ForwardingEnabled);
            Console.WriteLine("  Interfaces .............................. : {0}",
                ipstat.NumberOfInterfaces);
            Console.WriteLine("  IP addresses ............................ : {0}",
                ipstat.NumberOfIPAddresses);
            Console.WriteLine("  Routes .................................. : {0}",
                ipstat.NumberOfRoutes);
            Console.WriteLine("  Default TTL ............................. : {0}",
                ipstat.DefaultTtl);
            Console.WriteLine("");
            Console.WriteLine("  Inbound Packet Data:");
            Console.WriteLine("      Received ............................ : {0}",
                ipstat.ReceivedPackets);
            Console.WriteLine("      Forwarded ........................... : {0}",
                ipstat.ReceivedPacketsForwarded);
            Console.WriteLine("      Delivered ........................... : {0}",
                ipstat.ReceivedPacketsDelivered);
            Console.WriteLine("      Discarded ........................... : {0}",
                ipstat.ReceivedPacketsDiscarded);
            Console.WriteLine("      Header Errors ....................... : {0}",
                ipstat.ReceivedPacketsWithHeadersErrors);
            Console.WriteLine("      Address Errors ...................... : {0}",
                ipstat.ReceivedPacketsWithAddressErrors);
            Console.WriteLine("      Unknown Protocol Errors ............. : {0}",
                ipstat.ReceivedPacketsWithUnknownProtocol);
            Console.WriteLine("");
            Console.WriteLine("  Outbound Packet Data:");
            Console.WriteLine("      Requested ........................... : {0}",
                 ipstat.OutputPacketRequests);
            Console.WriteLine("      Discarded ........................... : {0}",
                ipstat.OutputPacketsDiscarded);
            Console.WriteLine("      No Routing Discards ................. : {0}",
                ipstat.OutputPacketsWithNoRoute);
            Console.WriteLine("      Routing Entry Discards .............. : {0}",
                ipstat.OutputPacketRoutingDiscards);
            Console.WriteLine("");
            Console.WriteLine("  Reassembly Data:");
            Console.WriteLine("      Reassembly Timeout .................. : {0}",
                ipstat.PacketReassemblyTimeout);
            Console.WriteLine("      Reassemblies Required ............... : {0}",
                ipstat.PacketReassembliesRequired);
            Console.WriteLine("      Packets Reassembled ................. : {0}",
                ipstat.PacketsReassembled);
            Console.WriteLine("      Packets Fragmented .................. : {0}",
                ipstat.PacketsFragmented);
            Console.WriteLine("      Fragment Failures ................... : {0}",
                ipstat.PacketFragmentFailures);
            Console.WriteLine("");
        }

        public static void DisplayIPv4TcpStatistics()
        {
            var properties = IPGlobalProperties.GetIPGlobalProperties();
            var tcpstat = properties.GetTcpIPv4Statistics();

            Console.WriteLine($"  Minimum Transmission Timeout..: {tcpstat.MinimumTransmissionTimeout}");
            Console.WriteLine($"  Maximum Transmission Timeout..: {tcpstat.MaximumTransmissionTimeout}");

            Console.WriteLine($"{Environment.NewLine}  Connection Data:");
            Console.WriteLine($"      Current...................: {tcpstat.CurrentConnections}");
            Console.WriteLine($"      Cumulative................: {tcpstat.CumulativeConnections}");
            Console.WriteLine($"      Initiated.................: {tcpstat.ConnectionsInitiated}");
            Console.WriteLine($"      Accepted..................: {tcpstat.ConnectionsAccepted}");
            Console.WriteLine($"      Failed Attempts...........: {tcpstat.FailedConnectionAttempts}");
            Console.WriteLine($"      Reset.....................: {tcpstat.ResetConnections}");

            Console.WriteLine($"{Environment.NewLine}  Segment Data:");
            Console.WriteLine($"      Received..................: {tcpstat.SegmentsReceived}");
            Console.WriteLine($"      Sent......................: {tcpstat.SegmentsSent}");
            Console.WriteLine($"      Retransmitted.............: {tcpstat.SegmentsResent}");
        }

        public static void DisplayIcmpV4Statistics()
        {
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            IcmpV4Statistics stat = properties.GetIcmpV4Statistics();

            Console.WriteLine("  Messages ............................ Sent: {0,-10}   Received: {1,-10}",
                stat.MessagesSent, stat.MessagesReceived);
            Console.WriteLine("  Errors .............................. Sent: {0,-10}   Received: {1,-10}",
                stat.ErrorsSent, stat.ErrorsReceived);

            Console.WriteLine("  Echo Requests ....................... Sent: {0,-10}   Received: {1,-10}",
                stat.EchoRequestsSent, stat.EchoRequestsReceived);
            Console.WriteLine("  Echo Replies ........................ Sent: {0,-10}   Received: {1,-10}",
                stat.EchoRepliesSent, stat.EchoRepliesReceived);

            Console.WriteLine("  Destination Unreachables ............ Sent: {0,-10}   Received: {1,-10}",
                stat.DestinationUnreachableMessagesSent, stat.DestinationUnreachableMessagesReceived);

            Console.WriteLine("  Source Quenches ..................... Sent: {0,-10}   Received: {1,-10}",
                stat.SourceQuenchesSent, stat.SourceQuenchesReceived);

            Console.WriteLine("  Redirects ........................... Sent: {0,-10}   Received: {1,-10}",
                stat.RedirectsSent, stat.RedirectsReceived);

            Console.WriteLine("  TimeExceeded ........................ Sent: {0,-10}   Received: {1,-10}",
                stat.TimeExceededMessagesSent, stat.TimeExceededMessagesReceived);

            Console.WriteLine("  Parameter Problems .................. Sent: {0,-10}   Received: {1,-10}",
                stat.ParameterProblemsSent, stat.ParameterProblemsReceived);

            Console.WriteLine("  Timestamp Requests .................. Sent: {0,-10}   Received: {1,-10}",
                stat.TimestampRequestsSent, stat.TimestampRequestsReceived);
            Console.WriteLine("  Timestamp Replies ................... Sent: {0,-10}   Received: {1,-10}",
                stat.TimestampRepliesSent, stat.TimestampRepliesReceived);

            Console.WriteLine("  Address Mask Requests ............... Sent: {0,-10}   Received: {1,-10}",
                stat.AddressMaskRequestsSent, stat.AddressMaskRequestsReceived);
            Console.WriteLine("  Address Mask Replies ................ Sent: {0,-10}   Received: {1,-10}",
                stat.AddressMaskRepliesSent, stat.AddressMaskRepliesReceived);
            Console.WriteLine("");
        }

        static List<UnicastIPAddressInformation> GetUnixUnicastAddressInfoList()
        {
            var ethernetAdapters = NetworkInterface.GetAllNetworkInterfaces().Select(nic => nic)
                .Where(nic => nic.Name.StartsWith("en", StringComparison.Ordinal)).ToList();

            var ipV4List = new List<UnicastIPAddressInformation>();
            foreach (var nic in ethernetAdapters)
            {
                var ips =
                    nic.GetIPProperties().UnicastAddresses
                        .Select(ipInfo => ipInfo)
                        .Where(ipInfo => ipInfo.Address.AddressFamily == AddressFamily.InterNetwork).ToList();

                ipV4List.AddRange(ips);
            }

            return ipV4List;
        }

        static List<UnicastIPAddressInformation> GetWindowsUnicastAddressInfoList()
        {
            var ethernetAdapters = NetworkInterface.GetAllNetworkInterfaces().Select(nic => nic)
                .Where(nic => nic.Name.Contains("ethernet", StringComparison.OrdinalIgnoreCase)
                              || nic.Description.Contains("ethernet", StringComparison.OrdinalIgnoreCase)).ToList();

            var ipV4List = new List<UnicastIPAddressInformation>();
            foreach (var nic in ethernetAdapters)
            {
                var ips =
                    nic.GetIPProperties().UnicastAddresses
                        .Select(ipInfo => ipInfo)
                        .Where(ipInfo => ipInfo.Address.AddressFamily == AddressFamily.InterNetwork).ToList();

                ipV4List.AddRange(ips);
            }

            return ipV4List;
        }

        static Result<string> GetCidrIpFromWindowsIpAddressInformation(UnicastIPAddressInformation ipInfo)
        {
            var ipAddress = ipInfo.Address;
            var networkBitCount = ipInfo.PrefixLength;

            return Result.Ok(GetCidrIpFromIpAddressAndNetworkBitCount(ipAddress, networkBitCount));
        }

        static Result<string> GetCidrIpFromUnixIpAddressInformation(UnicastIPAddressInformation ipInfo)
        {
            var getNetworkBitCount = GetNetworkBitCountFromSubnetMask(ipInfo.IPv4Mask);
            if (getNetworkBitCount.Failure)
            {
                return Result.Fail<string>("Unable to determine CIDR IP from available network adapter information");
            }

            var networkBitCount = getNetworkBitCount.Value;

            return Result.Ok(GetCidrIpFromIpAddressAndNetworkBitCount(ipInfo.Address, networkBitCount));
        }

        static Result<int> GetNetworkBitCountFromSubnetMask(IPAddress subnetMask)
        {
            var binaryArray = ConvertIpAddressToBinary(subnetMask, false).ToCharArray();

            if (binaryArray.Length == 0 || binaryArray.Length != 32)
            {
                return Result.Fail<int>("Binary string was not in the expected format.");
            }

            if (binaryArray[0] != '1')
            {
                return Result.Fail<int>("Binary string was not in the expected format.");
            }

            var onesCount = 0;
            var zerosCount = 0;
            var firstZeroEncountered = false;

            foreach (var bit in binaryArray)
            {
                if (!firstZeroEncountered)
                {
                    if (bit == '1')
                    {
                        onesCount++;
                    }

                    if (bit == '0')
                    {
                        firstZeroEncountered = true;
                        zerosCount++;
                    }
                }
                else
                {
                    if (bit == '1')
                    {
                        break;
                    }

                    if (bit == '0')
                    {
                        zerosCount++;
                    }
                }
            }

            var totalBits = onesCount + zerosCount;
            if (totalBits != 32)
            {
                return Result.Fail<int>("Binary string was not in the expected format.");
            }

            return Result.Ok(onesCount);
        }

        static string GetCidrIpFromIpAddressAndNetworkBitCount(IPAddress address, int networkBitCount)
        {
            var ipAddressBytes = address.GetAddressBytes();
            var networkIdBytes = new byte[4];

            foreach (var i in Enumerable.Range(0, ipAddressBytes.Length))
            {
                var byteArray = Convert.ToString(ipAddressBytes[i], 2).PadLeft(8, '0').ToCharArray();
                foreach (var j in Enumerable.Range(0, byteArray.Length))
                {
                    var bitNumber = i * 8 + j + 1;
                    if (bitNumber > networkBitCount)
                    {
                        byteArray[j] = '0';
                    }
                }

                var byteString = new string(byteArray);
                networkIdBytes[i] = Convert.ToByte(byteString, 2);
            }

            var networkId = new IPAddress(networkIdBytes);
            return $"{networkId}/{networkBitCount}";
        }

        public static Result<string> ConvertIpAddressToBinary(string ip, bool separateBytes)
        {
            var parseResult = ParseIPv4Addresses(ip);
            if (parseResult.Failure)
            {
                return Result.Fail<string>(parseResult.Error);
            }

            var binary = ConvertIpAddressToBinary(parseResult.Value[0], separateBytes);

            return Result.Ok(binary);
        }

        public static string ConvertIpAddressToBinary(IPAddress ip, bool separateBytes)
        {
            var bytes = ip.GetAddressBytes();
            var s = string.Empty;

            foreach (var i in Enumerable.Range(0, bytes.Length))
            {
                var oneByte = Convert.ToString(bytes[i], 2).PadLeft(8, '0');
                s += oneByte;

                if (!separateBytes) continue;

                if (!i.IsLastIteration(bytes.Length))
                {
                    s += " - ";
                }
            }

            return s;
        }

        public static IpAddressSimilarity CompareTwoIpAddresses(IPAddress ip1, IPAddress ip2)
        {
            var ip1Bytes = ip1.GetAddressBytes();
            var ip2Bytes = ip2.GetAddressBytes();

            if (ip1Bytes[0] != ip2Bytes[0]) return IpAddressSimilarity.None;
            if (ip1Bytes[1] != ip2Bytes[1]) return IpAddressSimilarity.OnlyFirstByteMatches;
            if (ip1Bytes[2] != ip2Bytes[2]) return IpAddressSimilarity.FirstTwoBytesMatch;

            return ip1Bytes[3] != ip2Bytes[3]
                ? IpAddressSimilarity.FirstThreeBytesMatch
                : IpAddressSimilarity.AllBytesMatch;
        }

        static Result<byte[]> ConvertIPv4StringToBytes(string ipv4)
        {
            var digits = ipv4.Trim().Split('.').ToList();
            if (digits.Count != 4)
            {
                return Result.Fail<byte[]>($"Unable to parse IPv4 address from string: {ipv4}");
            }

            var bytes = new byte[4];
            foreach (var i in Enumerable.Range(0, digits.Count))
            {
                if (!int.TryParse(digits[i], out var parsedInt))
                {
                    return Result.Fail<byte[]>($"Unable to parse IPv4 address from string: {ipv4}");
                }

                if (0 > parsedInt || parsedInt > 255)
                {
                    return Result.Fail<byte[]>($"{parsedInt} not within required IP address range [0,255]");
                }

                bytes[i] = (byte)parsedInt;
            }

            return Result.Ok(bytes);
        }
    }
}