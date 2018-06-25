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

        public static async Task<Result<IPAddress>> GetPublicIPv4AddressAsync()
        {
            Result<string> getUrlResult;

            var getUrlTask = Task.Run(() => HttpHelper.GetUrlContentAsStringAsync("http://ipv4.icanhazip.com/"));
            if (getUrlTask == await Task.WhenAny(getUrlTask, Task.Delay(3000)))
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
                var result = IpAddressIsInRange(ip, localNetworkCidrIp);
                if (!result.Success) continue;
                if (!result.Value) continue;

                return Result.Ok(ip);
            }

            return Result.Fail<IPAddress>(string.Empty);
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
            var cidrMaskParseError2 = $"Netmask bit count value is invalid, must be in range 0-32";

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

                Console.WriteLine($"Adapter Name....................: {adapter.Name}");
                Console.WriteLine($"Description.....................: {adapter.Description}");
                foreach (var uni in uniCast)
                {
                    Console.WriteLine($"  Unicast Address...............: {uni.Address}");
                    Console.WriteLine($"    IPv4 Mask...................: {uni.IPv4Mask}");
                    if (!platform.Contains("win", StringComparison.OrdinalIgnoreCase)) continue;

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

        public static Result<string> AttemptToDetermineLanCidrIp()
        {
            var platform = Environment.OSVersion.Platform.ToString();

            var ipAddressInfoList = platform.Contains("win", StringComparison.OrdinalIgnoreCase)
                ? GetWindowsUnicastAddressInfoList()
                : GetUnixUnicastAddressInfoList();

            if (ipAddressInfoList.Count == 0)
            {
                return Result.Fail<string>("No ethernet adapters found, unable to determine CIDR IP");
            }

            if (ipAddressInfoList.Count > 1)
            {
                return Result.Fail<string>("More than one ethernet adapters found, unable to determine CIDR IP");
            }

            return platform.Contains("win", StringComparison.OrdinalIgnoreCase)
                ? GetCidrIpFromWindowsIpAddressInformation(ipAddressInfoList[0])
                : GetCidrIpFromUnixIpAddressInformation(ipAddressInfoList[0]);

        }

        static List<UnicastIPAddressInformationCollection> GetUnixUnicastAddressInfoList()
        {
            return NetworkInterface.GetAllNetworkInterfaces().Select(nic => nic)
                .Where(nic => nic.Name.StartsWith("en", StringComparison.Ordinal))
                .Select(nic => nic.GetIPProperties().UnicastAddresses)
                .Where(uni => uni.Any()).ToList();
        }

        static List<UnicastIPAddressInformationCollection> GetWindowsUnicastAddressInfoList()
        {
            return NetworkInterface.GetAllNetworkInterfaces().Select(a => a)
                .Where(a => a.Name.Contains("ethernet", StringComparison.OrdinalIgnoreCase) ||
                            a.Description.Contains("ethernet", StringComparison.OrdinalIgnoreCase))
                .Select(nic => nic.GetIPProperties().UnicastAddresses)
                .Where(uni => uni.Any()).ToList();
        }
        
        static Result<string> GetCidrIpFromWindowsIpAddressInformation(UnicastIPAddressInformationCollection addressInfoCollection)
        {
            var ipInfoList = addressInfoCollection.Select(uni => uni)
                .Where(uni => uni.Address.AddressFamily == AddressFamily.InterNetwork).ToList();

            if (ipInfoList.Count == 0 || ipInfoList.Count > 1)
            {
                return Result.Fail<string>("Unable to determine CIDR IP from available network adapter information");
            }

            var ipAddress = ipInfoList[0].Address;
            var networkBitCount = ipInfoList[0].PrefixLength;

            return Result.Ok(GetCidrIpFromIpAddressAndNetworkBitCount(ipAddress, networkBitCount));
        }

        static Result<string> GetCidrIpFromUnixIpAddressInformation(
            UnicastIPAddressInformationCollection addressInfoCollection)
        {
            if (addressInfoCollection.Count == 0 || addressInfoCollection.Count > 1)
            {
                return Result.Fail<string>("Unable to determine CIDR IP from available network adapter information");
            }

            var addressInfo = addressInfoCollection[0];
            var parseIpv4 = ParseSingleIPv4Address(addressInfo.Address.ToString());
            if (parseIpv4.Failure)
            {
                return Result.Fail<string>("Unable to determine CIDR IP from available network adapter information");
            }

            var ipAddress = parseIpv4.Value;

            var parseIpMask = ParseSingleIPv4Address(addressInfo.IPv4Mask.ToString());
            if (parseIpMask.Failure)
            {
                return Result.Fail<string>("Unable to determine CIDR IP from available network adapter information");
            }

            var ipMask = parseIpMask.Value;
            var getNetworkBitCount = GetNetworkBitCountFromSubnetMask(ipMask);

            if (getNetworkBitCount.Failure)
            {
                return Result.Fail<string>("Unable to determine CIDR IP from available network adapter information");
            }

            var networkBitCount = getNetworkBitCount.Value;

            return Result.Ok(GetCidrIpFromIpAddressAndNetworkBitCount(ipAddress, networkBitCount));
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
            var cidrIp = $"{networkId}/{networkBitCount}";

            return cidrIp;
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
                s += oneByte.Insert(4, " ");

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