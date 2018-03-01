using System;

namespace AaronLuna.Common.Network
{
    using Enums;
    using Http;
    using Result;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public static class IpAddressHelper
    {
        public const string CidrBlockClassA = "10.0.0.0/8";
        public const string CidrBlockClassB = "172.16.0.0/12";
        public const string CidrBlockClassC = "192.168.0.0/16";

        const string Pattern =
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

        public static IPAddress GetLocalIPv4Address()
        {
            var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            var ipAddress = ipHostInfo.AddressList.Select(ip => ip)
                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

            return ipAddress;
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
                var regex = new Regex(Pattern);
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

            return ips.Any() ? Result.Ok(ips) : Result.Fail<List<IPAddress>>("Input string did not contain any valid IPv4 addreses");
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
        // bool result = IsInRange("192.168.2.3", ""192.168.2.0");
        public static Result<bool> IsInCidrRange(string ipAddress, string CIDRmask)
        {
            if (string.IsNullOrEmpty(ipAddress))
            {
                return Result.Fail<bool>($"IP address was null or empty string, {ipAddress}");
            }

            if (string.IsNullOrEmpty(CIDRmask))
            {
                return Result.Fail<bool>($"CIDR mask was null or empty string, {CIDRmask}");
            }

            var parts = CIDRmask.Split('/');
            if (parts.Length != 2)
            {
                return Result.Fail<bool>(
                    $"CIDRmask was not in the correct format:\nExpected: a.b.c.d/n\nActual: {CIDRmask}");
            }

            bool ipIsInRange;
            try
            {
                var IP_addr = BitConverter.ToInt32(IPAddress.Parse(parts[0]).GetAddressBytes(), 0);
                var CIDR_addr = BitConverter.ToInt32(IPAddress.Parse(ipAddress).GetAddressBytes(), 0);
                var CIDR_mask = IPAddress.HostToNetworkOrder(-1 << (32 - int.Parse(parts[1])));

                ipIsInRange = ((IP_addr & CIDR_mask) == (CIDR_addr & CIDR_mask));
            }
            catch (Exception ex)
            {
                return Result.Fail<bool>($"{ex.Message} ({ex.GetType()})");
            }
            
            return Result.Ok(ipIsInRange);
        }

        public static bool IsLocalIpAddress(string ipAddress)
        {
            var parse = ParseSingleIPv4Address(ipAddress);
            if (parse.Failure)
            {
                return false;
            }

            var checkRangeA = IsInCidrRange(ipAddress, CidrBlockClassA);
            var checkRangeB = IsInCidrRange(ipAddress, CidrBlockClassB);
            var checkRangeC = IsInCidrRange(ipAddress, CidrBlockClassC);

            return checkRangeA.Value || checkRangeB.Value || checkRangeC.Value;
        }

        private static Result<byte[]> ConvertIPv4StringToBytes(string ipv4)
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

                bytes[i] = (byte) parsedInt;
            }

            return Result.Ok(bytes);
        }
    }
}