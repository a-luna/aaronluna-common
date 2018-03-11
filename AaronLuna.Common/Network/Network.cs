namespace AaronLuna.Common.Network
{
    using Enums;
    using Http;
    using Result;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public static class Network
    {
        public const string CidPrivateBlockClassA = "10.0.0.0/8";
        public const string CidrPrivateBlockClassB = "172.16.0.0/12";
        public const string CidrPrivateBlockClassC = "192.168.0.0/16";

        const string IPv4Pattern =
            @"((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)";

        public static async Task<Result<IPAddress>> GetPublicIPv4AddressAsync()
        {
            var getUrlResult = await HttpHelper.GetUrlContentAsStringAsync("http://ipv4.icanhazip.com/").ConfigureAwait(false);
            if (getUrlResult.Failure)
            {
                return Result.Fail<IPAddress>(getUrlResult.Error);
            }

            var parse = ParseSingleIPv4Address(getUrlResult.Value);

            return parse.Success
                ? Result.Ok(parse.Value)
                : Result.Fail<IPAddress>(parse.Error);
        }

        public static List<IPAddress> GetLocalIPv4AddressList()
        {
            var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());

            var ips =
                ipHostInfo.AddressList.Select(ip => ip)
                    .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToList();

            return ips;
        }

        public static Result<IPAddress> GetLocalIPv4AddressFromInternet()
        {
            IPAddress localIp;
            try
            {
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    var endPoint = socket.LocalEndPoint as IPEndPoint;
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

        public static Result<List<IPAddress>> ParseAllIPv4Addresses(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return Result.Fail<List<IPAddress>>("Input string cannot be null");
            }

            var ips = new List<IPAddress>();
            try
            {
                var regex = new Regex(IPv4Pattern);
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
                return Result.Fail<List<IPAddress>>($"{ex.Message} ({ex.GetType()}) raised in method IpAddressHelper.ParseAllIPv4Addresses");
            }

            return ips.Count > 0 ? Result.Ok(ips) : Result.Fail<List<IPAddress>>("Input string did not contain any valid IPv4 addreses");
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
        // bool result = IsInCidrRange("192.168.2.3", "192.168.2.0/24");
        public static Result<bool> IpAddressIsInCidrRange(string ipAddress, string cidrMask)
        {
            if (string.IsNullOrEmpty(ipAddress))
            {
                return Result.Fail<bool>($"IP address was null or empty string, {ipAddress}");
            }

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

            var cidrAddress = parts[0];
            var cidrNetworkBitCount = parts[1];
            bool ipIsInRange;

            try
            {
                var ipAddressBytes = BitConverter.ToInt32(IPAddress.Parse(cidrAddress).GetAddressBytes(), 0);
                var cidrAddressBytes = BitConverter.ToInt32(IPAddress.Parse(ipAddress).GetAddressBytes(), 0);
                var cidrMaskBytes = IPAddress.HostToNetworkOrder(-1 << (32 - int.Parse(cidrNetworkBitCount)));

                ipIsInRange = (ipAddressBytes & cidrMaskBytes) == (cidrAddressBytes & cidrMaskBytes);
            }
            catch (Exception ex)
            {
                return Result.Fail<bool>($"{ex.Message} ({ex.GetType()})");
            }

            return Result.Ok(ipIsInRange);
        }

        public static bool IpAddressIsInPrivateAddressSpace(string ipAddress)
        {
            var parse = ParseSingleIPv4Address(ipAddress);
            if (parse.Failure)
            {
                return false;
            }

            var checkRangeA = IpAddressIsInCidrRange(ipAddress, CidPrivateBlockClassA);
            var checkRangeB = IpAddressIsInCidrRange(ipAddress, CidrPrivateBlockClassB);
            var checkRangeC = IpAddressIsInCidrRange(ipAddress, CidrPrivateBlockClassC);

            return checkRangeA.Value || checkRangeB.Value || checkRangeC.Value;
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

                bytes[i] = (byte)parsedInt;
            }

            return Result.Ok(bytes);
        }
    }
}