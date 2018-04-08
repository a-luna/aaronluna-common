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

    using Numeric;
	using Enums;
    using Http;
    using Result;
    
    public static class Network
    {
        public const string CidrPrivateAddressBlockA = "10.0.0.0/8";
        public const string CidrPrivateAddressBlockB = "172.16.0.0/12";
        public const string CidrPrivateAddressBlockC = "192.168.0.0/16";

        public static async Task<Result<IPAddress>> GetPublicIPv4AddressAsync()
        {
            var getUrlResult = await HttpHelper.GetUrlContentAsStringAsync("http://ipv4.icanhazip.com/").ConfigureAwait(false);

            return getUrlResult.Success
                ? ParseSingleIPv4Address(getUrlResult.Value)
                : Result.Fail<IPAddress>(getUrlResult.Error);
        }

        public static Result<IPAddress> GetLocalIpAddress(string cidrMask)
        {
            const string localIpError =
                "Unable to determine the local IP address for this machine, " +
                "please ensure that the CIDR mask is correct for your LAN. " +
                "For example, the correct CIDR mask for IP address 192.168.2.3 " +
                "would be a string value of \"192.168.2.0/24\"";

            var acquiredLocalIp = GetLocalIPv4AddressFromInternet();
            if (acquiredLocalIp.Success)
            {
                return Result.Ok(acquiredLocalIp.Value);
            }

            var localIps = GetLocalIPv4AddressList();
            if (localIps.Count == 1)
            {
                return Result.Ok(localIps[0]);
            }

            foreach (var ip in localIps)
            {
                var result = IpAddressIsInCidrRange(ip, cidrMask);
                if (!result.Success) continue;
                if (!result.Value) continue;

                return Result.Ok(ip);
            }

            return Result.Fail<IPAddress>(localIpError);
        }
        
        public static Result<IPAddress> GetLocalIPv4AddressFromInternet()
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
                : Result.Fail<List<IPAddress>>("Input string did not contain any valid IPv4 addreses");
        }

		public static Result<string> ConvertIpAddressToBinary(string ip)
		{
			var parseResult = ParseSingleIPv4Address(ip);
            if (parseResult.Failure)
			{
				return Result.Fail<string>(parseResult.Error);
			}

			var binary = ConvertIpAddressToBinary(parseResult.Value);

			return Result.Ok(binary);
		}

		public static string ConvertIpAddressToBinary(IPAddress ip)
        {
            var bytes = ip.GetAddressBytes();
            var s = string.Empty;

            foreach (var i in Enumerable.Range(0, bytes.Length))
            {
                var oneByte = Convert.ToString(bytes[i], 2).PadLeft(8, '0');
				s += oneByte.Insert(4, " ");

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

        // true if ipAddress falls inside the CIDR range, example
        // bool result = IsInCidrRange("192.168.2.3", "192.168.2.0/24"); // result = true
        public static Result<bool> IpAddressIsInCidrRange(string checkIp, string cidrMask)
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

            return IpAddressIsInCidrRange(parseIp.Value[0], cidrMask);
        }

        public static Result<bool> IpAddressIsInCidrRange(IPAddress checkIp, string cidrMask)
        {
            if (string.IsNullOrEmpty(cidrMask))
            {
                return Result.Fail<bool>($"CIDR mask was null or empty string, {cidrMask}");
            }

            var parts = cidrMask.Split('/');
            if (parts.Length != 2)
            {
                return Result.Fail<bool>(
                    $"cidrMask was not in the correct format:\nExpected: a.b.c.d/n\nActual: {cidrMask}");
            }

            var parseIp = ParseIPv4Addresses(cidrMask);
            if (parseIp.Failure)
            {
                return Result.Fail<bool>($"Unable to parse IP address from input string {cidrMask}");
            }

            var cidrAddress = parseIp.Value[0];

            if (!Int32.TryParse(parts[1], out var netmaskBitCount))
            {
                return Result.Fail<bool>($"Unable to parse netmask bit count from {cidrMask}");
            }

            if (0 > netmaskBitCount || netmaskBitCount > 32)
            {
                return Result.Fail<bool>($"Netmask bit count value of {netmaskBitCount} is invalid, must be in range 0-32");
            }

            var ipAddressBytes = BitConverter.ToInt32(checkIp.GetAddressBytes(), 0);
            var cidrAddressBytes = BitConverter.ToInt32(cidrAddress.GetAddressBytes(), 0);
            var cidrMaskBytes = IPAddress.HostToNetworkOrder(-1 << (32 - netmaskBitCount));

            var ipIsInRange = (ipAddressBytes & cidrMaskBytes) == (cidrAddressBytes & cidrMaskBytes);

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
            var inPrivateBlockA = IpAddressIsInCidrRange(ipAddress, CidrPrivateAddressBlockA).Value;
            var inPrivateBlockB = IpAddressIsInCidrRange(ipAddress, CidrPrivateAddressBlockB).Value;
            var inPrivateBlockC = IpAddressIsInCidrRange(ipAddress, CidrPrivateAddressBlockC).Value;

            return inPrivateBlockA || inPrivateBlockB || inPrivateBlockC;
        }

        public static List<IPAddress> GetLocalIPv4AddressList()
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

        public static void DisplayLocalIPv4AddressInfo()
        {
            foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                var adapterProperties = adapter.GetIPProperties();

                var uniCast =
                    adapterProperties.UnicastAddresses
                        .Select(uni => uni)
                        .Where(uni => uni.Address.AddressFamily == AddressFamily.InterNetwork).ToList();

                if (uniCast.Count > 0)
                {
                    Console.WriteLine($"Adapter Name....................: {adapter.Name}");
                    Console.WriteLine($"Description.....................: {adapter.Description}");
                    foreach (var uni in uniCast)
                    {
                        Console.WriteLine($"  Unicast Address...............: {uni.Address}");
                        Console.WriteLine($"    IPv4 Mask...................: {uni.IPv4Mask}");
                        Console.WriteLine($"    Prefix Length...............: {uni.PrefixLength}");
                        Console.WriteLine($"    Prefix Origin...............: {uni.PrefixOrigin}");
                        Console.WriteLine($"    Suffix Origin...............: {uni.SuffixOrigin}");
                        Console.WriteLine($"    Duplicate Address Detection : {uni.DuplicateAddressDetectionState}");
                        Console.WriteLine($"    DNS Eligible................: {uni.IsDnsEligible}");
                        Console.WriteLine($"    Transient...................: {uni.IsTransient}");
                    }
                    Console.WriteLine();
                }
            }
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