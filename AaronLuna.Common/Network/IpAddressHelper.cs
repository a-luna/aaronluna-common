namespace AaronLuna.Common.Network
{
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
                if (!int.TryParse(digits[i], out int parsedInt))
                {
                    return Result.Fail<byte[]>($"Unable to parse IPv4 address from string: {ipv4}");
                }

                bytes[i] = (byte) parsedInt;
            }

            return Result.Ok(bytes);
        }
    }
}